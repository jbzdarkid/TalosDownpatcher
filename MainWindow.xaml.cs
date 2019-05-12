using System.Collections.Generic;
using System.Windows;

// TODO: x64/x86 compat
// TODO: On launch, change time
// TODO: Only copy files I actually want (i.e. leave other files untouched)
// ^ So maybe "CopyAndReplace" isn't actually correct?

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();

      List<int> versions = new List<int> {
        429074, 426014, 424910, 326589, 301136, 300763, 293384,
        291145, 284152, 277544, 269335, 267252, 264510, 260924, 258375, 252786,
        250756, 249913, 249740, 248828, 248139, 246379, 244371, 243520, 226087,
        224995, 224531, 223249, 222477, 221394, 220996, 220675, 220625, 220480};
      DepotManager depotManager = new DepotManager();

      for (int i = 0; i < versions.Count; i++) {
        Idk idk = new Idk(versions[i], 30 + 20 * i, this.Dispatcher, RootGrid, depotManager);
      }
    }
  }
}
