using System.Diagnostics;
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

    public static void DownloadDepot(int depot, long manifest) {
      Logging.Log($"download_depot {GAME_ID} {depot} {manifest}");
      if (manifest == 0) return; // 0 indicates "No such manifest", so we shouldn't attempt to download it.
      sim.Keyboard.TextEntry($"download_depot {GAME_ID} {depot} {manifest}");
      sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }

    public static void StartGame() {
      Logging.Log("Starting Talos");
      Process.Start($"steam://rungameid/{GAME_ID}");

      // Wait for talos to launch (10s max) before returning
      for (var i=0; i<10; i++) {
        Thread.Sleep(1000);
        foreach (var process in Process.GetProcesses()) {
          if (process.ProcessName == "Talos") {
            Logging.Log("Talos process started");
            return;
          }
        }
      }
      Logging.Log("Talos process not started, not waiting any longer");
    }
  }
}
