using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using TalosDownpatcher.Properties;

// TODO: Editor -- this is Apple's problem to solve.
// TODO: Cancel download? There's a Thread.Abort(), but I need a nice way to wire it
// TODO: Progress bar for copying? It's awkward to do inside of the copy operation.
// TODO: You can queue "set version active", which is maybe not good. Disable buttons / use cancel?

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public DepotManager depotManager = new DepotManager();
    private Dictionary<int, VersionUIComponent> uiComponents = new Dictionary<int, VersionUIComponent>();
    private SettingsWindow settingsWindow = null;

    public MainWindow() {
      InitializeComponent();
      LoadVersions();
    }

    public void LoadVersions() {
      var versions = new List<int>(ManifestData.defaultVersions);
      if (Settings.Default.showAllVersions) versions.AddRange(ManifestData.extraVersions);
      this.Height = 50 + versions.Count * 20;
      foreach (var uiComponent in this.uiComponents.Values) uiComponent.Dispose();
      this.uiComponents.Clear();

      for (int i = 0; i < versions.Count; i++) {
        int version = versions[i];
        var uiComponent = new VersionUIComponent(version, 20 * i, this);

        double downloadFraction = depotManager.GetDownloadFraction(version, false);
        if (downloadFraction == 1.0) {
          if (version == Settings.Default.activeVersion) {
            uiComponent.State = VersionState.Active;
          } else {
            uiComponent.State = VersionState.Downloaded;
          }
        } else if (downloadFraction == 0.0) {
          uiComponent.State = VersionState.Not_Downloaded;
        } else {
          Console.WriteLine($"Version {version} is {downloadFraction} downloaded -- marking as corrupt");
          uiComponent.State = VersionState.Corrupt;
        }
        uiComponents[version] = uiComponent;
      }
    }

    // This function is always called on a background thread.
    public void VersionButton_OnClick(VersionUIComponent component) {
      Console.WriteLine($"VersionUIComponent {component.version} clicked in state {component.State}");
      switch (component.State) {
        case VersionState.Not_Downloaded:
        case VersionState.Corrupt:
          component.State = VersionState.Download_Pending;
          depotManager.DownloadDepotsForVersion(component.version, delegate {
            component.State = VersionState.Downloading;
          }, component.SetDownloadFraction);
          component.State = VersionState.Downloaded;
          break;
        case VersionState.Downloaded:
          int activeVersion = Settings.Default.activeVersion;
          if (uiComponents.ContainsKey(activeVersion)) uiComponents[activeVersion].State = VersionState.Downloaded;
          component.State = VersionState.Copying;
          depotManager.SetActiveVersion(component.version);
          component.State = VersionState.Active;
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
  }
}
