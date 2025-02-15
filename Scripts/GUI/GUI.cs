using Godot;
using System;

public partial class GUI : CanvasLayer
{
	public static GUI Instance;
	public ActionList ActionList;
	public LineEdit BrushSize;
	public Panel EditWasteland;
	public CountryList CountryList;
	public TerrainList TerrainList;
	public BuildingList BuildingList;
	public bool IsMouseOverGUI {
		get {
			return (ActionList.GetRect().HasPoint(GetViewport().GetMousePosition()) && ActionList.Visible) || 
			(EditWasteland.GetRect().HasPoint(GetViewport().GetMousePosition()) && EditWasteland.Visible) || 
			(BrushSize.GetRect().HasPoint(GetViewport().GetMousePosition()) && BrushSize.Visible) || 
			(CountryList.GetRect().HasPoint(GetViewport().GetMousePosition()) && CountryList.Visible) ||
			(TerrainList.GetRect().HasPoint(GetViewport().GetMousePosition()) && TerrainList.Visible) ||
			(BuildingList.GetRect().HasPoint(GetViewport().GetMousePosition()) && BuildingList.Visible);
		}
	}
	public override void _Ready()
	{
		Instance = this;
		ActionList = GetNode<ActionList>("ActionList");
		BrushSize = GetNode<LineEdit>("BrushSize");
		EditWasteland = GetNode<Panel>("EditWastelandPanel");
		CountryList = GetNode<CountryList>("CountryList");
		TerrainList = GetNode<TerrainList>("TerrainList");
		BuildingList = GetNode<BuildingList>("BuildingList");
	}

	public override void _Process(double delta)
	{
		if(ActionList.SelectedItem == 0) {
			BrushSize.Visible = false;
			EditWasteland.Visible = false;
			CountryList.Visible = false;
			TerrainList.Visible = false;
			BuildingList.Visible = false;
		}
		else if(ActionList.SelectedItem == 1) {
			BrushSize.Visible = false;
			EditWasteland.Visible = false;
			CountryList.Visible = false;
			TerrainList.Visible = false;
			BuildingList.Visible = false;
		}
		else if(ActionList.SelectedItem == 2) {
			BrushSize.Visible = true;
			EditWasteland.Visible = true;
			CountryList.Visible = true;
			TerrainList.Visible = false;
			BuildingList.Visible = false;
		}
		else if(ActionList.SelectedItem == 3) {
			BrushSize.Visible = true;
			EditWasteland.Visible = true;
			CountryList.Visible = false;
			TerrainList.Visible = true;
			BuildingList.Visible = false;
		}
		else if(ActionList.SelectedItem == 4) {
			BrushSize.Visible = true;
			EditWasteland.Visible = true;
			CountryList.Visible = false;
			TerrainList.Visible = false;
			BuildingList.Visible = true;
		}
		else {
			BrushSize.Visible = false;
			EditWasteland.Visible = false;
			CountryList.Visible = false;
			TerrainList.Visible = false;
			BuildingList.Visible = false;
		}
	}
}
