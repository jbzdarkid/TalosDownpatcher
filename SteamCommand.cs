using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace TalosDownpatcher {
  public static class SteamCommand {

    public static readonly int GAME_ID = 257510;
    private static InputSimulator sim = new InputSimulator();

    public static void OpenConsole() {
      Process.Start($"steam://nav/console");
      Thread.Sleep(100); // Slight delay for steam to become foreground
    }

    public static void DownloadDepot(int depot, long manifest) {
      sim.Keyboard.TextEntry($"download_depot {GAME_ID} {depot} {manifest}");
      sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }

    public static void StartGame() {
      Process.Start($"steam://run/{GAME_ID}");
    }

    public static void StopGame() {
      // https://stackoverflow.com/a/49245781
      Process[] runningProcesses = Process.GetProcesses();
      foreach (Process process in runningProcesses) {
        foreach (ProcessModule module in process.Modules) {
          if (module.FileName.Equals("Talos.exe")) {
            process.Kill();
          }
        }
      }
    }
 }
}