using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TalosDownpatcher {
  public class DepotManager {
    public readonly Manifests manifests;

    private readonly string steamapps;
    private readonly string activeVersionLocation;
    private readonly string oldVersionLocation;
    private readonly string depotLocation;

    private static readonly object downloadLock = new object();
    private static readonly object versionLock = new object();
    private int activeVersion = 0;

    public DepotManager() {
      manifests = new Manifests();
      steamapps = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") + "/steamapps";
      activeVersionLocation = $"{steamapps}/common/The Talos Principle";
      oldVersionLocation = $"{steamapps}/common/The Talos Principle Old Versions";
      depotLocation = $"{steamapps}/content/app_257510";
    }

    public int TrySetActiveVersion(int version) {
      lock (versionLock) {
        if (activeVersion == 0) {
          activeVersion = version;
          // Delete file Content/Talos/All.dat
          CopyAndOverwrite($"{oldVersionLocation}/{version}", activeVersionLocation);
        }
        return activeVersion;
      }
    }

    public void DownloadDepotsForVersion(int version, Action onDownloadStart, Action<double> showDownloadProgress) {
      lock (downloadLock) {
        // Ordered by size (2MB, 2MB, 26MB, 6+ GB)
        var depots = new List<int> { 257516, 257519, 257511, 257515 };

        SteamCommand.OpenConsole();
        foreach (var depot in depots) {
          var datum = this.manifests.data[version][depot];
          SteamCommand.DownloadDepot(depot, datum.manifest);
        }
        onDownloadStart();

        double downloadFraction = 0.0;
        while (downloadFraction < 1.0) {
          Thread.Sleep(1000);
          downloadFraction = GetDownloadFraction(version, true);
          showDownloadProgress(downloadFraction);
        }

        foreach (var depot in depots) {
          CopyAndOverwrite($"{depotLocation}/depot_{depot}", $"{oldVersionLocation}/{version}");
        }
      }
    }

    public double GetDownloadFraction(int version, bool isInTemporaryLocation) {
      var depots = new List<int> { 257511, 257515, 257516, 257519};

      long expectedSize = 0;
      foreach (var depot in depots) {
        expectedSize += manifests.data[version][depot].size;
      }

      // @Performance: If I need to, I can compute number of files downloaded instead of folder size first.
      long actualSize = 0;
      if (isInTemporaryLocation) {
        foreach (var depot in depots) {
          actualSize += GetFolderSize($"{depotLocation}/depot_{depot}");
        }
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
      Console.WriteLine($"Merging {srcFolder} into {dstFolder}");

      var src = new DirectoryInfo(srcFolder);
      if (!src.Exists) return;
      var dst = new DirectoryInfo(dstFolder);
      if (!dst.Exists) Directory.CreateDirectory(dstFolder);

      foreach (var file in src.GetFiles()) {
        Console.WriteLine("Copying file: " + file.Name);
        File.Copy(file.FullName, $"{dst}/{file.Name}", true);
      }
      foreach (var dir in src.GetDirectories()) {
        CopyAndOverwrite($"{srcFolder}/{dir}", $"{dstFolder}/{dir}");
      }
    }
  }
}
