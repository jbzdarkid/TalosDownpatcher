using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  public class DepotManager {
    private readonly ManifestData manifestData;

    private readonly string depotLocation;
    private static readonly object downloadLock = new object();
    private static readonly object versionLock = new object();

    public DepotManager() {
      manifestData = new ManifestData();
      string steamInstall = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", "C:/Program Files (x86)/Steam");
      depotLocation = $"{steamInstall}/steamapps/content/app_257510";
    }

    public void SetActiveVersion(VersionUIComponent component, Action onSetActiveVersion) {
      Contract.Requires(component != null && onSetActiveVersion != null);
      string activeVersionLocation = Settings.Default.activeVersionLocation;
      string oldVersionLocation = Settings.Default.oldVersionLocation;

      component.State = VersionState.CopyPending;
      lock (versionLock) {
        if (component.version == Settings.Default.activeVersion) {
          component.State = VersionState.Active;
          return;
        }
        Logging.Log($"Changing active version from {Settings.Default.activeVersion} to {component.version}");
        component.State = VersionState.Copying;
        onSetActiveVersion();

        // Carefully clean target folder before copying
        try {
          Directory.Delete(activeVersionLocation, true);
        } catch (DirectoryNotFoundException) {
          Logging.Log("Caught DirectoryNotFoundException: Folder already deleted");
        } catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException) {
          Logging.MessageBox($"Unable to clear {activeVersionLocation}, please ensure that nothing is using it.", "Folder in use");
          component.State = VersionState.Downloaded;
          Settings.Default.activeVersion = 0; // Reset active version, since we're now in a bad state
          Settings.Default.Save();
          return;
        }
        Logging.Log("Deletion successful");

        // Copy the x86 binaries to the x64 folder. They may be overwritten by the next copy operation if there are real x64 binaries.
        CopyAndOverwrite($"{oldVersionLocation}/{component.version}/Bin", $"{activeVersionLocation}/Bin/x64", null);

        double totalSize = manifestData.GetTotalDownloadSize(component.version);
        long copied = 0;
        Action<long> progress = delegate (long fileSize) {
          copied += fileSize;
          component.SetProgress(copied / totalSize);
        };
        CopyAndOverwrite(GetFolder(component.version, Package.Main), activeVersionLocation, progress);
        CopyAndOverwrite(GetFolder(component.version, Package.Gehenna), activeVersionLocation, progress);
        CopyAndOverwrite(GetFolder(component.version, Package.Prototype), activeVersionLocation, progress);
        component.State = VersionState.Active;

        Settings.Default.activeVersion = component.version;
        Settings.Default.Save();
      }
    }
    
    public void DownloadDepots(VersionUIComponent component) {
      Contract.Requires(component != null);

      Logging.Log($"Downloading depots for {component.version}");
      var drive = new DriveInfo(new DirectoryInfo(depotLocation).Root.FullName);
      if (!drive.IsReady) {
        Logging.MessageBox($"Steam install location is in drive {drive.Name}, which is unavailable.", "Drive unavailable");
        return;
      }

      double totalDownloadSize = manifestData.GetTotalDownloadSize(component.version);
      long freeSpace = drive.TotalFreeSpace;
      if (drive.TotalFreeSpace < totalDownloadSize) {
        Logging.MessageBox($@"Steam install location is in drive {drive.Name}
has {Math.Round(freeSpace / 1000000000.0, 1)} GB of free space
but {Math.Round(totalDownloadSize / 1000000000.0, 1)} GB are required.", "Not enough space");
        return;
      }

      component.State = VersionState.DownloadPending; // Pending until we lock
      lock (downloadLock) {
        component.State = VersionState.Downloading;
        SteamCommand.OpenConsole();

        // foreach (var depot in ManifestData.depots) {
        //   SteamCommand.DownloadDepot(depot, manifestData[component.version, depot].manifest);
        // }
        if (Settings.Default.ownsGehenna) {
          SteamCommand.DownloadDepot(ManifestData.GEHENNA, manifestData[component.version, ManifestData.GEHENNA].manifest);
        }
        if (Settings.Default.ownsPrototype) {
          SteamCommand.DownloadDepot(ManifestData.PROTOTYPE, manifestData[component.version, ManifestData.PROTOTYPE].manifest);
        }
        MainWindow.SetForeground();

        Thread.Sleep(5000); // Extra sleep to avoid a race condition where we check for depots before they're actually cleared.
        double downloadFraction = 0.0;
        while (downloadFraction < 1.0) {
          Logging.Log($"Gehenna: {GetFolderSize($"{depotLocation}/depot_{ManifestData.GEHENNA}")}");
          Logging.Log($"Prototype: {GetFolderSize($"{depotLocation}/depot_{ManifestData.PROTOTYPE}")}");

          Thread.Sleep(1000);
          downloadFraction = GetDownloadFraction(component.version, Location.Depots);
          component.SetProgress(0.8 * downloadFraction); // 80% - Downloading
        }
        component.State = VersionState.Saving;

        long copied = 0;
        Action<long> progress = delegate (long fileSize) {
          copied += fileSize;
          component.SetProgress(0.8 + 0.2 * (copied / totalDownloadSize)); // 20% - Copying
        };
        foreach (var depot in ManifestData.depots) {
          CopyAndOverwrite($"{depotLocation}/depot_{depot}", GetFolder(component.version, Package.Main), progress);
        }
        CopyAndOverwrite($"{depotLocation}/depot_{ManifestData.GEHENNA}", GetFolder(component.version, Package.Gehenna), progress);
        CopyAndOverwrite($"{depotLocation}/depot_{ManifestData.PROTOTYPE}", GetFolder(component.version, Package.Prototype), progress);

        if (drive.TotalFreeSpace < 5 * totalDownloadSize) {
          Logging.Log("Low on disk space, clearing download directory");
          Directory.Delete(depotLocation, true);
        }
      }
      component.State = VersionState.Downloaded;
    }

    // *** Utilities ***

    public bool IsFullyDownloaded(int version, Package package) {
      long expectedSize = manifestData.GetDownloadSize(version, package);
      long actualSize = GetFolderSize(GetFolder(version, package));
      Logging.Log($"Package {package} for version {version} is {actualSize} bytes, expected {expectedSize}");

      return expectedSize == actualSize;
    }

    private static string GetFolder(int version, Package package) {
      string folder = $"{Settings.Default.oldVersionLocation}/{version}";
      if (package == Package.Main) return folder;
      else if (package == Package.Gehenna) return folder + "_Gehenna";
      else if (package == Package.Prototype) return folder + "_Prototype";
      else {
        Debug.Assert(false);
        return "";
      }
    }

    public enum Location {
      Depots,
      Active
    };

    // @Bug: This is still wrong, I think.
    public double GetDownloadFraction(int version, Location location) {
      long actualSize = 0;
      switch (location) {
        case Location.Depots:
          foreach (var depot in ManifestData.depots) actualSize += GetFolderSize($"{depotLocation}/depot_{depot}");
          break;
        case Location.Active:
          actualSize += GetFolderSize($"{Settings.Default.activeVersionLocation}");
          break;
      }

      // If this metric is not accurate, I can potentially improve it using the number of files.
      return actualSize / (double)manifestData.GetTotalDownloadSize(version);
    }

    private static long GetFolderSize(string folder) {
      long size = 0;
      var src = new DirectoryInfo(folder);
      if (src.Exists) {
        var files = src.GetFiles("*", SearchOption.AllDirectories);
        Logging.Log($"Actual file count: {files.Length}");
        foreach (var file in files) size += file.Length;
      }
      return size;
    }

    private static void CopyAndOverwrite(string srcFolder, string dstFolder, Action<long> onCopyBytes) {
      var src = new DirectoryInfo(srcFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(dstFolder);
      if (!dst.Exists) Directory.CreateDirectory(dstFolder);

      foreach (var dir in src.GetDirectories()) CopyAndOverwrite($"{srcFolder}/{dir}", $"{dstFolder}/{dir}", onCopyBytes);
      foreach (var file in src.GetFiles()) FCopy(file.FullName, $"{dst}/{file.Name}", onCopyBytes);
    }

    /// <summary>
    /// Fast file move with big buffers
    /// Author: Frank T. Clark
    /// Downloaded from: https://www.codeproject.com/Tips/777322/A-Faster-File-Copy
    /// License: CPOL
    /// Modifications:
    ///   - Formatted code
    ///   - Removed trailing "Delete"
    ///   - Renamed variables
    ///   - Changed from move to copy (overwrite implied)
    ///   - Add callback for progress indicator
    /// </summary>
    /// <param name="source">Source file path</param> 
    /// <param name="destination">Destination file path</param> 
    /// <param name="onCopyBytes">Callback to fire after copying bytes (used for progress bars)</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1005:Delegate invocation can be simplified.", Justification="The if check is relevant, as onCopyBytes may be null.")]
    static void FCopy(string source, string destination, Action<long> onCopyBytes) {
      int buffSize = (1 << 20); // 1 MB
      byte[] buff = new byte[buffSize];
      using (FileStream fsread = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None, buffSize)) {
        using (BinaryReader bwread = new BinaryReader(fsread)) {
          using (FileStream fswrite = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, buffSize)) {
            using (BinaryWriter bwwrite = new BinaryWriter(fswrite)) {
              for (;;) {
                int readBytes = bwread.Read(buff, 0, buffSize);
                if (readBytes == 0) break;
                bwwrite.Write(buff, 0, readBytes);
                if (onCopyBytes != null) onCopyBytes(readBytes);
              }
            }
          }
        }
      }
    }
  }
}
