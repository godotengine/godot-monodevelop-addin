using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "GodotAddin",
    Namespace = "GodotAddin",
    Version = "1.1"
)]

[assembly: AddinName("Godot Addin")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Extension for the Godot game engine")]
[assembly: AddinAuthor("Godot Engine contributors")]
