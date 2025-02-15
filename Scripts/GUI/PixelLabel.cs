using Godot;
using System;

public partial class PixelLabel : RichTextLabel
{
	public override void _Process(double delta)
	{
		Text = "";
		int x = (int)Map.Instance.GetGlobalMousePosition().X, y = (int)Map.Instance.GetGlobalMousePosition().Y;
		if(x < 0 || y < 0 || x >= Map.Instance.MapSize.X || y >= Map.Instance.MapSize.Y)
		{
			return;
		}
		Text += $"{Map.Instance.Pixels[x, y].PixelType}\n{Map.Instance.Pixels[x, y].Building?.BuildingType}\n";
	}
}
