using BlueLine.Management;
using Godot;

namespace BlueLine;

public partial class MainMenu : CanvasLayer
{
    [Export] public Button StartButton;
    [Export] public Button QuitButton;
    [Export] public PackedScene TeamSelectScene;

    public override void _Ready()
    {
        StartButton.Pressed += OnStartPressed;
        QuitButton.Pressed += OnQuitPressed;
        StartButton.GrabFocus(); // see note below
        MatchStatus.Instance.State = GameState.MainMenu;
    }

    private void OnStartPressed() => GetTree().ChangeSceneToPacked(TeamSelectScene);
    private void OnQuitPressed() => GetTree().Quit();
}