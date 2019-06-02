using System.Diagnostics;
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

      // Wait for talos to launch before returning
      while (Process.GetProcessesByName("talos.exe").Length == 0) {
        Thread.Sleep(1000);
      }
    }
  }
}