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
      if (new DirectoryInfo(ActiveBox.Text).Exists) {
        settings.activeVersionLocation = ActiveBox.Text;
      }
      if (new DirectoryInfo(InactiveBox.Text).Exists) {
        settings.oldVersionLocation = InactiveBox.Text;
      }
      settings.Save(); // Writes to disk
    }

    private void ButtonDefault_Click(object sender, RoutedEventArgs e) {
      settings.Reset(); // TODO: Does this modify on-disk data? Ideally not, that should only happen when we save.
    }
  }
}
