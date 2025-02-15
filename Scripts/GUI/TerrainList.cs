using Godot;
using System;

public partial class TerrainList : ItemList
{
	public override void _Ready()
    {
        ItemSelected += OnItemSelected;
		foreach(string terrain in GameManager.Instance.PixelData.Keys) {
			AddItem(terrain);
		}
    }

	public void OnItemSelected(long idx)
	{
		GameManager.Instance.SelectedTerrain = GetItemText((int)idx);
	}
}
