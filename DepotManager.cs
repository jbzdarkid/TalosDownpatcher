using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace TalosDownpatcher {
  public class DepotManager {
    public readonly Manifests manifests = new Manifests();

    private static readonly string steamapps = "C:/Program Files (x86)/Steam/steamapps"; // TODO: discover this, somehow
    private static readonly string activeVersionLocation = $"{steamapps}/common/The Talos Principle";
    private static readonly string oldVersionLocation = $"{steamapps}/common/The Talos Principle Old Versions";
    private static readonly string depotLocation = $"{steamapps}/content/app_257510";

    private static readonly object downloadLock = new object();
    private static readonly object versionLock = new object();
    private int activeVersion = 0;

    public int TrySetActiveVersion(int version) {
      lock (versionLock) {
        if (activeVersion == 0) {
          activeVersion = version;
          CopyAndOverwrite($"{oldVersionLocation}/{version}", activeVersionLocation);
        }
        return activeVersion;
      }
    }

    public void DownloadDepotsForVersion(int version) {
      // Ordered by size (2MB, 2MB, 26MB, 6+ GB)
      var depots = new List<int> {257516, 257519, 257511, 257515};

      SteamCommand.OpenConsole();
      long totalSize = 0;
      foreach (var depot in depots) {
        var datum = this.manifests.data[version][depot];
        totalSize += datum.size;
        SteamCommand.DownloadDepot(depot, datum.manifest);
      }

      long currentSize = 0;
      while (currentSize < totalSize) {
        Thread.Sleep(1000);

        currentSize = 0;
        foreach (var depot in depots) {
          // @Performance: I keep querying folder size even after I've finished downloading.
          // Potentially, I can just remember the state for each depot and not check it once it's done.
          currentSize += GetFolderSize($"{depotLocation}/depot_{depot}");
        }

        // TODO: Report download % here, via some callback
      }

      foreach (var depot in depots) {
        CopyAndOverwrite($"{depotLocation}/depot_{depot}", $"{oldVersionLocation}/{version}");
      }
    }

    public long GetFolderSize(string folder) {
      var src = new DirectoryInfo(folder);
      if (!src.Exists) return 0;
      var files = src.GetFiles("*", SearchOption.AllDirectories);
      long size = 0;
      foreach (var file in files) size += file.Length;
      Console.WriteLine($"Folder {folder} is {size}");
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


    /* TODO: Hash check?
    using (var hash = SHA256.Create()) {
      try {
        foreach (var file in files) {

        }
      } catch (IOException) {
        // File in use, so we're still downloading
        return State.Downloading;
      }
    }

    using (var hash = SHA256.Create()) {
      try {
        foreach (var file in files) {
          using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite)) {
            hash.TransformBlock(stream);
          }
        }
      } catch (IOException) {
        // File in use, return an invalid hash.
        return -1;
      }
      string folderHash = ConvertByteArrayToString(hash.TransformFinalBlock());
    }


        if (ComputeHash(files) != datum.hash) return State.Corrupt;
    return State.Valid;
    */

  }
}
