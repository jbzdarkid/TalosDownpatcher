using System.IO;
using System.Windows;
using WinForms = System.Windows.Forms;
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
      EditorCheckbox.IsChecked = settings.wantsEditor;
      ModdableCheckbox.IsChecked = settings.launchModdable;
      SymlinkCheckbox.IsChecked = settings.useSymlinks;
      HackCheckbox.IsChecked = settings.steamHack;

      // Add hooks so that clicking on text toggles checkboxes
      AllVersionsLabel.PreviewMouseDown += delegate { AllVersionsCheckbox.IsChecked = !AllVersionsCheckbox.IsChecked; };
      GehennaLabel.PreviewMouseDown += delegate { GehennaCheckbox.IsChecked = !GehennaCheckbox.IsChecked; };
      PrototypeLabel.PreviewMouseDown += delegate { PrototypeCheckbox.IsChecked = !PrototypeCheckbox.IsChecked; };
      EditorLabel.PreviewMouseDown += delegate { EditorCheckbox.IsChecked = !EditorCheckbox.IsChecked; };
      ModdableLabel.PreviewMouseDown += delegate { ModdableCheckbox.IsChecked = !ModdableCheckbox.IsChecked; };
      SymlinkLabel.PreviewMouseDown += delegate { SymlinkCheckbox.IsChecked = !SymlinkCheckbox.IsChecked; };
      HackLabel.PreviewMouseDown += delegate { HackCheckbox.IsChecked = !HackCheckbox.IsChecked; };

      if (!showHiddenSettings) Height -= 26; // Height of the final row
      SymlinkCheckbox.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
      SymlinkLabel.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
      HackCheckbox.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
      HackLabel.Visibility = showHiddenSettings ? Visibility.Visible : Visibility.Hidden;
    }

    private void SetActiveLocation(string location) {
      if (!string.IsNullOrWhiteSpace(location)) {
        var dir = new DirectoryInfo(location);
        if (!dir.Exists) dir.Create();
        settings.activeVersionLocation = location;
        ActiveBox.Text = location;
      }
    }

    private void SetInactiveLocation(string location) {
      if (!string.IsNullOrWhiteSpace(location)) {
        var dir = new DirectoryInfo(location);
        if (!dir.Exists) dir.Create();
        settings.oldVersionLocation = location;
        InactiveBox.Text = location;
      }
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e) {
      if (mainWindow.ActionInProgress()) {
        Logging.MessageBox("Error", $"Cannot save settings while an operation is in progress.");
        return;
      }

      // If the user can download depots, and they're just now turning on gehenna or prototype, warn them about hangs.
      if ((bool)HackCheckbox.IsChecked) {
        if ((!settings.ownsGehenna && (bool)GehennaCheckbox.IsChecked)
          || (!settings.ownsPrototype && (bool)PrototypeCheckbox.IsChecked)) {
          Logging.MessageBox("Warning", $"Warning: Attempting to download Prototype or Gehenna without owning them will cause the downpatcher to get stuck while waiting for the download.");
        }
      }

      SetActiveLocation(ActiveBox.Text);
      SetInactiveLocation(InactiveBox.Text);

      settings.showAllVersions = (bool)AllVersionsCheckbox.IsChecked;
      settings.ownsGehenna = (bool)GehennaCheckbox.IsChecked;
      settings.ownsPrototype = (bool)PrototypeCheckbox.IsChecked;
      settings.wantsEditor = (bool)EditorCheckbox.IsChecked;
      settings.launchModdable = (bool)ModdableCheckbox.IsChecked;
      settings.useSymlinks = (bool)SymlinkCheckbox.IsChecked;
      settings.steamHack = (bool)HackCheckbox.IsChecked;

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
      EditorCheckbox.IsChecked = false;
      ModdableCheckbox.IsChecked = false;
      SymlinkCheckbox.IsChecked = false;
      HackCheckbox.IsChecked = false;
    }

    private void ActiveLocation_Click(object sender, RoutedEventArgs e) {
      using (var dialog = new WinForms.FolderBrowserDialog()) {
        if (dialog.ShowDialog() == WinForms.DialogResult.OK) {
          SetActiveLocation(dialog.SelectedPath);
        }
      }
    }

    private void InactiveLocation_Click(object sender, RoutedEventArgs e) {
      using (var dialog = new WinForms.FolderBrowserDialog()) {
        if (dialog.ShowDialog() == WinForms.DialogResult.OK) {
          SetInactiveLocation(dialog.SelectedPath);
        }
      }
    }
  }
}
