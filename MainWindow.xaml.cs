using System.Collections.Generic;
using System.Windows;

// TODO: Total file counts are in order, to make sure that we don't exit out early.
// ^ Write a quick python script to get the file counts from apple's dump. Then, create a map of manifest ID: count (and store it in separate file).
// ^ Download %?
// TODO: x64/x86 compat
// TODO: Old versions need a time change (or binary edit :thinking:)
// TODO: Main window state is not reading from file / computing from anywhere
// TODO: Some indicator of 'useful' or 'important' versions.
// TODO: Editor?
// TODO: How on earth does "State: Running" work if you can exit the game outside the launcher?
//        I should probably just have a "start" button and check active processes to make sure we don't do something dumb.

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
        VersionUIComponent idk = new VersionUIComponent(versions[i], 30 + 20 * i, this.Dispatcher, RootGrid, depotManager);
      }
    }
  }
}
