using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

    public VersionUIComponent(int version, int yPos, MainWindow mainWindow) {
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
      actionButton.Click += Button_Click;
      actionButton.Margin = new Thickness(180, yPos, 0, 0);
      mainWindow.RootGrid.Children.Add(actionButton);

      Logging.Log($"Constructed UI Component {version} at yPos {yPos}");
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
        Logging.Log($"Changing state for UIComponent {version} from {state} to {value}");
        state = value;
        mainWindow.Dispatcher.Invoke(delegate {
          switch (state) {
            case VersionState.NotDownloaded:
              actionButton.Content = "Download";
              stateBox.Text = "Not Downloaded";
              break;
            case VersionState.PartiallyDownloaded:
              actionButton.Content = "Redownload";
              stateBox.Text = "Partially Downloaded";
              break;
            case VersionState.DownloadPending:
              actionButton.Content = "Set Active";
              actionButton.IsEnabled = false;
              stateBox.Text = "Download Pending";
              break;
            case VersionState.Downloading:
              actionButton.Content = "Set Active";
              actionButton.IsEnabled = false;
              stateBox.Text = "Downloading";
              break;
            case VersionState.Saving:
              actionButton.Content = "Set Active";
              actionButton.IsEnabled = false;
              stateBox.Text = "Saving";
              break;
            case VersionState.Downloaded:
              SetProgress(0.0);
              actionButton.Content = "Set Active";
              actionButton.IsEnabled = true;
              stateBox.Text = "Downloaded";
              break;
            case VersionState.CopyPending:
              actionButton.Content = "Play";
              actionButton.IsEnabled = false;
              stateBox.Text = "Copy Pending";
              break;
            case VersionState.Copying:
              actionButton.Content = "Play";
              actionButton.IsEnabled = false;
              stateBox.Text = "Copying";
              break;
            case VersionState.Active:
              SetProgress(0.0);
              actionButton.Content = "Play";
              actionButton.IsEnabled = true;
              stateBox.Text = "Active";
              break;
          }
        });
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      var thread = new Thread(() => { mainWindow.VersionButtonOnClick(this); });
      thread.IsBackground = true;
      thread.Start();
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