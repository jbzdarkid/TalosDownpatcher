using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  public class DepotManager {
    private readonly ManifestData manifestData;

    private readonly object downloadLock = new object();
    private readonly object versionLock = new object();

    public DepotManager() {
      manifestData = new ManifestData();
    }

    public void SetActiveVersion(VersionUIComponent component, Action onSetActiveVersion) {
      var thread = new Thread(() => { SetActiveVersionInternal(component, onSetActiveVersion); });
      thread.IsBackground = true;
      thread.Start();
    }

    private void SetActiveVersionInternal(VersionUIComponent component, Action onSetActiveVersion) {
      Contract.Requires(component != null && onSetActiveVersion != null);
      string activeVersionLocation = Settings.Default.activeVersionLocation;

      component.State = VersionState.CopyPending;
      lock (versionLock) {
        if (component.version == Settings.Default.activeVersion) {
          component.State = VersionState.Active;
          return;
        }
        Logging.Log($"Changing active version from {Settings.Default.activeVersion} to {component.version}");
        component.State = VersionState.Copying;
        onSetActiveVersion(); // Sets the previously active version to Downloaded

        // Reset active version for the duration of the copy, since any failure will leave us in a bad state.
        Settings.Default.activeVersion = 0;
        Settings.Default.Save();

        // Clean target folder before copying
        try {
          Directory.Delete(activeVersionLocation, true);
        } catch (DirectoryNotFoundException) {
          Logging.Log("Caught DirectoryNotFoundException: Folder already deleted");
        } catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException) {
          Logging.MessageBox($"Unable to clear {activeVersionLocation}, please ensure that nothing is using it.", "Folder in use");
          component.State = VersionState.Downloaded;
          return;
        }
        Logging.Log("Deletion successful");

        // Copy the x86 binaries to the x64 folder. They may be overwritten by the next copy operation if there are real x64 binaries.
        CopyAndOverwrite(GetFolder(component.version, Package.Main) + "/Bin", activeVersionLocation + "/Bin/x64");

        List<Package> requiredPackages = new List<Package> { Package.Main };
        if (Settings.Default.ownsGehenna) requiredPackages.Add(Package.Gehenna);
        if (Settings.Default.ownsPrototype) requiredPackages.Add(Package.Prototype);

        double totalSize = 0;
        foreach (var package in requiredPackages) totalSize += GetFolderSize(GetFolder(component.version, package));

        // @Performance Multithreading *may* save time here. I doubt it.
        long copied = 0;
        foreach (var package in requiredPackages) {
          CopyAndOverwrite(GetFolder(component.version, package), activeVersionLocation, delegate (long fileSize) {
            copied += fileSize;
            component.SetProgress(copied / totalSize);
          });
        }

        Settings.Default.activeVersion = component.version;
        Settings.Default.Save();

        component.State = VersionState.Active;
      }
    }

    public void DownloadDepots(VersionUIComponent component) {
      var thread = new Thread(() => { DownloadDepotsInternal(component); });
      thread.IsBackground = true;
      thread.Start();
    }

    private void DownloadDepotsInternal(VersionUIComponent component) {
      Contract.Requires(component != null);
      component.State = VersionState.DownloadPending; // Pending until we lock
      lock (downloadLock) {
        int version = component.version;

        Logging.Log($"Downloading depots for {version}");
        var drive = new DriveInfo(new DirectoryInfo(ManifestData.DepotLocation).Root.FullName);
        if (!drive.IsReady) {
          Logging.MessageBox($"Steam install location is in drive {drive.Name}, which is unavailable.", "Drive unavailable");
          return;
        }

        var neededManifests = new List<SteamManifest>();
        if (!IsFullyDownloaded(version, Package.Main)) {
          neededManifests.AddRange(manifestData[version, Package.Main]);
        }
        if (Settings.Default.ownsGehenna && !IsFullyDownloaded(version, Package.Gehenna)) {
          neededManifests.AddRange(manifestData[version, Package.Gehenna]);
        }
        if (Settings.Default.ownsPrototype && !IsFullyDownloaded(version, Package.Prototype)) {
          neededManifests.AddRange(manifestData[version, Package.Prototype]);
        }

        double totalDownloadSize = 0;
        foreach (var manifest in neededManifests) totalDownloadSize += manifest.size;

        long freeSpace = drive.TotalFreeSpace;
        if (drive.TotalFreeSpace < totalDownloadSize) {
          Logging.MessageBox($@"Steam install location is in drive {drive.Name}
  has {Math.Round(freeSpace / 1000000000.0, 1)} GB of free space
  but {Math.Round(totalDownloadSize / 1000000000.0, 1)} GB are required.", "Not enough space");
          return;
        }

        component.State = VersionState.Downloading;

        { // Keep steam interaction close together, to avoid accidental user interference
          SteamCommand.OpenConsole();
          Thread.Sleep(10);
          foreach (var manifest in neededManifests) SteamCommand.DownloadDepot(manifest.appId, manifest.depotId, manifest.manifestId);
          MainWindow.SetForeground();
        }

        Thread.Sleep(5000); // Extra sleep to avoid a race condition where we check for depots before they're actually cleared.

        while (true) {
          long actualSize = 0;
          foreach (var manifest in neededManifests) actualSize += GetFolderSize(manifest.location);
          component.SetProgress(0.8 * actualSize / totalDownloadSize); // 80% - Downloading
          if (actualSize == totalDownloadSize) break;
          Thread.Sleep(1000);
        }
        component.State = VersionState.Saving;

        long copied = 0;

        // @Performance: Start copying while downloads are in progress?
        foreach (var manifest in neededManifests) {
          CopyAndOverwrite(manifest.location, GetFolder(version, manifest.package), delegate (long fileSize) {
            copied += fileSize;
            component.SetProgress(0.8 + 0.2 * (copied / totalDownloadSize)); // 20% - Copying
          });
        }

        if (drive.TotalFreeSpace < 5 * totalDownloadSize) {
          Logging.Log("Low on disk space, clearing download directory");
          Directory.Delete(ManifestData.DepotLocation, true);
        }
        component.State = VersionState.Downloaded;
      }
    }

    public static void SaveActiveVersion(VersionUIComponent component) {
      var thread = new Thread(() => { SaveActiveVersionInternal(component); });
      thread.IsBackground = true;
      thread.Start();
    }

    private static void SaveActiveVersionInternal(VersionUIComponent component) {
      long totalSize = GetFolderSize(Settings.Default.activeVersionLocation);
      long copied = 0;
      CopyAndOverwrite(Settings.Default.activeVersionLocation, GetFolder(component.version, Package.Main), delegate (long fileSize) {
        copied += fileSize;
        component.SetProgress(copied / totalSize);
      });

      // These moves are in the same drive, so they're hopefully fast enough to not worry about the progress bar.
      MoveMatching(GetFolder(component.version, Package.Main), GetFolder(component.version, Package.Gehenna), "Content/Talos/DLC_01_Road_To_Gehenna*");
      MoveMatching(GetFolder(component.version, Package.Main), GetFolder(component.version, Package.Prototype), "Content/Talos/DLC_Prototype*");

      component.State = VersionState.Active;
    }

    #region utilities

    public bool IsFullyCopied(int version) {
      long actualSize = GetFolderSize($"{Settings.Default.activeVersionLocation}");

      long expectedSize = 0;
      expectedSize += manifestData.GetDownloadSize(version, Package.Main);
      if (Settings.Default.ownsGehenna) expectedSize += manifestData.GetDownloadSize(version, Package.Gehenna);
      if (Settings.Default.ownsPrototype) expectedSize += manifestData.GetDownloadSize(version, Package.Prototype);
      Logging.Log("Actual: " + actualSize + " Expected: " + expectedSize + " IsFullyCopied: " + (actualSize >= expectedSize));
      return actualSize >= expectedSize;
    }

    public bool IsFullyDownloaded(int version, Package package) {
      long actualSize = GetFolderSize(GetFolder(version, package));
      if (actualSize == 0) return false;

      long expectedSize = manifestData.GetDownloadSize(version, package);
      if (actualSize == expectedSize) return true;
      Logging.Log($"Package {package} for version {version} is {actualSize} bytes, expected {expectedSize}");
      if (actualSize > expectedSize) {
        Logging.Log("Package has additional, unexpected data. Assuming it was fully downloaded...");
        return true;
      }
      return false;
    }

    private static int Find(byte[] data, byte[] search, int startIndex = 0) {
      for (int i = startIndex; i < data.Length - search.Length; i++) {
        bool match = true;
        for (int j = 0; j < search.Length; j++) {
          if (data[i + j] == search[j]) {
            continue;
          }
          match = false;
          break;
        }
        if (match) return i;
      }
      return -1;
    }

    public static int GetInstalledVersion() {
      var talos = new FileInfo(Settings.Default.activeVersionLocation + "/Bin/x64/Talos.exe");
      if (!talos.Exists) {
        talos = new FileInfo(Settings.Default.activeVersionLocation + "/Bin/Talos.exe");
      }
      if (!talos.Exists) return 0;
      int buffSize = (1 << 20); // 1 MB
      byte[] buff = new byte[buffSize];
      using (FileStream fsread = talos.OpenRead()) {
        using (BinaryReader bwread = new BinaryReader(fsread)) {
          for (; ; ) {
            int readBytes = bwread.Read(buff, 0, buffSize);
            if (readBytes == 0) break;
            int index = Find(buff, Encoding.ASCII.GetBytes("Talos-Windows-Final"));
            if (index != -1) {
              return int.Parse(Encoding.ASCII.GetString(buff, index + 21, 6), CultureInfo.InvariantCulture);
            }
          }
        }
      }
      return 0;
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

    private static long GetFolderSize(string folder) {
      long size = 0;
      var src = new DirectoryInfo(folder);
      if (src.Exists) {
        var files = src.GetFiles("*", SearchOption.AllDirectories);
        foreach (var file in files) size += file.Length;
      }
      return size;
    }

    private static void MoveMatching(string srcFolder, string dstFolder, string searchPattern) {
      var src = new DirectoryInfo(srcFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(dstFolder);
      if (!dst.Exists) Directory.CreateDirectory(dstFolder);

      foreach (var dir in src.GetDirectories()) MoveMatching($"{srcFolder}/{dir}", $"{dstFolder}/{dir}", searchPattern);
      foreach (var file in src.GetFiles(searchPattern)) file.MoveTo(dstFolder);
    }

    private static void CopyAndOverwrite(string srcFolder, string dstFolder, Action<long> onCopyBytes = null) {
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1005:Delegate invocation can be simplified.", Justification = "The if check is relevant, as onCopyBytes may be null.")]
    static void FCopy(string source, string destination, Action<long> onCopyBytes) {
      int buffSize = (1 << 20); // 1 MB
      byte[] buff = new byte[buffSize];
      using (FileStream fsread = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None, buffSize)) {
        using (BinaryReader bwread = new BinaryReader(fsread)) {
          using (FileStream fswrite = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, buffSize)) {
            using (BinaryWriter bwwrite = new BinaryWriter(fswrite)) {
              for (; ; ) {
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

    #endregion
  }
}
