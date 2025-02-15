using Godot;
using System;

public partial class MenuButton : Button
{
	[Export] public PackedScene SceneToLoad;

    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton)
		{
			if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				GetTree().ChangeSceneToPacked(SceneToLoad);
			}
		}
    }
}
