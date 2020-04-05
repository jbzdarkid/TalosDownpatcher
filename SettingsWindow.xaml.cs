using System.IO;
using System.Windows;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  public partial class SettingsWindow : Window {
    private readonly Settings settings = Settings.Default;
    private readonly MainWindow mainWindow;

    public SettingsWindow(MainWindow mainWindow) {
      this.mainWindow = mainWindow;
      InitializeComponent();
      ActiveBox.Text = settings.activeVersionLocation;
      InactiveBox.Text = settings.oldVersionLocation;
      AllVersionsCheckbox.IsChecked = settings.showAllVersions;
      GehennaCheckbox.IsChecked = settings.ownsGehenna;
      PrototypeCheckbox.IsChecked = settings.ownsPrototype;

      AllVersionsLabel.PreviewMouseDown += delegate { AllVersionsCheckbox.IsChecked = !AllVersionsCheckbox.IsChecked; };
      GehennaLabel.PreviewMouseDown += delegate { GehennaCheckbox.IsChecked = !GehennaCheckbox.IsChecked; };
      PrototypeLabel.PreviewMouseDown += delegate { PrototypeCheckbox.IsChecked = !PrototypeCheckbox.IsChecked; };
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e) {
      if (mainWindow.ActionInProgress()) {
        Logging.MessageBox($"Cannot save settings while an operation is in progress.", "Error");
        return;
      }

      if ((!settings.ownsGehenna && (bool)GehennaCheckbox.IsChecked) || (!settings.ownsPrototype && (bool)PrototypeCheckbox.IsChecked)) {
        Logging.MessageBox($"Warning: Attempting to download Prototype or Gehenna without owning them will cause the downpatcher to get stuck while waiting for the download.", "Warning");
      }

      settings.showAllVersions = (bool)AllVersionsCheckbox.IsChecked;
      settings.ownsGehenna = (bool)GehennaCheckbox.IsChecked;
      settings.ownsPrototype = (bool)PrototypeCheckbox.IsChecked;

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
    }
  }
}
