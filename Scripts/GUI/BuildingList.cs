using Godot;
using System;

public partial class BuildingList : ItemList
{
	public override void _Ready()
    {
        ItemSelected += OnItemSelected;
		foreach(string building in GameManager.Instance.BuildingData.Keys) {
			AddItem(building);
		}
    }

	public void OnItemSelected(long idx)
	{
		GameManager.Instance.SelectedBuilding = GetItemText((int)idx);
	}
}
