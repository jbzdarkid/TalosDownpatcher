using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using TalosDownpatcher.Properties;

// TODO: Editor -- this is Apple's problem to solve.
// TODO: Gehenna -- this is someone else's problem to solve.
// TODO: You can queue "set version active", which is not good. This should cancel the previous copy.
// ^ This is also a lot of work, and complexity, that nobody really cares about. Stability > features

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public DepotManager depotManager = new DepotManager();
    private Dictionary<int, VersionUIComponent> uiComponents = new Dictionary<int, VersionUIComponent>();
    private SettingsWindow settingsWindow = null;

    public MainWindow() {
      InitializeComponent();
      LoadVersions();
      dispatcher = Dispatcher; // Saved statically so that we can consistently dispatch from any thread
    }

    public bool LoadVersions() {
      // Ensure that it's safe to change settings
      foreach (var uiComponent in uiComponents.Values) {
        if (uiComponent.ActionInProgress()) {
          MessageBox.Show($"Cannot save settings while an operation is in progress:\nVersion {uiComponent.version} is {uiComponent.State}");
          return false;
        }
      }
      foreach (var uiComponent in uiComponents.Values) {
        uiComponent.Dispose();
      }
      uiComponents.Clear();

      List<int> versions;
      if (Settings.Default.showAllVersions) {
        versions = ManifestData.allVersions;
      } else {
        versions = ManifestData.commonVersions;
      }
      Height = 50 + versions.Count * 20;

      for (int i = 0; i < versions.Count; i++) {
        int version = versions[i];
        uiComponents[version] = new VersionUIComponent(version, 20 * i, this);

        if (version == Settings.Default.activeVersion) {
          // Running the game creats additional files. This check is mostly to prevent against partial copies.
          if (depotManager.GetDownloadFraction(version, DepotManager.Location.Active) >= 1.0) {
            uiComponents[version].State = VersionState.Active;
            continue;
          } else {
            Settings.Default.activeVersion = 0;
            Settings.Default.Save();
          }
        }

        double downloadFraction = depotManager.GetDownloadFraction(version, DepotManager.Location.Cached);
        if (downloadFraction == 1.0) {
          uiComponents[version].State = VersionState.Downloaded;
        } else if (downloadFraction == 0.0) {
          uiComponents[version].State = VersionState.Not_Downloaded;
        } else {
          Console.WriteLine($"Version {version} is {downloadFraction} downloaded -- marking as corrupt");
          uiComponents[version].State = VersionState.Corrupt;
        }
      }
      return true;
    }

    // This function is always called on a background thread.
    public void VersionButton_OnClick(VersionUIComponent component) {
      Console.WriteLine($"VersionUIComponent {component.version} clicked in state {component.State}");
      switch (component.State) {
        case VersionState.Not_Downloaded:
        case VersionState.Corrupt:
          depotManager.DownloadDepots(component);
          break;
        case VersionState.Downloaded:
          depotManager.SetActiveVersion(component, delegate {
            // Mark the current active version as inactive. Delayed to account for queueing.
            int activeVersion = Settings.Default.activeVersion;
            if (uiComponents.ContainsKey(activeVersion)) uiComponents[activeVersion].State = VersionState.Downloaded;
          });
          break;
        case VersionState.Active:
          if (component.version <= 249740) {
            // Launch a separate, elevated process to change the date
            var processPath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(new ProcessStartInfo(processPath) {
              Verb = "runas",
              Arguments = "LaunchOldVersion",
            });
          } else {
            SteamCommand.StartGame();
          }
          break;
      }
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
      if (settingsWindow == null || !settingsWindow.IsLoaded) {
        settingsWindow = new SettingsWindow();
        settingsWindow.Show();
        settingsWindow.Activate();
      } else {
        settingsWindow.Close();
      }
    }

    protected override void OnClosing(CancelEventArgs e) {
      if (settingsWindow != null) settingsWindow.Close();
      base.OnClosing(e);
    }

    private static Dispatcher dispatcher;
    public static void SetForeground() {
      dispatcher.Invoke(delegate {
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        mainWindow.Activate();
      });
    }
  }
}
