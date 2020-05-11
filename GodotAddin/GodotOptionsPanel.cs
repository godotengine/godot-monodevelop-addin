using MonoDevelop.Ide.Gui.Dialogs;
using MonoDevelop.Components;
using MonoDevelop.Core;

namespace GodotAddin
{
    public class GodotOptionsPanel : OptionsPanel
    {
        private readonly FileEntry _godotExeFileEntry = new FileEntry();
        private readonly Gtk.CheckButton _alwaysUseExeCheckButton = new Gtk.CheckButton();

        public override Control CreatePanelWidget()
        {
            var vbox = new Gtk.VBox { Spacing = 6 };

            var generalSectionLabel = new Gtk.Label
            {
                Text = $"<b>{GettextCatalog.GetString("General")}</b>",
                UseMarkup = true,
                Xalign = 0
            };

            vbox.PackStart(generalSectionLabel, false, false, 0);

            var godotExeHBox = new Gtk.HBox { BorderWidth = 10, Spacing = 6 };

            var godotExeLabel = new Gtk.Label
            {
                Text = GettextCatalog.GetString("Godot executable:"),
                Xalign = 0
            };

            godotExeHBox.PackStart(godotExeLabel, false, false, 0);
            _godotExeFileEntry.Path = Settings.GodotExecutablePath.Value;
            godotExeHBox.PackStart(_godotExeFileEntry, true, true, 0);

            vbox.PackStart(godotExeHBox, false, false, 0);

            var alwaysUseExeHBox = new Gtk.HBox { BorderWidth = 10, Spacing = 6 };

            var alwaysUseExeLabel = new Gtk.Label
            {
                Text = GettextCatalog.GetString("Always use this executable:"),
                Xalign = 0
            };

            alwaysUseExeHBox.PackStart(alwaysUseExeLabel, false, false, 0);
            _alwaysUseExeCheckButton.Active = Settings.AlwaysUseConfiguredExecutable.Value;
            alwaysUseExeHBox.PackStart(_alwaysUseExeCheckButton, true, true, 0);

            vbox.PackStart(alwaysUseExeHBox, false, false, 0);

            vbox.ShowAll();

            return vbox;
        }

        public override void ApplyChanges()
        {
            Settings.GodotExecutablePath.Value = _godotExeFileEntry.Path;
            Settings.GodotExecutablePath.Value = _godotExeFileEntry.Path;
        }
    }
}
