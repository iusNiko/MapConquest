using Godot;
using System;

public partial class MapList : ItemList
{
	TextureRect MapImage;
	public override void _Ready()
	{
		DirAccess dir = DirAccess.Open("res://Maps");
		if (dir == null)
		{
			GD.PrintErr("Error: Could not open \"res://Maps\" directory! Exiting...");
			GetTree().Quit();
			return;
		}
		foreach (string file in dir.GetDirectories())
		{
			AddItem(file);
		}
		ItemSelected += OnItemSelected;
	}

	public void OnItemSelected(long index)
	{
		string mapName = GetItemText((int)index);
		MapImage?.QueueFree();
		MapImage = new TextureRect();
		MapImage.Texture = ResourceLoader.Load<Texture2D>($"res://Maps/{mapName}/Countries.png");
		MapImage.Size = MapImage.Texture.GetSize();
		GetNode<Node>("../").AddChild(MapImage);
		MapImage.Position = new Vector2(450, 75);
	}

	public override void _Process(double delta)
	{
	}
}
