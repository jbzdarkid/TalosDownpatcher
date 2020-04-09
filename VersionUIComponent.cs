using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TalosDownpatcher.Properties;

public static class Utils {
  /// TODO: Find a real home for this
  /// <summary>
  /// Removes all event handlers subscribed to the specified routed event from the specified element.
  /// https://stackoverflow.com/a/16392387
  /// </summary>
  /// <param name="element">The UI element on which the routed event is defined.</param>
  /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
  public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent) {
    if (element == null) return;

    // Get the EventHandlersStore instance which holds event handlers for the specified element.
    // The EventHandlersStore class is declared as internal.
    var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
        "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
    object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

    if (eventHandlersStore == null) return;

    // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
    // for getting an array of the subscribed event handlers.
    var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
        "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
        eventHandlersStore, new object[] { routedEvent });

    // Iteratively remove all routed event handlers from the element.
    foreach (var routedEventHandler in routedEventHandlers) {
      element.RemoveHandler(routedEvent, routedEventHandler.Handler);
    }
  }
};

namespace TalosDownpatcher {
  public enum VersionState {
    NotDownloaded,
    PartiallyDownloaded,
    DownloadPending,
    Downloading,
    Saving,
    Downloaded,
    CopyPending,
    Copying,
    Active,
  };

  public class VersionUIComponent : IDisposable {
    private readonly MainWindow mainWindow;

    private TextBox versionBox;
    private Rectangle downloadBar;
    private TextBox stateBox;
    private Button actionButton;

    public VersionUIComponent(int version, double yPos, MainWindow mainWindow) {
      Contract.Requires(mainWindow != null);
      this.version = version;
      this.mainWindow = mainWindow;

      versionBox = new TextBox();
      versionBox.HorizontalAlignment = HorizontalAlignment.Left;
      versionBox.VerticalAlignment = VerticalAlignment.Top;
      versionBox.Height = 21;
      versionBox.Width = 51;
      versionBox.Margin = new Thickness(10, yPos, 0, 0);
      versionBox.Text = version.ToString("F0", CultureInfo.InvariantCulture);
      versionBox.IsReadOnly = true;
      mainWindow.RootGrid.Children.Add(versionBox);

      downloadBar = new Rectangle();
      downloadBar.HorizontalAlignment = HorizontalAlignment.Left;
      downloadBar.VerticalAlignment = VerticalAlignment.Top;
      downloadBar.Height = 21;
      downloadBar.Width = 1;
      downloadBar.Margin = new Thickness(60, yPos, 0, 0);
      downloadBar.Fill = new SolidColorBrush(Colors.Green);
      mainWindow.RootGrid.Children.Add(downloadBar);

      stateBox = new TextBox();
      stateBox.HorizontalAlignment = HorizontalAlignment.Left;
      stateBox.VerticalAlignment = VerticalAlignment.Top;
      stateBox.Height = 21;
      stateBox.Width = 121;
      stateBox.Margin = new Thickness(60, yPos, 0, 0);
      stateBox.Background = Brushes.Transparent;
      stateBox.IsReadOnly = true;
      mainWindow.RootGrid.Children.Add(stateBox);

      actionButton = new Button();
      actionButton.HorizontalAlignment = HorizontalAlignment.Left;
      actionButton.VerticalAlignment = VerticalAlignment.Top;
      actionButton.Height = 21;
      actionButton.Width = 71;
      actionButton.Margin = new Thickness(180, yPos, 0, 0);
      mainWindow.RootGrid.Children.Add(actionButton);
    }

    public void SetProgress(double fractionDownloaded) {
      mainWindow.Dispatcher.Invoke(delegate {
        downloadBar.Width = stateBox.Width * fractionDownloaded;
      });
    }

    public bool ActionInProgress() {
      switch (state) {
        case VersionState.NotDownloaded:
        case VersionState.PartiallyDownloaded:
        case VersionState.Downloaded:
        case VersionState.Active:
          return false;
        case VersionState.DownloadPending:
        case VersionState.Downloading:
        case VersionState.Saving:
        case VersionState.CopyPending:
        case VersionState.Copying:
        default:
          return true;
      }
    }

    public readonly int version;
    private VersionState state;
    public VersionState State {
      get {
        return state;
      }
      set {
        // Since this touches a *lot* of UI state, we should just handle it on the main thread.
        // Actions which need to happen on background threads *should enforce this themselves*
        mainWindow.Dispatcher.Invoke(delegate {
          if (state != value) { // No need to log if this is just a re-initialization (or otherwise a no-op)
            Logging.Log($"Changing state for UIComponent {version} from {state} to {value}");
          }
          state = value;
          switch (state) {
            case VersionState.NotDownloaded:
              stateBox.Text = "Not Downloaded";
              actionButton.Content = "Download";
              SetOnClick(actionButton, delegate {
                mainWindow.depotManager.DownloadDepots(this);
              });
              break;
            case VersionState.PartiallyDownloaded:
              stateBox.Text = "Partially Downloaded";
              actionButton.Content = "Redownload";
              SetOnClick(actionButton, delegate {
                mainWindow.depotManager.DownloadDepots(this);
              });
              break;
            case VersionState.DownloadPending:
              stateBox.Text = "Download Pending";
              actionButton.Content = "Set Active";
              SetOnClick(actionButton, null);
              break;
            case VersionState.Downloading:
              stateBox.Text = "Downloading";
              actionButton.Content = "Set Active";
              SetOnClick(actionButton, null);
              break;
            case VersionState.Saving:
              stateBox.Text = "Saving";
              actionButton.Content = "Set Active";
              SetOnClick(actionButton, null);
              break;
            case VersionState.Downloaded:
              SetProgress(0.0);
              stateBox.Text = "Downloaded";
              actionButton.Content = "Set Active";
              SetOnClick(actionButton, delegate {
                mainWindow.depotManager.SetActiveVersion(this, delegate {
                  // Mark the current active version as inactive. Delayed to account for queueing.
                  int activeVersion = Settings.Default.activeVersion;
                  if (mainWindow.uiComponents.ContainsKey(activeVersion)) {
                    mainWindow.uiComponents[activeVersion].State = VersionState.Downloaded;
                  }
                });
              });
              break;
            case VersionState.CopyPending:
              stateBox.Text = "Copy Pending";
              actionButton.Content = "Play";
              SetOnClick(actionButton, null);
              break;
            case VersionState.Copying:
              stateBox.Text = "Copying";
              actionButton.Content = "Play";
              SetOnClick(actionButton, null);
              break;
            case VersionState.Active:
              SetProgress(0.0);
              stateBox.Text = "Active";
              actionButton.Content = "Play";
              SetOnClick(actionButton, delegate {
                if (this.version <= 249740) {
                  // Launch a separate, elevated process to change the date
                  var processPath = Process.GetCurrentProcess().MainModule.FileName;
                  Process.Start(new ProcessStartInfo(processPath) {
                    Verb = "runas",
                    Arguments = "LaunchOldVersion" + (Settings.Default.launchModdable ? " Moddable" : ""),
                  });
                } else if (Settings.Default.launchModdable) {
                  SteamCommand.StartModdableGame();
                } else {
                  SteamCommand.StartGame();
                }
              });
              break;
          }
        });
      }
    }

    private static void SetOnClick(Button button, Action onClick) {
      button.IsEnabled = (onClick != null);
      Utils.RemoveRoutedEventHandlers(button, Button.ClickEvent);
      button.Click += delegate (object sender, RoutedEventArgs e) {
        onClick();
      };
    }

    #region IDisposable Support
    private bool disposed = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (disposed) return;
      disposed = true;

      if (disposing) {
        mainWindow.RootGrid.Children.Remove(versionBox);
        mainWindow.RootGrid.Children.Remove(downloadBar);
        mainWindow.RootGrid.Children.Remove(stateBox);
        mainWindow.RootGrid.Children.Remove(actionButton);
        versionBox = null;
        downloadBar = null;
        stateBox = null;
        actionButton = null;
      }
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
 