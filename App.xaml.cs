using System.Threading;
using System.Windows;

namespace TalosDownpatcher {
  public partial class App : Application {
    private void AppStartup(object sender, StartupEventArgs e) {
      Logging.Init();

      if (e.Args.Length > 0 && e.Args[0] == "LaunchOldVersion") {
        int yearDelta = DateUtils.GetCurrentYear() - 2016;
        Logging.Log($"Changing date in order to launch old version. YearDelta: {yearDelta}");
        DateUtils.SetYears(-yearDelta);
        if (e.Args.Length > 1 && e.Args[1] == "Moddable") {
          Logging.Log("Launching old, moddable version");
          SteamCommand.StartModdableGame();
        } else {
          Logging.Log("Launching old, base version");
          SteamCommand.StartGame();
        }
        // There's no rush on returning here -- the game is launched, the user is busy.
        // However, there is harm in not waiting long enough, since the game needs to boot while we're still in 2016.
        Thread.Sleep(10000);
        Logging.Log($"Restoring date. YearDelta: {yearDelta}");
        DateUtils.SetYears(+yearDelta);

        Shutdown();
      }
    }
  }
}
