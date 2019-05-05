using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

public enum VersionState
{
  Not_Downloaded,
  Downloading,
  Never_Launched,
  Launched,
  Running,
  Corrupt
};

public class Idk
{
  public VersionState state;
  public int version;
  public Dispatcher dispatcher;

  public TextBox versionBox;
  public TextBox stateBox;
  public Button actionButton;

  public Idk(int version, int yPos)
  {
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

  public void Download()
  {
    Thread.Sleep(5000);
    Console.WriteLine("Downloaded");
    dispatcher.Invoke(() => {
      UpdateState(VersionState.Never_Launched);
    });
  }

  public void UpdateState(VersionState newState)
  {
    this.state = newState;
    stateBox.Text = this.state.ToString();
    switch (this.state)
    {
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
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    switch (this.state)
    {
      case VersionState.Not_Downloaded:
      case VersionState.Corrupt:
        Console.WriteLine("Downloading...");
        Thread downloadThread = new Thread(Download);
        downloadThread.Start();
        UpdateState(VersionState.Downloading);
        break;
      case VersionState.Downloading:
        Console.WriteLine("Can't play game yet, still downloading (and the button is disabled, how did you click this?)");
        break;
      case VersionState.Never_Launched:
      case VersionState.Launched:
        Console.WriteLine("Starting game...");
        UpdateState(VersionState.Running);
        break;
      case VersionState.Running:
        Console.WriteLine("Stopping game...");
        UpdateState(VersionState.Launched);
        break;
    }
  }
}

namespace TalosDownpatcher
{
  public partial class MainWindow : Window
  {
    public Dictionary<int, Idk> data = new Dictionary<int, Idk>();

    public MainWindow()
    {
      InitializeComponent();

      List<int> versions = new List<int>{244371, 301136, 326589, 429074};

      for (int i=0; i<versions.Count; i++) {
        int version = versions[i];

        Idk idk = new Idk(version, 30 + 20 * i);
        idk.dispatcher = this.Dispatcher;
        idk.UpdateState(VersionState.Not_Downloaded);

        // this is not encapsulation. idk though
        RootGrid.Children.Add(idk.versionBox);
        RootGrid.Children.Add(idk.stateBox);
        RootGrid.Children.Add(idk.actionButton);

        this.data[version] = idk;
      }
    }
  }
}
