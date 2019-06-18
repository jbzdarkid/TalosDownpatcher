using System.IO;
using System.Windows;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  public partial class SettingsWindow : Window {
    private readonly Settings settings = Settings.Default;

    public SettingsWindow() {
      InitializeComponent();
      ActiveBox.Text = settings.activeVersionLocation;
      InactiveBox.Text = settings.oldVersionLocation;
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e) {
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
      Close();
    }

    // Restores default settings without saving -- user must explicitly 'save and close'
    private void ButtonDefault_Click(object sender, RoutedEventArgs e) {
      ActiveBox.Text = settings.Properties["activeVersionLocation"].DefaultValue as string;
      InactiveBox.Text = settings.Properties["oldVersionLocation"].DefaultValue as string;
    }
  }
}
