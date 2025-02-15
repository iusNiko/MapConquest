using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ActionList : ItemList
{
	public int SelectedItem = -1;
	[Export] public LineEdit BrushSize;
	[Export] public CheckBox EditWasteland;
	public void OnItemSelected(long idx) {
		SelectedItem = (int)idx;
	}
    public override void _Ready()
    {
		ItemSelected += OnItemSelected;
    }
    public override void _Process(double delta)
	{
		if(!GetRect().HasPoint(GetViewport().GetMousePosition()) && SelectedItem != -1 && Input.IsActionPressed("right_click")) {
			Vector2I mousePos = (Vector2I)Map.Instance.GetGlobalMousePosition();
			Object obj = mousePos;
			GetType().GetMethod(GetItemText(SelectedItem).Replace(" ", "_").ToLower()).Invoke(this, new[] {obj});
		}
	}

	public void attack(Object mouse_pos) {
		int country1 = GameManager.Instance.SelectedCountry.Id;
		int country2 = Map.Instance.Pixels[((Vector2I)mouse_pos).X, ((Vector2I)mouse_pos).Y].Owner;
		if(country1 != country2 && country1 > 0 && country2 > 0) {
			Country.SetRelation(Map.Instance.Countries[country1], Map.Instance.Countries[country2], 2);
		}
	}
	public void force_peace(Object mouse_pos) {
		int country1 = GameManager.Instance.SelectedCountry.Id;
		int country2 = Map.Instance.Pixels[((Vector2I)mouse_pos).X, ((Vector2I)mouse_pos).Y].Owner;
		if(country1 != country2 && country1 > 0 && country2 > 0) {
			Country.SetRelation(Map.Instance.Countries[country1], Map.Instance.Countries[country2], 0);
		}
	}
	public void paint_country(Object mouse_pos) {
		int country = GameManager.Instance.SelectedCountry.Id;
		bool editWasteland = EditWasteland.ButtonPressed;
		List<Pixel> pixelsToPaint = new();
		int size = 0;
		try {
			size = int.Parse(BrushSize.Text);
		} catch {}
		
		for(int x = -size; x <= size; x++) {
			for(int y = -size; y <= size; y++) {
				if(Mathf.Sqrt(x * x + y * y) <= size) {
					if(((Vector2I)mouse_pos).X + x >= 0 && ((Vector2I)mouse_pos).X + x < Map.Instance.Pixels.GetLength(0) && ((Vector2I)mouse_pos).Y + y >= 0 && ((Vector2I)mouse_pos).Y + y < Map.Instance.Pixels.GetLength(1)) {
						pixelsToPaint.Add(Map.Instance.Pixels[((Vector2I)mouse_pos).X + x, ((Vector2I)mouse_pos).Y + y]);
					}
				}
			}
		}

		pixelsToPaint = pixelsToPaint.Where(p => p.PixelType != "Wasteland" || editWasteland).ToList();

		for(int i = 0; i < pixelsToPaint.Count; i++) {
			pixelsToPaint[i].Owner = country;
			pixelsToPaint[i].QueueToRefresh();
		}

	}
	public void paint_terrain(Object mouse_pos) {
		string terrain = GameManager.Instance.SelectedTerrain;
		bool editWasteland = EditWasteland.ButtonPressed;
		List<Pixel> pixelsToPaint = new();
		int size = 0;
		try {
			size = int.Parse(BrushSize.Text);
		} catch {}
		
		for(int x = -size; x <= size; x++) {
			for(int y = -size; y <= size; y++) {
				if(Mathf.Sqrt(x * x + y * y) <= size) {
					if(((Vector2I)mouse_pos).X + x >= 0 && ((Vector2I)mouse_pos).X + x < Map.Instance.Pixels.GetLength(0) && ((Vector2I)mouse_pos).Y + y >= 0 && ((Vector2I)mouse_pos).Y + y < Map.Instance.Pixels.GetLength(1)) {
						pixelsToPaint.Add(Map.Instance.Pixels[((Vector2I)mouse_pos).X + x, ((Vector2I)mouse_pos).Y + y]);
					}
				}
			}
		}

		pixelsToPaint = pixelsToPaint.Where(p => p.PixelType != "Wasteland" || editWasteland).ToList();
		if(terrain != "Wasteland" || editWasteland) {
			for(int i = 0; i < pixelsToPaint.Count; i++) {
				if(terrain == "Wasteland") {
					pixelsToPaint[i].Owner = 0;
				}
				pixelsToPaint[i].SetNewType(terrain);
			}
		}
	}
	public void paint_buildings(Object mouse_pos) {
		string building = GameManager.Instance.SelectedBuilding;
		bool editWasteland = EditWasteland.ButtonPressed;
		List<Pixel> pixelsToPaint = new();
		int size = 0;
		try {
			size = int.Parse(BrushSize.Text);
		} catch {}
		
		for(int x = -size; x <= size; x++) {
			for(int y = -size; y <= size; y++) {
				if(Mathf.Sqrt(x * x + y * y) <= size) {
					if(((Vector2I)mouse_pos).X + x >= 0 && ((Vector2I)mouse_pos).X + x < Map.Instance.Pixels.GetLength(0) && ((Vector2I)mouse_pos).Y + y >= 0 && ((Vector2I)mouse_pos).Y + y < Map.Instance.Pixels.GetLength(1)) {
						pixelsToPaint.Add(Map.Instance.Pixels[((Vector2I)mouse_pos).X + x, ((Vector2I)mouse_pos).Y + y]);
					}
				}
			}
		}

		pixelsToPaint = pixelsToPaint.Where(p => p.PixelType != "Wasteland" || editWasteland).ToList();

		for(int i = 0; i < pixelsToPaint.Count; i++) {
			pixelsToPaint[i].Building = new Building(building, new Vector2I(pixelsToPaint[i].Position.X, pixelsToPaint[i].Position.Y));
			pixelsToPaint[i].QueueToRefresh();
		}
	}
}
