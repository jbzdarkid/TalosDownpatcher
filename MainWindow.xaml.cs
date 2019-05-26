using System;
using System.Collections.Generic;
using System.Windows;
// TODO: SetActive needs work when used twice. We seem to get stuck in Copying (and definitely aren't changing state of other components).
// TODO: Editor -- this is Apple's problem to solve.

namespace TalosDownpatcher {
  public partial class MainWindow : Window {
    public DepotManager depotManager;
    private Dictionary<int, VersionUIComponent> uiComponents;

    public MainWindow() {
      InitializeComponent();
      this.depotManager = new DepotManager();
      int activeVersion = depotManager.GetActiveVersion();
      for (int i = 0; i < ManifestData.versions.Count; i++) {
        int version = ManifestData.versions[i];
        var uiComponent = new VersionUIComponent(version, 30 + 20 * i, this);

        double downloadFraction = depotManager.GetDownloadFraction(version, false);
        if (downloadFraction == 1.0) {
          if (ManifestData.versions[i] == activeVersion) {
            uiComponent.state = VersionState.Active;
          } else {
            uiComponent.state = VersionState.Downloaded;
          }
        } else if (downloadFraction == 0.0) {
          uiComponent.state = VersionState.Not_Downloaded;
        } else {
          Console.WriteLine($"Version {version} is {downloadFraction} downloaded -- marking as corrupt");
          uiComponent.state = VersionState.Corrupt;
        }
        // uiComponents[version] = uiComponent;
      }
    }
  }
}
