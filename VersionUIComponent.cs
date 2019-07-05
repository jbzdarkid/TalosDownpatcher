using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TalosDownpatcher {
  public enum VersionState {
    Not_Downloaded,
    Corrupt,
    Download_Pending,
    Downloading,
    Saving,
    Downloaded,
    Copy_Pending,
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
      this.version = version;
      this.mainWindow = mainWindow;

      versionBox = new TextBox();
      versionBox.HorizontalAlignment = HorizontalAlignment.Left;
      versionBox.VerticalAlignment = VerticalAlignment.Top;
      versionBox.Height = 21;
      versionBox.Width = 51;
      versionBox.Margin = new Thickness(10, yPos, 0, 0);
      versionBox.Text = version.ToString();
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
    }

    public void SetProgress(double fractionDownloaded) {
      mainWindow.Dispatcher.Invoke(delegate {
        downloadBar.Width = stateBox.Width * fractionDownloaded;
      });
    }

    public readonly int version;
    private VersionState state;
    public VersionState State {
      get {
        return state;
      }
      set {
        state = value;
        mainWindow.Dispatcher.Invoke(delegate {
          stateBox.Text = state.ToString().Replace('_', ' ');
          switch (state) {
            case VersionState.Not_Downloaded:
              actionButton.Content = "Download";
              break;
            case VersionState.Corrupt:
              actionButton.Content = "Redownload";
              break;
            case VersionState.Download_Pending:
            case VersionState.Downloading:
            case VersionState.Saving:
              actionButton.Content = "Set Active";
              actionButton.IsEnabled = false;
              break;
            case VersionState.Downloaded:
              SetProgress(0.0);
              actionButton.Content = "Set Active";
              actionButton.IsEnabled = true;
              break;
            case VersionState.Copy_Pending:
            case VersionState.Copying:
              actionButton.Content = "Play";
              actionButton.IsEnabled = false;
              break;
            case VersionState.Active:
              SetProgress(0.0);
              actionButton.Content = "Play";
              actionButton.IsEnabled = true;
              break;
          }
        });
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      var thread = new Thread(() => { mainWindow.VersionButton_OnClick(this); });
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
    }
    #endregion
  }
}