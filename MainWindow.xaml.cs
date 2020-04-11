using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TalosDownpatcher.Properties;

// TODO: Progress bar for downloading. This requires estimation of network speeds, which isn't *too* hard. But it's not fun.
// TODO: What happens if steam only downloads 99.9% of the depots? How do I 'give up' gracefully? Or, how do I communicate to the user that they should "give up"?
// TODO: Potentially integrate with SteamWorks to determine install path / DLC status / Game launch status
// TODO: You can queue "set version active", which is not good. This should cancel the previous copy.
// ^ This is also a lot of work, and complexity, that nobody really cares about. Stability > features
// TODO: Some way to "delete" a download through the UI?
// ^ Maybe just add an "Open old versions location" button in settings.
// TODO: Warn when trying to download with steam hack deactivated
// TODO: Remove files which are not selected from the active version (at some point in time)

// To make apple happy:
// - /path/to/downpatcher.exe %command% (steam direct launch option)
// - Symlinks (https://github.com/apple1417/LegacyWorkshopLoader/blob/master/SymlinkWindows.cs)

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    internal Dictionary<int, VersionUIComponent> uiComponents = new Dictionary<int, VersionUIComponent>();
    private SettingsWindow settingsWindow = null;
    private static Dispatcher dispatcher;
    private static readonly List<int> commonVersions = new List<int> { 440323, 326589, 301136, 244371 };

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

      int installedVersion = DepotManager.GetInstalledVersion();

      this.Height = 50;
      foreach (int version in ManifestData.allVersions) {
        using (var uiComponent = new VersionUIComponent(version, this.Height - 50, this)) {
          bool mainDownloaded = DepotManager.IsFullyDownloaded(version, Package.Main);
          bool gehennaDownloaded = DepotManager.IsFullyDownloaded(version, Package.Gehenna);
          bool prototypeDownloaded = DepotManager.IsFullyDownloaded(version, Package.Prototype);
          bool editorDownloaded = DepotManager.IsFullyDownloaded(version, Package.Editor);

          if (mainDownloaded &&
            (!Settings.Default.ownsGehenna || gehennaDownloaded) &&
            (!Settings.Default.ownsPrototype || prototypeDownloaded) &&
            (!Settings.Default.wantsEditor || editorDownloaded)) {
            // We have downloaded everything we should
            uiComponent.State = VersionState.Downloaded;
          } else if (mainDownloaded ||
            (Settings.Default.ownsGehenna && gehennaDownloaded) ||
            (Settings.Default.ownsPrototype && prototypeDownloaded) ||
            (Settings.Default.wantsEditor && editorDownloaded)) {
            // We haven't downloaded everything, but we do have *something*
            uiComponent.State = VersionState.PartiallyDownloaded;
          } else {
            // We have nothing downloaded
            uiComponent.State = VersionState.NotDownloaded;
          }

          if (version == installedVersion) {
            if (uiComponent.State == VersionState.Downloaded && DepotManager.IsFullyCopied(installedVersion)) {
              // Only mark active if the data is fully copied.
              uiComponent.State = VersionState.Active;
            }
          }

          // Only add the version if it's downloaded, common, active in steam, or we're showing all versions
          if (uiComponent.State != VersionState.NotDownloaded
            || commonVersions.Contains(version)
            || version == installedVersion
            || Settings.Default.showAllVersions) {
            uiComponents[version] = uiComponent;
            this.Height += 20;
          }
        }
      }
    }

    [SuppressMessage("Style", "CA1801", Justification = "This is inheriting an interface")]
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

    public static void SetForeground() {
      dispatcher.Invoke(delegate {
        Logging.Log("Setting as foreground window");
        var mainWindow = (MainWindow)Application.Current.MainWindow;
        mainWindow.Activate();
      });
    }
  }
}
