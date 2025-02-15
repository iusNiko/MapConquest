using Godot;
using System;
using System.Linq;
public partial class GameManager : Node {
    public Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>> PixelData = new();
    public Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>> BuildingData = new();
    public Godot.Collections.Dictionary<Color, string> ColorToPixelType = new();
    public Godot.Collections.Dictionary<Color, string> ColorToBuildingType = new();
    public string[] Resources = Array.Empty<string>();
    public string CurrentMap = "";
    public Country SelectedCountry = null;
    public string SelectedTerrain = "";
    public string SelectedBuilding = "";
    public static GameManager Instance;
    public RandomNumberGenerator RNG = new RandomNumberGenerator();

    public void LoadGameData() {
        // Load Resource Data
        DirAccess dir = DirAccess.Open("res://Data/Resources");
        if(dir == null) {
            Log("Error: Could not open \"res://Data/Resources\" directory! Exiting...");
            GetTree().Quit();
            return;
        }
        foreach(string file in dir.GetFiles()) {
            Json json = ResourceLoader.Load<Json>("res://Data/Resources/" + file);
            string[] data = (string[])json.Data;
            foreach(string resource in data) {
                Resources = Resources.Append(resource).ToArray();
            }
        }

        // Load Pixel Data
        dir = DirAccess.Open("res://Data/Pixels");
        if(dir == null) {
            Log("Error: Could not open \"res://Data/Pixels\" directory! Exiting...");
            GetTree().Quit();
            return;
        }
        foreach(string file in dir.GetFiles()) {
            Json json = ResourceLoader.Load<Json>("res://Data/Pixels/" + file);
            Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>> data = (Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>>)json.Data;
            foreach(string key in data.Keys) {
                PixelData[key] = data[key];
                ColorToPixelType[Color.Color8((byte)data[key]["PNG_R"], (byte)data[key]["PNG_G"], (byte)data[key]["PNG_B"], (byte)data[key]["PNG_A"])] = key;
            }
        }

        // Load Building Data
        dir = DirAccess.Open("res://Data/Buildings");
        if(dir == null) {
            Log("Error: Could not open \"res://Data/Buildings\" directory! Exiting...");
            GetTree().Quit();
            return;
        }
        foreach(string file in dir.GetFiles()) {
            Json json = ResourceLoader.Load<Json>("res://Data/Buildings/" + file);
            Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>> data = (Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>>)json.Data;
            foreach(string key in data.Keys) {
                BuildingData[key] = data[key];
                ColorToBuildingType[Color.Color8((byte)data[key]["PNG_R"], (byte)data[key]["PNG_G"], (byte)data[key]["PNG_B"], (byte)data[key]["PNG_A"])] = key;
            }
        }
    }
    public static void Log(string message) {
        GD.Print("[INFO] " + message);
    }
    public override void _Ready() {
        Instance = this;
        RNG.Randomize();
        LoadGameData();
    }
    public override void _Process(double delta) {
        if(Input.IsActionJustPressed("left_click") && Map.Instance != null) {
            Vector2I mousePos = (Vector2I)Map.Instance.GetGlobalMousePosition();
            if(mousePos.X < 0 || mousePos.Y < 0 || mousePos.X >= Map.Instance.MapSize.X || mousePos.Y >= Map.Instance.MapSize.Y || GUI.Instance.IsMouseOverGUI) {
                return;
            }
            
            SelectedCountry = Map.Instance.Countries[Map.Instance.Pixels[mousePos.X, mousePos.Y].Owner];
            GUI.Instance.CountryList.Select(SelectedCountry.Id);
        }
    }
}