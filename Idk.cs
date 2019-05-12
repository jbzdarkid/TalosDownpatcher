using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TalosDownpatcher {
  public enum VersionState {
    Not_Downloaded,
    Downloading,
    Never_Launched,
    Launched,
    Running,
    Corrupt
  };

  public class Idk {
    public static int activeVersion = 0;

    private VersionState state;
    private readonly int version;
    private Dispatcher dispatcher;
    private TextBox versionBox;
    private TextBox stateBox;
    private Button actionButton;
    private DepotManager depotManager;

    public Idk(int version, int yPos, Dispatcher dispatcher, Grid RootGrid, DepotManager depotManager) {
      this.version = version;

      versionBox = new TextBox();
      versionBox.HorizontalAlignment = HorizontalAlignment.Left;
      versionBox.VerticalAlignment = VerticalAlignment.Top;
      versionBox.Height = 21;
      versionBox.Width = 51;
      versionBox.Margin = new Thickness(10, yPos, 0, 0);
      versionBox.Text = version.ToString();
      RootGrid.Children.Add(this.versionBox);

      stateBox = new TextBox();
      stateBox.HorizontalAlignment = HorizontalAlignment.Left;
      stateBox.VerticalAlignment = VerticalAlignment.Top;
      stateBox.Height = 21;
      stateBox.Width = 101;
      stateBox.Margin = new Thickness(60, yPos, 0, 0);
      RootGrid.Children.Add(this.stateBox);

      actionButton = new Button();
      actionButton.HorizontalAlignment = HorizontalAlignment.Left;
      actionButton.VerticalAlignment = VerticalAlignment.Top;
      actionButton.Height = 21;
      actionButton.Width = 71;
      actionButton.Click += Button_Click;
      actionButton.Margin = new Thickness(160, yPos, 0, 0);
      RootGrid.Children.Add(this.actionButton);

      this.UpdateState(VersionState.Not_Downloaded);
      this.dispatcher = dispatcher;
      this.depotManager = depotManager;
    }

    private void Download() {
      UpdateState(VersionState.Downloading);
      Console.WriteLine("Downloading...");
      // TODO: Jump to a background thread.
      depotManager.DownloadDepotsForVersion(this.version);

      UpdateState(VersionState.Never_Launched);
      Console.WriteLine("Downloaded");
    }

    private void StartGame() {
      if (activeVersion == 0) {
        activeVersion = version;
      } else if (activeVersion != version) {
        Console.WriteLine("Version " + activeVersion + " is already running!");
        return;
      }

      UpdateState(VersionState.Running);
      activeVersion = version;
      SteamCommand.StartGame();
    }

    private void StopGame() {
      UpdateState(VersionState.Launched);
      activeVersion = 0;
      SteamCommand.StopGame();
    }

    public void UpdateState(VersionState newState) {
      dispatcher.Invoke(() => {
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
          Download();
          break;
        case VersionState.Downloading:
          Console.WriteLine("Can't play game yet, still downloading (and the button is disabled, how did you click this?)");
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