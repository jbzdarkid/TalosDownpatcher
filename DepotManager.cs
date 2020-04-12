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
  public static class DepotManager {
    private static readonly object downloadLock = new object();
    private static readonly object versionLock = new object();

    public static void SetActiveVersionAsync(VersionUIComponent component, Action onSetActiveVersion) {
      Utils.RunAsync(delegate { SetActiveVersion(component, onSetActiveVersion); });
    }

    private static void SetActiveVersion(VersionUIComponent component, Action onSetActiveVersion) {
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
          Logging.MessageBox("Folder in use", $"Unable to clear {activeVersionLocation}, please ensure that nothing is using it.");
          component.State = VersionState.Downloaded;
          return;
        }
        Logging.Log("Deletion successful");

        // Copy the x86 binaries to the x64 folder. They may be overwritten by the next copy operation if there are real x64 binaries.
        CopyAndOverwrite(GetFolder(component.version, Package.Main) + "/Bin", activeVersionLocation + "/Bin/x64");

        List<Package> requiredPackages = new List<Package> { Package.Main };
        if (Settings.Default.ownsGehenna) requiredPackages.Add(Package.Gehenna);
        if (Settings.Default.ownsPrototype) requiredPackages.Add(Package.Prototype);
        if (Settings.Default.wantsEditor) requiredPackages.Add(Package.Editor);

        double totalSize = 0;
        foreach (var package in requiredPackages) totalSize += Utils.GetFolderSize(GetFolder(component.version, package));

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

    public static void DownloadDepotsAsync(VersionUIComponent component) {
      Utils.RunAsync(delegate { DownloadDepots(component); });
    }

    private static void DownloadDepots(VersionUIComponent component) {
      var previousState = component.State;
      component.State = VersionState.DownloadPending; // Pending until we acquire the  lock
      lock (downloadLock) {
        int version = component.version;

        Logging.Log($"Downloading depots for {version}");
        var drive = new DriveInfo(new DirectoryInfo(ManifestData.DepotLocation).Root.FullName);
        if (!drive.IsReady) {
          Logging.MessageBox("Drive unavailable", $"Steam install location is in drive {drive.Name}, which is unavailable.");
          return;
        }

        var neededManifests = new List<SteamManifest>();
        if (!IsFullyDownloaded(version, Package.Main)) {
          neededManifests.AddRange(ManifestData.Get(version, Package.Main));
        }
        if (Settings.Default.ownsGehenna && !IsFullyDownloaded(version, Package.Gehenna)) {
          neededManifests.AddRange(ManifestData.Get(version, Package.Gehenna));
        }
        if (Settings.Default.ownsPrototype && !IsFullyDownloaded(version, Package.Prototype)) {
          neededManifests.AddRange(ManifestData.Get(version, Package.Prototype));
        }
        if (Settings.Default.wantsEditor && !IsFullyDownloaded(version, Package.Editor)) {
          neededManifests.AddRange(ManifestData.Get(version, Package.Editor));
        }
        if (neededManifests.Count == 0) {
          Logging.Log($"Attempted to download manifests for {version}, but no manfiests applied.");
          component.State = VersionState.Downloaded;
          return;
        }

        if (Settings.Default.steamHack == false) {
          Logging.MessageBox("Unable to download",
@"Steam has broken the download_depots command, so this version can't be downloaded.
To download versions, please change your beta participation in Steam, re-download the game, then re-launch the downpatcher.
Version | Steam beta
--------+--------------------
440323  | NONE
326589  | previousversion
252786  | legacy_winxp
244371  | speedrun-244371");
          component.State = previousState;
          return;
        }

        double totalDownloadSize = 0;
        foreach (var manifest in neededManifests) totalDownloadSize += manifest.size;

        long freeSpace = drive.TotalFreeSpace;
        if (drive.TotalFreeSpace < totalDownloadSize) {
          Logging.MessageBox("Not enough space",
$@"Steam install location is in drive {drive.Name}
has {Math.Round(freeSpace / 1000000000.0, 1)} GB of free space
but {Math.Round(totalDownloadSize / 1000000000.0, 1)} GB are required.");
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
          foreach (var manifest in neededManifests) actualSize += Utils.GetFolderSize(manifest.location);
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

    public static void SaveActiveVersionAsync(VersionUIComponent component) {
      Utils.RunAsync(delegate { SaveActiveVersion(component); });
    }

    private static void SaveActiveVersion(VersionUIComponent component) {
      component.State = VersionState.Saving;

      bool mainDownloaded = IsFullyDownloaded(component.version, Package.Main);
      bool gehennaDownloaded = IsFullyDownloaded(component.version, Package.Gehenna);
      bool prototypeDownloaded = IsFullyDownloaded(component.version, Package.Prototype);
      bool editorDownloaded = IsFullyDownloaded(component.version, Package.Editor);

      double processedFiles = 0;
      var src = new DirectoryInfo(Settings.Default.activeVersionLocation);
      var files = src.GetFiles("*", SearchOption.AllDirectories);
      foreach (var file in files) {
        var package = DeterminePackage(file.FullName);
        if ((package == Package.Main && !mainDownloaded)
          || (package == Package.Gehenna && !gehennaDownloaded)
          || (package == Package.Prototype && !prototypeDownloaded)
          || (package == Package.Editor && !editorDownloaded)) {
          // This .Replace is a little bit gross, I'd rather have a 'relative path to activeVersionLocation'. But it works.
          var dest = file.FullName.Replace(src.FullName, GetFolder(component.version, package));
          Utils.FCopy(file.FullName, dest);
          processedFiles++;
          component.SetProgress(processedFiles / files.Length);
        }
      }

      DeleteExtraFiles(component);
    }

    public static void DeleteExtraFilesAsync(VersionUIComponent component) {
      Utils.RunAsync(delegate { DeleteExtraFiles(component); });
    }

    private static void DeleteExtraFiles(VersionUIComponent component) {
      component.State = VersionState.Cleaning;

      double processedFiles = 0;
      var src = new DirectoryInfo(Settings.Default.activeVersionLocation);
      var files = src.GetFiles("*", SearchOption.AllDirectories);
      foreach (var file in files) {
        var package = DeterminePackage(file.FullName);
        if ((package == Package.Gehenna && !Settings.Default.ownsGehenna)
        || (package == Package.Prototype && !Settings.Default.ownsPrototype)
        || (package == Package.Editor && !Settings.Default.wantsEditor)) {
          file.Delete();
          processedFiles++;
          component.SetProgress(processedFiles / files.Length);
        }
      }

      component.State = VersionState.Active;
    }

    #region utilities

    public static bool IsFullyCopied(int version) {
      long actualSize = Utils.GetFolderSize($"{Settings.Default.activeVersionLocation}");
    
      long expectedSize = 0;
      expectedSize += ManifestData.GetDownloadSize(version, Package.Main);
      if (Settings.Default.ownsGehenna)   expectedSize += ManifestData.GetDownloadSize(version, Package.Gehenna);
      if (Settings.Default.ownsPrototype) expectedSize += ManifestData.GetDownloadSize(version, Package.Prototype);
      if (Settings.Default.wantsEditor)   expectedSize += ManifestData.GetDownloadSize(version, Package.Editor);
      Logging.Log("Actual: " + actualSize + " Expected: " + expectedSize + " IsFullyCopied: " + (actualSize >= expectedSize));
      return actualSize >= expectedSize;
    }

    public static bool IsFullyDownloaded(int version, Package package) {
      long actualSize = Utils.GetFolderSize(GetFolder(version, package));
      if (actualSize == 0) return false;

      long expectedSize = ManifestData.GetDownloadSize(version, package);
      if (actualSize == expectedSize) return true;
      Logging.Log($"Package {package} for version {version} is {actualSize} bytes, expected {expectedSize}");
      if (actualSize > expectedSize) {
        Logging.Log("Package has additional, unexpected data. Assuming it was fully downloaded...");
        return true;
      }
      return false;
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
          while (true) {
            int readBytes = bwread.Read(buff, 0, buffSize);
            if (readBytes == 0) break;
            int index = Utils.Find(buff, Encoding.ASCII.GetBytes("Talos-Windows-Final"));
            if (index != -1) {
              return int.Parse(Encoding.ASCII.GetString(buff, index + 21, 6), CultureInfo.InvariantCulture);
            }
          }
        }
      }
      return 0;
    }

    static readonly List<string> editorFiles = new List<string>{
        "GISolver.exe",
        "ImportExportFBX.dll",
        "Talos_SeriousEditor.exe",
        "TexDXT.dll",
        "SECmd.exe",
        "ImportExportFBX2015.dll",
        "ImportExportFBX2016.dll",
        "ImportExportFBX2018.dll",
        "ReplaceHistory.txt",
        "ApplySetting.lua",
        "ApplySettingEgypt.lua",
        "ApplySettingGeneric.lua",
        "ApplySettingMedieval.lua",
        "ApplySettingRome.lua",
        "ApplySettingStone.lua",
        "ApplySettingWood.lua",
      };

    private static Package DeterminePackage(string file) {
      if (file.StartsWith("DLC_01_Road_To_Gehenna", StringComparison.OrdinalIgnoreCase)) {
        return Package.Gehenna;
      } else if (file.StartsWith("DLC_Prototype", StringComparison.OrdinalIgnoreCase)) {
        return Package.Prototype;
      } else if (editorFiles.Contains(file) || file.EndsWith("_edit.gro", StringComparison.OrdinalIgnoreCase)) {
        return Package.Editor;
      } else {
        return Package.Main;
      }
    }

    private static string GetFolder(int version, Package package) {
      string folder = $"{Settings.Default.oldVersionLocation}/{version}";
      if (package == Package.Main) return folder;
      else if (package == Package.Gehenna) return folder + "_Gehenna";
      else if (package == Package.Prototype) return folder + "_Prototype";
      else if (package == Package.Editor) return folder + "_Editor";
      else {
        Debug.Assert(false);
        return "";
      }
    }

    private static void CopyAndOverwrite(string srcFolder, string dstFolder, Action<long> onCopyBytes = null) {
      var src = new DirectoryInfo(srcFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(dstFolder);
      if (!dst.Exists) Directory.CreateDirectory(dstFolder);

      foreach (var dir in src.GetDirectories()) CopyAndOverwrite($"{srcFolder}/{dir}", $"{dstFolder}/{dir}", onCopyBytes);
      foreach (var file in src.GetFiles()) Utils.FCopy(file.FullName, $"{dst}/{file.Name}", onCopyBytes);
    }

    #endregion
  }
}
