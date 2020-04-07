using System.IO;
using System.Windows;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  public partial class SettingsWindow : Window {
    private readonly Settings settings = Settings.Default;
    private readonly MainWindow mainWindow;

    public SettingsWindow(MainWindow mainWindow, bool showHiddenSettings) {
      this.mainWindow = mainWindow;
      InitializeComponent();
      // Set all the settings based on the saved settings
      ActiveBox.Text = settings.activeVersionLocation;
      InactiveBox.Text = settings.oldVersionLocation;
      AllVersionsCheckbox.IsChecked = settings.showAllVersions;
      GehennaCheckbox.IsChecked = settings.ownsGehenna;
      PrototypeCheckbox.IsChecked = settings.ownsPrototype;
      LaunchModdable.IsChecked = settings.launchModdable;
      UseSymlinks.IsChecked = settings.useSymlinks;
      SteamHack.IsChecked = settings.steamHack;

      // Add hooks so that clicking on text toggles checkboxes
      AllVersionsLabel.PreviewMouseDown += delegate { AllVersionsCheckbox.IsChecked = !AllVersionsCheckbox.IsChecked; };
      GehennaLabel.PreviewMouseDown += delegate { GehennaCheckbox.IsChecked = !GehennaCheckbox.IsChecked; };
      PrototypeLabel.PreviewMouseDown += delegate { PrototypeCheckbox.IsChecked = !PrototypeCheckbox.IsChecked; };
      ModdableLabel.PreviewMouseDown += delegate { LaunchModdable.IsChecked = !LaunchModdable.IsChecked; };
      SymlinkLabel.PreviewMouseDown += delegate { UseSymlinks.IsChecked = !UseSymlinks.IsChecked; };
      HackLabel.PreviewMouseDown += delegate { SteamHack.IsChecked = !SteamHack.IsChecked; };

      UseSymlinks.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
      SymlinkLabel.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
      SteamHack.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
      HackLabel.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e) {
      if (mainWindow.ActionInProgress()) {
        Logging.MessageBox($"Cannot save settings while an operation is in progress.", "Error");
        return;
      }

      if ((!settings.ownsGehenna && (bool)GehennaCheckbox.IsChecked) || (!settings.ownsPrototype && (bool)PrototypeCheckbox.IsChecked)) {
        Logging.MessageBox($"Warning: Attempting to download Prototype or Gehenna without owning them will cause the downpatcher to get stuck while waiting for the download.", "Warning");
      }

      if (!string.IsNullOrWhiteSpace(ActiveBox.Text)) {
        var dir = new DirectoryInfo(ActiveBox.Text);
        if (!dir.Exists) dir.Create();
        settings.activeVersionLocation = ActiveBox.Text;
      }

      if (!string.IsNullOrWhiteSpace(InactiveBox.Text)) {
        var dir = new DirectoryInfo(InactiveBox.Text);
        if (!dir.Exists) dir.Create();
        settings.oldVersionLocation = InactiveBox.Text;
      }

      settings.showAllVersions = (bool)AllVersionsCheckbox.IsChecked;
      settings.ownsGehenna = (bool)GehennaCheckbox.IsChecked;
      settings.ownsPrototype = (bool)PrototypeCheckbox.IsChecked;
      settings.launchModdable = (bool)LaunchModdable.IsChecked;
      settings.useSymlinks = (bool)UseSymlinks.IsChecked;
      settings.steamHack = (bool)SteamHack.IsChecked;

      settings.Save(); // Writes to disk
      mainWindow.LoadVersions(); // Reload versions for potential changes
      Close();
    }

    // Restores default settings without saving -- user must explicitly 'save and close'
    private void ButtonDefault_Click(object sender, RoutedEventArgs e) {
      ActiveBox.Text = settings.Properties["activeVersionLocation"].DefaultValue as string;
      InactiveBox.Text = settings.Properties["oldVersionLocation"].DefaultValue as string;
      AllVersionsCheckbox.IsChecked = false;
      GehennaCheckbox.IsChecked = false;
      PrototypeCheckbox.IsChecked = false;
      LaunchModdable.IsChecked = false;
      UseSymlinks.IsChecked = false;
      SteamHack.IsChecked = false;
    }
  }
}
