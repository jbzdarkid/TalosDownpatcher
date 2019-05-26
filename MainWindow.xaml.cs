using System.Windows;
// TODO: SetActive needs work when used twice. We seem to get stuck in Copying (and definitely aren't changing state of other components).
// TODO: Editor -- this is Apple's problem to solve.

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public DepotManager depotManager;

    public MainWindow() {
      InitializeComponent();
      this.depotManager = new DepotManager();
      int activeVersion = depotManager.GetActiveVersion();
      for (int i = 0; i < ManifestData.versions.Count; i++) {
        var uiComponent = new VersionUIComponent(ManifestData.versions[i], 30 + 20 * i, this);
        if (ManifestData.versions[i] == activeVersion) {
          uiComponent.state = VersionState.Active;
        }
      }
    }
  }
}
