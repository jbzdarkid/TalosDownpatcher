using System.Collections.Generic;
using System.Windows;

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public Dictionary<int, Idk> data = new Dictionary<int, Idk>();

    public MainWindow() {
      InitializeComponent();

      List<int> versions = new List<int> { 244371, 301136, 326589, 429074 };

      for (int i = 0; i < versions.Count; i++) {
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
