using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
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

    public void SetActiveVersion(VersionUIComponent component, Action onDownloadStart) {
      string activeVersionLocation = Settings.Default.activeVersionLocation;
      string oldVersionLocation = Settings.Default.oldVersionLocation;

      component.State = VersionState.Copy_Pending;
      lock (versionLock) {
        if (component.version == Settings.Default.activeVersion) {
          component.State = VersionState.Active;
          return;
        }
        component.State = VersionState.Copying;
        onDownloadStart();

        // Carefully clean target folder before copying
        try {
          Directory.Delete(activeVersionLocation, true);
        } catch (DirectoryNotFoundException) {
          // Folder already deleted
        } catch (UnauthorizedAccessException) {
          MessageBox.Show($"Unable to clear {activeVersionLocation}, please ensure that nothing is using it.", "Folder in use");
          component.State = VersionState.Downloaded;
          Settings.Default.activeVersion = 0; // Reset active version, since we're now in a bad state
          Settings.Default.Save();
          return;
        }

        // Copy the x86 binaries to the x64 folder. They may be overwritten by the next copy operation if there are real x64 binaries.
        CopyAndOverwrite($"{oldVersionLocation}/{component.version}/Bin", $"{activeVersionLocation}/Bin/x64", delegate { });

        double totalSize = manifestData.GetTotalDownloadSize(component.version);
        double copied = 0.0;
        CopyAndOverwrite($"{oldVersionLocation}/{component.version}", activeVersionLocation, delegate (double fileSize) {
          copied += fileSize;
          component.SetProgress(copied / totalSize);
        });
        component.State = VersionState.Active;

        Settings.Default.activeVersion = component.version;
        Settings.Default.Save();
      }
    }

    public void DownloadDepots(VersionUIComponent component) {
      var drive = new DriveInfo(new DirectoryInfo(depotLocation).Root.FullName);
      if (!drive.IsReady) {
        MessageBox.Show($"Steam install location is in drive {drive.Name}, which is unavailable.", "Drive unavailable");
        return;
      }

      double totalDownloadSize = manifestData.GetTotalDownloadSize(component.version);
      long freeSpace = drive.TotalFreeSpace;
      if (drive.TotalFreeSpace < totalDownloadSize) {
        MessageBox.Show($@"Steam install location is in drive {drive.Name}
has {Math.Round(freeSpace / 1000000000.0, 1)} GB of free space
but {Math.Round(totalDownloadSize / 1000000000.0, 1)} GB are required.", "Not enough space");
        return;
      }

      component.State = VersionState.Download_Pending; // Pending until we lock
      lock (downloadLock) {
        component.State = VersionState.Downloading;
        SteamCommand.OpenConsole();

        foreach (var depot in ManifestData.depots) {
          SteamCommand.DownloadDepot(depot, manifestData[component.version, depot].manifest);
        }
        MainWindow.SetForeground();

        Thread.Sleep(5000); // Extra sleep to avoid a race condition where we check for depots before they're actually cleared.
        double downloadFraction = 0.0;
        while (downloadFraction < 1.0) {
          Thread.Sleep(1000);
          downloadFraction = GetDownloadFraction(component.version, Location.Depots);
          component.SetProgress(0.8 * downloadFraction); // 80% - Downloading
        }
        component.State = VersionState.Saving;

        double copied = 0.0;
        foreach (var depot in ManifestData.depots) {
          CopyAndOverwrite($"{depotLocation}/depot_{depot}", $"{Settings.Default.oldVersionLocation}/{component.version}", delegate (double fileSize) {
            copied += fileSize;
            component.SetProgress(0.8 + 0.2 * (copied / totalDownloadSize));
          });
        }
        if (drive.TotalFreeSpace < 5 * totalDownloadSize) {
          // If we are low on space, clear the download directory.
          Directory.Delete(depotLocation, true);
        }
      }
      component.State = VersionState.Downloaded;
    }

    public enum Location {
      Depots,
      Cached,
      Active
    };

    public double GetDownloadFraction(int version, Location location) {
      long actualSize = 0;
      switch (location) {
        case Location.Depots:
          foreach (var depot in ManifestData.depots) actualSize += GetFolderSize($"{depotLocation}/depot_{depot}");
          break;
        case Location.Cached:
          actualSize += GetFolderSize($"{Settings.Default.oldVersionLocation}/{version}");
          break;
        case Location.Active:
          actualSize += GetFolderSize($"{Settings.Default.activeVersionLocation}");
          break;
      }

      // If this metric is not accurate, I can potentially improve it using the number of files.
      return actualSize / manifestData.GetTotalDownloadSize(version);
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

    private static void CopyAndOverwrite(string srcFolder, string dstFolder, Action<double> onCopyFile) {
      var src = new DirectoryInfo(srcFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(dstFolder);
      if (!dst.Exists) Directory.CreateDirectory(dstFolder);

      foreach (var dir in src.GetDirectories()) CopyAndOverwrite($"{srcFolder}/{dir}", $"{dstFolder}/{dir}", onCopyFile);
      foreach (var file in src.GetFiles()) {
        File.Copy(file.FullName, $"{dst}/{file.Name}", true);
        onCopyFile(file.Length);
      }
    }
  }
}
