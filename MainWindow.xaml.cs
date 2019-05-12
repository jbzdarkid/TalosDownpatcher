using System.Collections.Generic;
using System.Windows;

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();

      List<int> versions = new List<int> { 244371, 301136, 326589, 429074 };
      DepotManager depotManager = new DepotManager();

      for (int i = 0; i < versions.Count; i++) {
        Idk idk = new Idk(versions[i], 30 + 20 * i, this.Dispatcher, RootGrid, depotManager);
      }
    }
  }
}
