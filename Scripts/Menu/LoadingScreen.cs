using Godot;
using System;

public partial class LoadingScreen : CanvasLayer
{
	[Export] public PackedScene SceneToLoad;

	public override void _Process(double delta)
	{
		GetTree().ChangeSceneToPacked(SceneToLoad);
	}
}
