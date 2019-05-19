using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TalosDownpatcher {
  public enum VersionState {
    Not_Downloaded,
    Corrupt,
    Download_Pending,
    Downloading,
    Downloaded,
    Copying,
    Active,
  };

  public class VersionUIComponent {
    private Dispatcher dispatcher;
    private DepotManager depotManager;

    private VersionState state;
    private readonly int version;
    private TextBox versionBox;
    private Rectangle downloadBar;
    private TextBox stateBox;
    private Button actionButton;

    public VersionUIComponent(int version, int yPos, MainWindow mainWindow) {
      this.version = version;

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

      dispatcher = mainWindow.Dispatcher;
      depotManager = mainWindow.depotManager;

      double downloadFraction = depotManager.GetDownloadFraction(this.version, false);
      if (downloadFraction == 0.0) {
        UpdateState(VersionState.Not_Downloaded);
      } else if (downloadFraction == 1.0) {
        UpdateState(VersionState.Downloaded);
      } else {
        Console.WriteLine($"Version {version} is {downloadFraction} downloaded -- marking as corrupt");
        UpdateState(VersionState.Corrupt);
      }
    }

    private void Download() {
      UpdateState(VersionState.Download_Pending);
      depotManager.DownloadDepotsForVersion(this.version, delegate {
        dispatcher.Invoke(() => {
          UpdateState(VersionState.Downloading);
          Application.Current.MainWindow.Activate();
        });
      }, delegate (double fractionDownloaded) {
        dispatcher.Invoke(() => {
          this.downloadBar.Width = stateBox.Width * fractionDownloaded;
        });
      });
      UpdateState(VersionState.Downloaded);
    }

    private void SetActive() {
      UpdateState(VersionState.Copying);
      var version = depotManager.TrySetActiveVersion(this.version);
      if (version != this.version) {
        Console.WriteLine($"Version {version} is already running!");
        return;
      }
      UpdateState(VersionState.Active);
    }

    public void LaunchGame() {
      if (version <= 249740) DateUtils.SetYears(-3);
      SteamCommand.StartGame();
      Thread.Sleep(5000);
      if (version <= 249740) DateUtils.SetYears(+3);
    }

    public void UpdateState(VersionState newState) {
      dispatcher.Invoke(() => {
        this.state = newState;
        stateBox.Text = newState.ToString().Replace('_', ' ');
        switch (newState) {
          case VersionState.Not_Downloaded:
            actionButton.Content = "Download";
            break;
          case VersionState.Corrupt:
            actionButton.Content = "Redownload";
            break;
          case VersionState.Download_Pending:
          case VersionState.Downloading:
            actionButton.Content = "Set Active";
            actionButton.IsEnabled = false;
            break;
          case VersionState.Downloaded:
            downloadBar.Width = 1;
            actionButton.Content = "Set Active";
            actionButton.IsEnabled = true;
            break;
          case VersionState.Copying:
            actionButton.Content = "Play";
            actionButton.IsEnabled = false;
            break;
          case VersionState.Active:
            actionButton.Content = "Play";
            actionButton.IsEnabled = true;
            break;
        }
      });
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      switch (this.state) {
        case VersionState.Not_Downloaded:
        case VersionState.Corrupt:
          new Thread(Download) { IsBackground = true }.Start();
          break;
        case VersionState.Downloaded:
          new Thread(SetActive) { IsBackground = true }.Start();
          break;
        case VersionState.Active:
          new Thread(LaunchGame) { IsBackground = true }.Start();
          break;
      }
    }
  }
}