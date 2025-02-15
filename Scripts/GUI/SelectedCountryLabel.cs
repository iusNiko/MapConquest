using Godot;
using System;

public partial class SelectedCountryLabel : Label
{
	public override void _Process(double delta)
	{
		Text = "Selected Country: ";
		if(GameManager.Instance.SelectedCountry == null)
		{
			Text += "None";
		}
		else {
			Text += GameManager.Instance.SelectedCountry.Name;
		}

		Text += "\nSelected Terrain: ";

		if(GameManager.Instance.SelectedTerrain == "")
		{
			Text += "None";
		}
		else
		{
			Text += GameManager.Instance.SelectedTerrain;
		}
	}
}
