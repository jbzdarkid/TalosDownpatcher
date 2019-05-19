using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TalosDownpatcher {
  public enum VersionState {
    Not_Downloaded,
    Downloading,
    Copying,
    Never_Launched,
    Launched,
    Running,
    Corrupt
  };

  public class VersionUIComponent {
    public static int activeVersion = 0;

    private VersionState state;
    private readonly int version;
    private Dispatcher dispatcher;
    private TextBox versionBox;
    private TextBox stateBox;
    private Button actionButton;
    private DepotManager depotManager;

    public VersionUIComponent(int version, int yPos, MainWindow mainWindow) {
      this.version = version;

      versionBox = new TextBox();
      versionBox.HorizontalAlignment = HorizontalAlignment.Left;
      versionBox.VerticalAlignment = VerticalAlignment.Top;
      versionBox.Height = 21;
      versionBox.Width = 51;
      versionBox.Margin = new Thickness(10, yPos, 0, 0);
      versionBox.Text = version.ToString();
      mainWindow.RootGrid.Children.Add(this.versionBox);

      stateBox = new TextBox();
      stateBox.HorizontalAlignment = HorizontalAlignment.Left;
      stateBox.VerticalAlignment = VerticalAlignment.Top;
      stateBox.Height = 21;
      stateBox.Width = 101;
      stateBox.Margin = new Thickness(60, yPos, 0, 0);
      mainWindow.RootGrid.Children.Add(this.stateBox);

      actionButton = new Button();
      actionButton.HorizontalAlignment = HorizontalAlignment.Left;
      actionButton.VerticalAlignment = VerticalAlignment.Top;
      actionButton.Height = 21;
      actionButton.Width = 71;
      actionButton.Click += Button_Click;
      actionButton.Margin = new Thickness(160, yPos, 0, 0);
      mainWindow.RootGrid.Children.Add(this.actionButton);

      this.dispatcher = mainWindow.Dispatcher;
      this.UpdateState(VersionState.Not_Downloaded);
      this.depotManager = mainWindow.depotManager;
    }

    private void Download() {
      UpdateState(VersionState.Downloading);
      depotManager.DownloadDepotsForVersion(this.version);
      UpdateState(VersionState.Never_Launched);
    }

    private void StartGame() {
      UpdateState(VersionState.Copying);
      var version = depotManager.TrySetActiveVersion(this.version);
      if (version != this.version) {
        Console.WriteLine($"Version {version} is already running!");
        return;
      }

      UpdateState(VersionState.Running);
      SteamCommand.StartGame();
    }

    private void StopGame() {
      UpdateState(VersionState.Launched);
      SteamCommand.StopGame();
    }

    public void UpdateState(VersionState newState) {
      dispatcher.Invoke(() => {
        Console.WriteLine($"Transitioning from state {this.state} to {newState}");
        this.state = newState;
        stateBox.Text = newState.ToString().Replace('_', ' ');
        switch (newState) {
          case VersionState.Not_Downloaded:
            actionButton.Content = "Download";
            break;
          case VersionState.Downloading:
            actionButton.Content = "Play";
            actionButton.IsEnabled = false;
            break;
          case VersionState.Never_Launched:
          case VersionState.Launched:
            actionButton.Content = "Play";
            actionButton.IsEnabled = true;
            break;
          case VersionState.Copying:
            actionButton.Content = "Starting";
            actionButton.IsEnabled = false;
            // TODO: Potentially allow for "stop copying" here. Simplest solution is to go the depotManager and stop from there.
            break;
          case VersionState.Running:
            actionButton.Content = "Stop";
            break;
          case VersionState.Corrupt:
            actionButton.Content = "Redownload";
            break;
        }
      });
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      switch (this.state) {
        case VersionState.Not_Downloaded:
        case VersionState.Corrupt:
          new Thread(Download).Start();
          break;
        case VersionState.Never_Launched:
        case VersionState.Launched:
          StartGame();
          break;
        case VersionState.Running:
          StopGame();
          break;
      }
    }
  }
}