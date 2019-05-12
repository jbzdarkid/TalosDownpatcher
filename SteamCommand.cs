using System;
using System.Diagnostics;
using System.Threading;

namespace TalosDownpatcher {
  public static class SteamCommand {

    public static readonly int gameId = 257510;

    public static void DownloadDepot(int depot, long manifest) {
      Thread.Sleep(1000);
      // TODO
      Console.WriteLine($"download_depot {gameId} {depot} {manifest}");
      // TODO: Wait until done, somehow
    }

    public static void StartGame() {
      Thread.Sleep(1000);
      // TODO
      Console.WriteLine($"steam://run/{gameId}");
    }

    public static void StopGame() {
      // https://stackoverflow.com/a/49245781
      Process[] runningProcesses = Process.GetProcesses();
      foreach (Process process in runningProcesses) {
        // now check the modules of the process
        foreach (ProcessModule module in process.Modules) {
          if (module.FileName.Equals("Talos.exe")) {
            process.Kill();
          } else {
            // Process not found
          }
        }
      }
    }

  }
}