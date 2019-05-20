using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TalosDownpatcher {
  public class DepotManager {
    private readonly Dictionary<int, Dictionary<int, Datum>> manifestData;

    private readonly string steamapps;
    private readonly string activeVersionLocation;
    private readonly string oldVersionLocation;
    private readonly string depotLocation;
    private readonly string activeVersionFile;

    private static readonly object downloadLock = new object();
    private static readonly object versionLock = new object();
    private int activeVersion;

    public DepotManager() {
      manifestData = ManifestData.GetData();
      steamapps = (string)Registry.GetValue(@"HKEY_CURRNET_USER\SOFTWARE\Valve\Steam", "SteamPath", @"C:\Program Files (x86)\Steam") + "/steamapps";
      activeVersionLocation = $"{steamapps}/common/The Talos Principle";
      oldVersionLocation = $"{steamapps}/common/The Talos Principle Old Versions";
      depotLocation = $"{steamapps}/content/app_257510";
      activeVersionFile = $"{activeVersionLocation}/active_version.txt";

      lock (versionLock) {
        activeVersion = 0;
        if (File.Exists(activeVersionFile)) {
          using (var file = File.OpenText(activeVersionFile)) {
            lock (versionLock) {
              activeVersion = int.Parse(file.ReadLine());
            }
          }
        }
      }
    }

    public int TrySetActiveVersion(int version) {
      lock (versionLock) {
        if (activeVersion == 0) {
          activeVersion = version;

          try {
            // Delete files created in newer versions which interfere with older versions.
            File.Delete($"{activeVersionLocation}/Content/Talos/All.dat");
          } catch (DirectoryNotFoundException) { } // File doesn't exist

          // Copy the x86 binaries to the x64 folder. They may be overwritten by the next copy operation if there are real x64 binaries.
          CopyAndOverwrite($"{oldVersionLocation}/{version}/Bin", $"{activeVersionLocation}/Bin/x64");

          CopyAndOverwrite($"{oldVersionLocation}/{version}", activeVersionLocation);
          File.WriteAllText(activeVersionFile, activeVersion.ToString());
        }
        return activeVersion;
      }
    }

    public int GetActiveVersion() {
      lock (versionLock) {
        return activeVersion;
      }
    }

    public void DownloadDepotsForVersion(int version, Action onDownloadStart, Action<double> showDownloadProgress) {
      lock (downloadLock) {
        SteamCommand.OpenConsole();
        foreach (var depot in ManifestData.depots) SteamCommand.DownloadDepot(depot, manifestData[version][depot].manifest);
        onDownloadStart();

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
