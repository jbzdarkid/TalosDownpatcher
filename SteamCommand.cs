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
      Process.Start($"steam://nav/console");
      Thread.Sleep(100); // Slight delay for steam to become foreground
    }

    public static void DownloadDepot(int depot, long manifest) {
      sim.Keyboard.TextEntry($"download_depot {GAME_ID} {depot} {manifest}");
      sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }

    public static void StartGame() {
      Process.Start($"steam://rungameid/{GAME_ID}");

      // Wait for talos to launch (10s max) before returning
      for (var i=0; i<10; i++) {
        foreach (var process in Process.GetProcesses()) {
          if (process.ProcessName == "Talos") return;
        }

        Thread.Sleep(1000);
      }
    }
  }
}