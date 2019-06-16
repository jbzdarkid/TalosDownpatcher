using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  public class DepotManager {
    private readonly Dictionary<int, Dictionary<int, Datum>> manifestData = ManifestData.GetData();

    private readonly string depotLocation;
    private static readonly object downloadLock = new object();
    private static readonly object versionLock = new object();
    private int activeVersion;

    public DepotManager() {
      string steamInstall = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", "C:/Program Files (x86)/Steam");
      depotLocation = $"{steamInstall}/steamapps/content/app_257510";
      foreach (DriveInfo drive in DriveInfo.GetDrives()) {

        var k = drive.TotalFreeSpace;
      }

    }

    public void SetActiveVersion(int version) {
      string activeVersionLocation = Settings.Default.activeVersionLocation;
      string oldVersionLocation = Settings.Default.oldVersionLocation;
      lock (versionLock) {
        if (version == activeVersion) return;
        activeVersion = version;

        // Clean target folder before copying
        try {
          Directory.Delete(activeVersionLocation, true);
        } catch (DirectoryNotFoundException) { }

        // Copy the x86 binaries to the x64 folder. They may be overwritten by the next copy operation if there are real x64 binaries.
        CopyAndOverwrite($"{oldVersionLocation}/{version}/Bin", $"{activeVersionLocation}/Bin/x64");

        CopyAndOverwrite($"{oldVersionLocation}/{version}", activeVersionLocation);
        Settings.Default.activeVersion = activeVersion;
      }
    }

    public int GetActiveVersion() {
      lock (versionLock) {
        return Settings.Default.activeVersion;
      }
    }

    public void DownloadDepotsForVersion(int version, Action onDownloadStart, Action<double> showDownloadProgress) {
      var drive = new DriveInfo(new DirectoryInfo(depotLocation).Root.FullName);
      drive = new DriveInfo("E:/");
      if (!drive.IsReady) {
        MessageBox.Show("Error Message", "Error Title");
        return;
      }

      string oldVersionLocation = Settings.Default.oldVersionLocation;
      lock (downloadLock) {
        SteamCommand.OpenConsole();

        foreach (var depot in ManifestData.depots) SteamCommand.DownloadDepot(depot, manifestData[version][depot].manifest);
        onDownloadStart();

        Thread.Sleep(5000); // Extra sleep to avoid a race condition where we check for depots before they're actually cleared.
        double downloadFraction = 0.0;
        while (downloadFraction < 1.0) {
          Thread.Sleep(1000);
          downloadFraction = GetDownloadFraction(version, true);
          showDownloadProgress(downloadFraction);
        }

        foreach (var depot in ManifestData.depots) {
          CopyAndOverwrite($"{depotLocation}/depot_{depot}", $"{oldVersionLocation}/{version}");
        }
      }
    }

    public double GetDownloadFraction(int version, bool isInTemporaryLocation) {
      string oldVersionLocation = Settings.Default.oldVersionLocation;
      long expectedSize = 0;
      foreach (var depot in ManifestData.depots) {
        expectedSize += manifestData[version][depot].size;
      }

      // @Performance: If I need to, I can compute number of files downloaded instead of folder size first.
      long actualSize = 0;
      if (isInTemporaryLocation) {
        foreach (var depot in ManifestData.depots) actualSize += GetFolderSize($"{depotLocation}/depot_{depot}");
      } else {
        actualSize += GetFolderSize($"{oldVersionLocation}/{version}");
      }

      // If this metric is not accurate, I can potentially improve it using the number of files.
      return actualSize / (double)expectedSize;
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

    private static void CopyAndOverwrite(string srcFolder, string dstFolder) {
      var src = new DirectoryInfo(srcFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(dstFolder);
      if (!dst.Exists) Directory.CreateDirectory(dstFolder);

      foreach (var dir in src.GetDirectories()) CopyAndOverwrite($"{srcFolder}/{dir}", $"{dstFolder}/{dir}");
      foreach (var file in src.GetFiles()) File.Copy(file.FullName, $"{dst}/{file.Name}", true);
    }
  }
}
