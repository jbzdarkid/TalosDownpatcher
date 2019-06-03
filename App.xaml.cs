using System.Windows;

namespace TalosDownpatcher {
  public partial class App : Application {
    private void LaunchOldVersion(object sender, StartupEventArgs e) {
      if (e.Args.Length > 0 && e.Args[0] == "LaunchOldVersion") {
        DateUtils.SetYears(-3);
        SteamCommand.StartGame();
        DateUtils.SetYears(+3);

        Shutdown();
      }
    }
  }
}
