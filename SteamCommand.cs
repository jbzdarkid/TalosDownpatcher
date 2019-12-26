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

    public static void DownloadManifest(SteamManifest manifest) {
      if (manifest == null || manifest.appId == 0 || manifest.depotId == 0 || manifest.manifestId == 0) {
        return;
      }

      string cmd = $"download_depot {manifest.appId} {manifest.depotId} {manifest.manifestId}";
      Logging.Log(cmd);
      sim.Keyboard.TextEntry(cmd);
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
