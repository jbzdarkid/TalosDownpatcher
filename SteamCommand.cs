using System.Diagnostics;
using System.Globalization;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace TalosDownpatcher {
  public static class SteamCommand {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification="Standard static value format")]
    public static readonly int GAME_ID = 257510;
    private static readonly InputSimulator sim = new InputSimulator();

    public static void OpenConsole() {
      Logging.Log("Opening steam console");
      Process.Start("steam://nav/console");
      Thread.Sleep(100); // Slight delay for steam to become foreground
    }

    public static void DownloadDepot(int appId, int depot, long manifest) {
      Logging.Log($"download_depot {appId} {depot} {manifest}");
      if (manifest == 0) return; // 0 indicates "No such manifest", so we shouldn't attempt to download it.
      sim.Keyboard.TextEntry($"download_depot {appId} {depot} {manifest}");
      sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }

    public static void StartModdableGame() {
      Logging.Log("Starting Moddable Talos");
      string blah = ManifestData.DepotLocation;
      blah = blah.Substring(0, blah.Length - 7) + "common/The Talos Principle/Bin/x64/Talos_Unrestricted.exe";
      var procInfo = new ProcessStartInfo(blah) {
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
        Thread.Sleep(1000);
        foreach (var process in Process.GetProcesses()) {
          if (process.ProcessName == processName) {
            Logging.Log($"{processName} started");
            return true;
          }
        }
      }
      Logging.Log($"{processName} not started, not waiting any longer");
      return false;
    }
  }
}
