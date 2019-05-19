using System.Collections.Generic;
using System.Windows;

// TODO: Total file counts are in order, to make sure that we don't exit out early.
// ^ Write a quick python script to get the file counts from apple's dump. Then, create a map of manifest ID: count (and store it in separate file).
// ^ Download %?
// Also added hashing here. Why not.
// TODO: x64/x86 compat
// TODO: Old versions need a time change (or binary edit :thinking:)
// TODO: Main window state is not reading from file / computing from anywhere
// ^ Should be able to just run hashes now that I "have" them.
// TODO: Some indicator of 'useful' or 'important' versions.
// TODO: Editor?
// TODO: How on earth does "State: Running" work if you can exit the game outside the launcher?
//        I should probably just have a "start" button and check active processes to make sure we don't do something dumb.

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public DepotManager depotManager;

    public MainWindow() {
      InitializeComponent();
      this.depotManager = new DepotManager();
      for (int i = 0; i < Manifests.versions.Count; i++) {
        new VersionUIComponent(Manifests.versions[i], 30 + 20 * i, this);
      }
    }
  }
}
