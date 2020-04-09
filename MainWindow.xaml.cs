using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TalosDownpatcher.Properties;

// TODO: Progress bar for downloading. This requires estimation of network speeds, which isn't *too* hard. But it's not fun.
// TODO: Editor
// TODO: What happens if steam only downloads 99.9% of the depots? How do I 'give up' gracefully? Or, how do I communicate to the user that they should "give up"?
// TODO: Potentially integrate with SteamWorks to determine install path / DLC status / Game launch status
// TODO: You can queue "set version active", which is not good. This should cancel the previous copy.
// ^ This is also a lot of work, and complexity, that nobody really cares about. Stability > features
// TODO: Some way to "delete" a download through the UI?
// ^ Maybe just add an "Open old versions location" button in settings.
// TODO: File chooser for version location?

// To make apple happy:
// - /path/to/downpatcher.exe %command% (steam direct launch option)
// - Symlinks (https://github.com/apple1417/LegacyWorkshopLoader/blob/master/SymlinkWindows.cs)
// - (of course) Automatic detection of steam version w/ copy
// - Editor support

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    internal DepotManager depotManager = new DepotManager();
    internal Dictionary<int, VersionUIComponent> uiComponents = new Dictionary<int, VersionUIComponent>();
    private SettingsWindow settingsWindow = null;

    public MainWindow() {
      InitializeComponent();
      LoadVersions();
      DetermineActiveVersion();
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

      this.Height = 50;
      foreach (int version in ManifestData.allVersions) {
        var uiComponent = new VersionUIComponent(version, this.Height - 50, this);

        bool hasMain = depotManager.IsFullyDownloaded(version, Package.Main);
        bool hasGehenna = depotManager.IsFullyDownloaded(version, Package.Gehenna);
        bool hasPrototype = depotManager.IsFullyDownloaded(version, Package.Prototype);

        if (hasMain && (!Settings.Default.ownsGehenna || hasGehenna) && (!Settings.Default.ownsPrototype || hasPrototype)) {
          // We have everything we should
          uiComponent.State = VersionState.Downloaded;
        } else if (hasMain || (Settings.Default.ownsGehenna && hasGehenna) || (Settings.Default.ownsPrototype && hasPrototype)) {
          // We don't have everything we should, but we do have *something*
          uiComponent.State = VersionState.PartiallyDownloaded;
        } else {
          // We have nothing
          uiComponent.State = VersionState.NotDownloaded;
        }

        if (uiComponent.State != VersionState.NotDownloaded || ManifestData.commonVersions.Contains(version) || Settings.Default.showAllVersions) {
          // Only add the version if it's downloaded, common, or we're showing all versions
          uiComponents[version] = uiComponent;
          this.Height += 20;
        } else {
          uiComponent.Dispose();
        }
      }
    }

    public void DetermineActiveVersion() {
      int version = DepotManager.GetInstalledVersion();
      if (depotManager.IsFullyCopied(version)) {
        uiComponents[version].State = VersionState.Active;
      } else {
        uiComponents[version].State = VersionState.Copying;
        DepotManager.SaveActiveVersion(uiComponents[version]);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1801", Justification = "This is inheriting an interface")]
    private void SettingsButton_Click(object sender, object e) {
      if (settingsWindow == null || !settingsWindow.IsLoaded) {
        Logging.Log("Showing settings window");
        // Because, for some reason, I need two handlers for lmb and rmb.
        bool isRmb = typeof(MouseButtonEventArgs).IsInstanceOfType(e) && ((MouseButtonEventArgs)e).ChangedButton == MouseButton.Right;
        settingsWindow = new SettingsWindow(this, isRmb);
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
