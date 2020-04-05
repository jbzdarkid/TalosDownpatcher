using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Threading;
using TalosDownpatcher.Properties;

// TODO: Show uncommon versions if downloaded

// TODO: Progress bar for downloading. This requires estimation of network speeds, which isn't *too* hard. But it's not fun.
// TODO: Editor
// TODO: What happens if steam only downloads 99.9% of the depots? How do I 'give up' gracefully? Or, how do I communicate to the user that they should "give up"?
// TODO: Potentially integrate with SteamWorks to determine install path / DLC status / Game launch status
// TODO: You can queue "set version active", which is not good. This should cancel the previous copy.
// ^ This is also a lot of work, and complexity, that nobody really cares about. Stability > features
// TODO: Some way to "delete" a download through the UI?
// ^ Maybe just add an "Open old versions location" button in settings.
// TODO: File chooser for version location?
// TODO: Wait for steam.exe to launch?

// To make apple happy:
// - /path/to/downpatcher.exe %command% (steam direct launch option)
// - Symlinks (https://github.com/apple1417/LegacyWorkshopLoader/blob/master/SymlinkWindows.cs)
// - (of course) Automatic detection of steam version w/ copy
// - (Editor, if possible. If I add a steam.exe hack, then think about this.)

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    // TODO: Audit protection levels
    public DepotManager depotManager = new DepotManager();
    public Dictionary<int, VersionUIComponent> uiComponents = new Dictionary<int, VersionUIComponent>();
    private SettingsWindow settingsWindow = null;

    public MainWindow() {
      InitializeComponent();
      LoadVersions();
      dispatcher = Dispatcher; // Saved statically so that we can consistently dispatch from any thread
    }

    public bool ActionInProgress() {
      foreach (var uiComponent in uiComponents.Values) {
        if (uiComponent.ActionInProgress()) {
          return true;
        }
      }
      return false;
    }

    public void LoadVersions() {
      // Ensure that it's safe to discard state
      if (ActionInProgress()) return;
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
      Height = 80 + versions.Count * 20;

      Settings.Default.activeVersion = 326589;
      for (int i = 0; i < versions.Count; i++) {
        int version = versions[i];
        uiComponents[version] = new VersionUIComponent(version, 30 + 20 * i, this);

        bool hasMain = depotManager.IsFullyDownloaded(version, Package.Main);
        bool hasGehenna = depotManager.IsFullyDownloaded(version, Package.Gehenna);
        bool hasPrototype = depotManager.IsFullyDownloaded(version, Package.Prototype);

        if (hasMain && (!Settings.Default.ownsGehenna || hasGehenna) && (!Settings.Default.ownsPrototype || hasPrototype)) {
          // We have everything we should
          if (version == Settings.Default.activeVersion) {
            if (depotManager.IsFullyCopied(version)) {
              uiComponents[version].State = VersionState.Active;
            } else {
              Settings.Default.activeVersion = 0;
              Settings.Default.Save();
            }
          } else {
            uiComponents[version].State = VersionState.Downloaded;
          }
        } else if (hasMain || (Settings.Default.ownsGehenna && hasGehenna) || (Settings.Default.ownsPrototype && hasPrototype)) {
          // We don't have everything we should, but we do have *something*
          uiComponents[version].State = VersionState.PartiallyDownloaded;
        } else {
          // We have nothing
          uiComponents[version].State = VersionState.NotDownloaded;
        }
      }
    }

    // This function is always called on a background thread.
    public void VersionButtonOnClick(VersionUIComponent component) {
      Contract.Requires(component != null);
      Logging.Log($"VersionUIComponent {component.version} clicked in state {component.State}");
      switch (component.State) {
        case VersionState.NotDownloaded:
        case VersionState.PartiallyDownloaded:
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
          break;
      }
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e) {
      if (settingsWindow == null || !settingsWindow.IsLoaded) {
        Logging.Log("Showing settings window");
        settingsWindow = new SettingsWindow(this);
        settingsWindow.Show();
        settingsWindow.Activate();
      } else {
        Logging.Log("Hiding settings window");
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
        Logging.Log("Setting as foreground window");
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        mainWindow.Activate();
      });
    }
  }
}
