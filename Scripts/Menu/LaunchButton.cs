using Godot;
using System;

public partial class LaunchButton : Button
{
	[Export] public PackedScene SceneToLoad;
	[Export] public ItemList MapList;
	public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton)
		{
			if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				GameManager.Instance.CurrentMap = MapList.GetItemText(MapList.GetSelectedItems()[0]);
				GetTree().ChangeSceneToPacked(SceneToLoad);
			}
		}
    }
}
