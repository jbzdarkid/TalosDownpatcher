using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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

  // TODO: Ideally all of these would be private...
  public Dispatcher dispatcher;
  public TextBox versionBox;
  public TextBox stateBox;
  public Button actionButton;

  private VersionState state;
  private readonly int version;

  public Idk(int version, int yPos) {
    this.version = version;

    versionBox = new TextBox();
    versionBox.HorizontalAlignment = HorizontalAlignment.Left;
    versionBox.VerticalAlignment = VerticalAlignment.Top;
    versionBox.Height = 21;
    versionBox.Width = 51;
    versionBox.Margin = new Thickness(10, yPos, 0, 0);
    versionBox.Text = version.ToString();

    stateBox = new TextBox();
    stateBox.HorizontalAlignment = HorizontalAlignment.Left;
    stateBox.VerticalAlignment = VerticalAlignment.Top;
    stateBox.Height = 21;
    stateBox.Width = 101;
    stateBox.Margin = new Thickness(60, yPos, 0, 0);

    actionButton = new Button();
    actionButton.HorizontalAlignment = HorizontalAlignment.Left;
    actionButton.VerticalAlignment = VerticalAlignment.Top;
    actionButton.Height = 21;
    actionButton.Width = 71;
    actionButton.Click += Button_Click;
    actionButton.Margin = new Thickness(160, yPos, 0, 0);
  }

  // TODO: Stub
  private void Download() {
    UpdateState(VersionState.Downloading);
    Console.WriteLine("Downloading...");

    Thread.Sleep(5000);

    UpdateState(VersionState.Never_Launched);
    Console.WriteLine("Downloaded");
  }

  // TODO: Stub
  private void StartGame() {
    if (activeVersion == 0) {
      activeVersion = version;
    } else if (activeVersion != version) {
      Console.WriteLine("Version " + activeVersion + " is already running!");
      return;
    }

    UpdateState(VersionState.Running);
    activeVersion = version;
    Console.WriteLine("Starting game...");
  }

  // TODO: Stub
  private void StopGame() {
    UpdateState(VersionState.Launched);
    activeVersion = 0;
    Console.WriteLine("Stopping game...");
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
