using System;
using System.Diagnostics;

namespace TalosDownpatcher {
  public static class SteamCommand {

    public static readonly int gameId = 257510;

    public static void DownloadDepot(int depot, long manifest) {
      // TODO
      Console.WriteLine($"download_depot {gameId} {depot} {manifest}");
    }

    public static void StartGame() {
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
            // Proccess not found
          }
        }
      }
    }

  }
}