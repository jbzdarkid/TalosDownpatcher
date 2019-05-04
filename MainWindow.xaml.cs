using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

public enum VersionState
{
  Not_Downloaded,
  Downloading,
  Not_Launched,
  Launched,
  Running,
  Corrupt
};

namespace TalosDownpatcher
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public Dictionary<int, VersionState> versionStates;

    public MainWindow()
    {
      InitializeComponent();

      this.versionStates = new Dictionary<int, VersionState>
      {
        {244371, VersionState.Not_Downloaded},
        {301136, VersionState.Not_Downloaded},
        {326589, VersionState.Not_Downloaded},
        {429074, VersionState.Not_Downloaded},
      };


      List<int> versions = new List<int>(this.versionStates.Keys);
      versions.Sort();
      for (int i=0; i<versions.Count; i++) {
        int version = versions[i];
        TextBox versionName = new TextBox();
        RootGrid.Children.Add(versionName);
        versionName.HorizontalAlignment = HorizontalAlignment.Left;
        versionName.VerticalAlignment = VerticalAlignment.Top;
        versionName.Height = 21;
        versionName.Width = 51;
        versionName.Margin = new Thickness(10, 30 + 20*i, 0, 0);
        versionName.Text = version.ToString();

        TextBox state = new TextBox();
        RootGrid.Children.Add(state);
        state.HorizontalAlignment = HorizontalAlignment.Left;
        state.VerticalAlignment = VerticalAlignment.Top;
        state.Height = 21;
        state.Width = 101;
        state.Margin = new Thickness(60, 30 + 20*i, 0, 0);
        state.Text = versionStates[version].ToString();

        Button action = new Button();
        RootGrid.Children.Add(action);
        action.HorizontalAlignment = HorizontalAlignment.Left;
        action.VerticalAlignment = VerticalAlignment.Top;
        action.Height = 21;
        action.Width = 71;
        action.Click += Button_Click;
        action.Tag = version;
        action.Margin = new Thickness(160, 30 + 20*i, 0, 0);
        switch (versionStates[version])
        {
          case VersionState.Not_Downloaded:
            action.Content = "Download";
            break;
          case VersionState.Downloading:
            action.Content = "Play";
            action.IsEnabled = false;
            break;
          case VersionState.Not_Launched:
          case VersionState.Launched:
            action.Content = "Play";
            action.IsEnabled = true;
            break;
          case VersionState.Running:
            action.Content = "Stop";
            break;
          case VersionState.Corrupt:
            action.Content = "Redownload";
            break;
        }
      }

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      String text = (String)((Button)sender).Content;

      Console.WriteLine(sender + " " + e);
    }
  }
}
