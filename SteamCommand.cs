using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using TalosDownpatcher.Properties;
using WindowsInput;
using WindowsInput.Native;

namespace TalosDownpatcher {
  public static class SteamCommand {
    public static readonly int GAME_ID = 257510;
    private static readonly InputSimulator sim = new InputSimulator();

    public static void OpenConsole() {
      Logging.Log("Opening steam console");
      bool wasRunning = IsProcessActive("steam");
      Process.Start("steam://open/console");
      WaitForProcessToLaunch("Steam", 10);
      if (wasRunning) {
        Thread.Sleep(100); // Slight delay for steam to become foreground
      } else {
        Thread.Sleep(10000); // Sleep until steam console actually opens
      }
    }

    public static void DownloadDepot(int appId, int depot, long manifest) {
      Logging.Log($"download_depot {appId} {depot} {manifest}");
      if (manifest == 0) return; // 0 indicates "No such manifest", so we shouldn't attempt to download it.
      sim.Keyboard.TextEntry($"download_depot {appId} {depot} {manifest}");
      sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }

    public static void StartModdableGame() {
      Logging.Log("Starting Moddable Talos");
      
      var procInfo = new ProcessStartInfo(Settings.Default.activeVersionLocation + "/Bin/x64/Talos_Unrestricted.exe") {
        UseShellExecute = false
      };
      procInfo.EnvironmentVariables["SteamAppId"] = GAME_ID.ToString(CultureInfo.InvariantCulture);
      Process.Start(procInfo);
      WaitForProcessToLaunch("Talos", 10);
    }

    public static void StartGame() {
      Logging.Log("Starting Talos");
      Process.Start($"steam://rungameid/{GAME_ID}");

      WaitForProcessToLaunch("Talos", 10);
    }

    private static bool WaitForProcessToLaunch(string processName, int seconds) {
      // Wait for talos to launch (10s max) before returning
      for (var i=0; i<seconds; i++) {
        if (IsProcessActive(processName)) return true;
        Thread.Sleep(1000);
      }
      Logging.Log($"{processName} not started, not waiting any longer");
      return false;
    }

    private static bool IsProcessActive(string processName) {
      foreach (var process in Process.GetProcesses()) {
        if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)) {
          Logging.Log($"{processName} started");
          return true;
        }
      }
      return false;
    }
  }
}
