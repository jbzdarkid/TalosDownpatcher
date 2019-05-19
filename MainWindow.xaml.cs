using System.Windows;

// WIP: Old versions need a time change (or binary edit :thinking:)
// TODO: x64/x86 compat (Might need buttons, seems like steam just launches 'default'). 
// TODO: Remember active version to avoid a copy.
// TODO: Editor?

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
