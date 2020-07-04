using MonoDevelop.Ide.Gui.Dialogs;
using MonoDevelop.Components;
using MonoDevelop.Core;

namespace GodotAddin
{
    public class GodotOptionsPanel : OptionsPanel
    {
        private readonly FileEntry _godotExeFile = new FileEntry();
        private readonly Gtk.CheckButton _alwaysUseExe = new Gtk.CheckButton { BorderWidth = 10 };

        private readonly Gtk.CheckButton _provideNodePathCompletion = new Gtk.CheckButton { BorderWidth = 10 };
        private readonly Gtk.CheckButton _provideInputActionCompletion = new Gtk.CheckButton { BorderWidth = 10 };
        private readonly Gtk.CheckButton _provideResourcePathCompletion = new Gtk.CheckButton { BorderWidth = 10 };
        private readonly Gtk.CheckButton _provideScenePathCompletion = new Gtk.CheckButton { BorderWidth = 10 };
        private readonly Gtk.CheckButton _provideSignalNameCompletion = new Gtk.CheckButton { BorderWidth = 10 };

        private void AddSection(Gtk.VBox vbox, string text)
        {
            var hbox = new Gtk.HBox();

            var sectionLabel = new Gtk.Label
            {
                Text = $"<b>{GettextCatalog.GetString(text)}</b>",
                UseMarkup = true,
                Xalign = 0
            };

            hbox.PackStart(sectionLabel, false, false, 0);

            vbox.PackStart(hbox, false, false, 0);
        }

        private static void AddFileProperty(Gtk.VBox vbox, string labelText, FileEntry fileEntry, ConfigurationProperty<string> property)
        {
            var alignment = new Gtk.Alignment(0f, 0f, 1f, 1f) { LeftPadding = 24 };

            var innerVBox = new Gtk.VBox();
            alignment.Add(innerVBox);

            var hbox = new Gtk.HBox(false, 6);

            var label = new Gtk.Label
            {
                Text = GettextCatalog.GetString(labelText),
                Xalign = 0
            };

            hbox.PackStart(label, false, false, 0);
            fileEntry.Path = property.Value;
            hbox.PackStart(fileEntry, true, true, 0);

            innerVBox.PackStart(hbox, false, false, 0);

            vbox.PackStart(alignment, false, false, 0);
        }

        private static void AddCheckProperty(Gtk.VBox vbox, string labelText, Gtk.CheckButton checkButton, ConfigurationProperty<bool> property)
        {
            var hbox = new Gtk.HBox();

            checkButton.Active = property.Value;
            hbox.PackStart(checkButton, false, false, 0);

            var label = new Gtk.Label
            {
                Text = GettextCatalog.GetString(labelText),
                Xalign = 0
            };

            hbox.PackStart(label, true, true, 0);

            vbox.PackStart(hbox, false, false, 0);
        }

        public override Control CreatePanelWidget()
        {
            var vbox = new Gtk.VBox { Spacing = 6 };

            AddSection(vbox, "Debugging");

            AddCheckProperty(vbox, "Always use this executable.", _alwaysUseExe, Settings.AlwaysUseConfiguredExecutable);
            AddFileProperty(vbox, "Godot executable:", _godotExeFile, Settings.GodotExecutablePath);

            AddSection(vbox, "Code completion");

            AddCheckProperty(vbox, "Provide node path completions.", _provideNodePathCompletion, Settings.ProvideNodePathCompletions);
            AddCheckProperty(vbox, "Provide input action completions.", _provideInputActionCompletion, Settings.ProvideInputActionCompletions);
            AddCheckProperty(vbox, "Provide resource path completions.", _provideResourcePathCompletion, Settings.ProvideResourcePathCompletions);
            AddCheckProperty(vbox, "Provide scene path completions.", _provideScenePathCompletion, Settings.ProvideScenePathCompletions);
            AddCheckProperty(vbox, "Provide signal name completions.", _provideSignalNameCompletion, Settings.ProvideSignalNameCompletions);

            vbox.ShowAll();

            return vbox;
        }

        public override void ApplyChanges()
        {
            Settings.GodotExecutablePath.Value = _godotExeFile.Path;
            Settings.AlwaysUseConfiguredExecutable.Value = _alwaysUseExe.Active;

            Settings.ProvideNodePathCompletions.Value = _provideNodePathCompletion.Active;
            Settings.ProvideInputActionCompletions.Value = _provideInputActionCompletion.Active;
            Settings.ProvideResourcePathCompletions.Value = _provideResourcePathCompletion.Active;
            Settings.ProvideScenePathCompletions.Value = _provideScenePathCompletion.Active;
            Settings.ProvideSignalNameCompletions.Value = _provideSignalNameCompletion.Active;
        }
    }
}
