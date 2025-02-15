using Godot;
using System;

public partial class CountryLabel : RichTextLabel
{
	public override void _Process(double delta)
	{
		Text = "";
		int x = (int)Map.Instance.GetGlobalMousePosition().X, y = (int)Map.Instance.GetGlobalMousePosition().Y;
		if(x < 0 || y < 0 || x >= Map.Instance.MapSize.X || y >= Map.Instance.MapSize.Y)
		{
			return;
		}
		
		Text += $"{Map.Instance.Countries[Map.Instance.Pixels[x, y].Owner].Name}";
		foreach(string resource in Map.Instance.Countries[Map.Instance.Pixels[x, y].Owner].Resources.Keys)
		{
			Text += $"\n{resource}: {Map.Instance.Countries[Map.Instance.Pixels[x, y].Owner].Resources[resource]}";
		}
		Text += $"\n+{Map.Instance.Countries[Map.Instance.Pixels[x, y].Owner].AttackUpgrades} Attack";
		Text += $"\n+{Map.Instance.Countries[Map.Instance.Pixels[x, y].Owner].DefenseUpgrades} Defense";
		Text += $"\n+{Map.Instance.Countries[Map.Instance.Pixels[x, y].Owner].InitiativeUpgrades} Initiative";
	}
}
