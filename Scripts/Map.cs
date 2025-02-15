using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


public partial class Map : Node2D
{
    public static Map Instance;
    public Vector2I MapSize;
    public Pixel[,] Pixels;
    public ConcurrentBag<Pixel> PixelsToRefresh = new();
    public Country[] Countries = Array.Empty<Country>(); //Countries[CountryID]
    Godot.Collections.Dictionary<Color, int> ColorToCountryID = new();
    Image TextureImage;
    Texture2D Texture;
    Thread thread;
    // Game Settings
    public long ColonizationCost = 25;
    public long NaturalResourceScale = 50000;
    public long BuildingResourceScale = 100;
    public int ArmyAttackScale = 5;
    public int ArmyDefenseScale = 5;
    public int ArmyInitiativeScale = 5;
    public int UpgradeCost = 1000;
    public ConcurrentDictionary<string, long> DesiredSavings = new();

    public int MaxTickrate = 20;
    public long Frame = 0;
    public long Tick = 0;

    public void LoadMap(string mapName) {
        string path = "res://Maps/" + mapName;
        DirAccess dir = DirAccess.Open(path);
        if(dir == null) {
            GameManager.Log($"Map {mapName} not found in core maps. Trying to load from user maps...");
            path = "user://Maps/" + mapName;
            dir = DirAccess.Open(path);
            if(dir == null) {
                GameManager.Log($"Error: Could not find {mapName} map! Exiting...");
                GetTree().Quit();
                return;
            }
        }

        // Load Map Config

        Json configJson = ResourceLoader.Load<Json>(path + "/MapConfig.json");
        if(configJson == null) {
            GameManager.Log($"Error: Could not load {mapName}/MapConfig.json! Exiting...");
            GetTree().Quit();
            return;
        }
        Godot.Collections.Dictionary<string, Variant> configData = (Godot.Collections.Dictionary<string, Variant>)configJson.Data;
        foreach(string key in  configData.Keys) {
            if(key == "ColonizationCost") ColonizationCost = (long)configData[key];
            if(key == "NaturalResourceScale") NaturalResourceScale = (long)configData[key];
            if(key == "BuildingResourceScale") BuildingResourceScale = (long)configData[key];
            if(key == "ArmyAttackScale") ArmyAttackScale = (int)configData[key];
            if(key == "ArmyDefenseScale") ArmyDefenseScale = (int)configData[key];
            if(key == "ArmyInitiativeScale") ArmyInitiativeScale = (int)configData[key];
            if(key == "UpgradeCost") UpgradeCost = (int)configData[key];
            if(key == "DesiredSavings") {
                Godot.Collections.Dictionary<string, Variant> savings = (Godot.Collections.Dictionary<string, Variant>)configData[key];
                foreach(string key2 in savings.Keys) {
                    DesiredSavings[key2] = (long)savings[key2];
                }
            }
        }
        
        
        // Load Terrain Data
        Image terrain = ResourceLoader.Load<CompressedTexture2D>(path + "/Terrain.png").GetImage();
        if(terrain == null) {
            GameManager.Log($"Error: Could not load {mapName}/Terrain.png! Exiting...");
            GetTree().Quit();
            return;
        }
        MapSize = new Vector2I(terrain.GetWidth(), terrain.GetHeight());
        GameManager.Log($"Map Size: {MapSize.X}x{MapSize.Y}");
        Pixels = new Pixel[MapSize.X, MapSize.Y];
        TextureImage = Image.Create(MapSize.X, MapSize.Y, false, Image.Format.Rgb8);
        int pixelCount = MapSize.X * MapSize.Y;
        int segment = 0;
        for(int x = 0; x < MapSize.X; x++) {
            for(int y = 0; y < MapSize.Y; y++) {
                Color color = terrain.GetPixel(x, y);
                string pixelType = GameManager.Instance.ColorToPixelType[color];
                Pixels[x, y] = new Pixel(new Vector2I(x, y), pixelType);
                segment++;
            }
        }
        
        // Load Countries Data
        DirAccess countryDir = DirAccess.Open(path + "/Countries");
        if(countryDir == null) {
            GameManager.Log($"Error: Could not open {mapName}/Countries directory! Exiting...");
            GetTree().Quit();
            return;
        }
        // Create neutral country and non-traversable wasteland
        Countries = new Country[] {
            new Country(0, "Neutral", "000", Color.Color8(255, 255, 255)),
        };
        ColorToCountryID[Color.Color8(255, 255, 255, 255)] = 0;
        ColorToCountryID[Color.Color8(0, 0, 0, 255)] = 0;

        foreach(string file in countryDir.GetFiles()) {
            Json json = ResourceLoader.Load<Json>(path + "/Countries/" + file);
            Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>> data = (Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>>)json.Data;
            foreach(string key in data.Keys) {
                Countries = Countries.Append(new Country(Countries.Length, key, (string)data[key]["Tag"], Color.Color8((byte)data[key]["R"], (byte)data[key]["G"], (byte)data[key]["B"]))).ToArray();
                ColorToCountryID[Color.Color8((byte)data[key]["PNG_R"], (byte)data[key]["PNG_G"], (byte)data[key]["PNG_B"], (byte)data[key]["PNG_A"])] = Countries.Length - 1;
                if(data[key].ContainsKey("Attack")) {
                    Countries[Countries.Length - 1].ArmyAttack = (int)data[key]["Attack"];
                }
                if(data[key].ContainsKey("Defense")) {
                    Countries[Countries.Length - 1].ArmyDefense = (int)data[key]["Defense"];
                }
                if(data[key].ContainsKey("Initiative")) {
                    Countries[Countries.Length - 1].ArmyInitiative = (int)data[key]["Initiative"];
                }
            }
        }
        foreach(Country country in Countries) {
            foreach(Country country2 in Countries) {
                Country.SetRelation(country, country2, 0);
            }
        }
        Image countries = ResourceLoader.Load<CompressedTexture2D>(path + "/Countries.png").GetImage();
        if(countries == null) {
            GameManager.Log($"Error: Could not load {mapName}/Countries.png! Exiting...");
            GetTree().Quit();
            return;
        }

        for(int x = 0; x < MapSize.X; x++) {
            for(int y = 0; y < MapSize.Y; y++) {
                Color color = countries.GetPixel(x, y);
                if(ColorToCountryID.ContainsKey(color)) {
                    Pixels[x, y].Owner = ColorToCountryID[color];
                }
            }
        }

        // Load Buildings Data
        Image buildings = ResourceLoader.Load<CompressedTexture2D>(path + "/Buildings.png").GetImage();
        if(buildings == null) {
            GameManager.Log($"Error: Could not load {mapName}/Buildings.png! Exiting...");
            GetTree().Quit();
            return;
        }
        for(int x = 0; x < MapSize.X; x++) {
            for(int y = 0; y < MapSize.Y; y++) {
                Color color = buildings.GetPixel(x, y);
                if(color == Color.Color8(0, 0, 0, 255) || color == Color.Color8(255, 255, 255, 255)) continue;
                if(GameManager.Instance.ColorToBuildingType.ContainsKey(color)) {
                    Pixels[x, y].Building = new Building(GameManager.Instance.ColorToBuildingType[color], new Vector2I(x, y));
                }
            }
        }

        // Refresh all pixels
        for(int x = 0; x < MapSize.X; x++) {
            for(int y = 0; y < MapSize.Y; y++) {
                Pixels[x, y].QueueToRefresh();
            }
        }
    }

    public override void _Draw()
    {
        Texture?.Dispose();
        Texture = ImageTexture.CreateFromImage(TextureImage);
        DrawTexture(Texture, new Vector2(0, 0));
    }

    public void RefreshPixels() {
        foreach(Pixel pixel in PixelsToRefresh) {
            TextureImage.SetPixel(pixel.Position.X, pixel.Position.Y, pixel.Color);
        }
        PixelsToRefresh.Clear();
    }

    public Pixel[] GetNeighbours(Pixel pixel) {
        List<Pixel> neighbours = new List<Pixel>();
        int x = pixel.Position.X, y = pixel.Position.Y;
        if(x > 0) neighbours.Add(Pixels[x - 1, y]);
        if(x < MapSize.X - 1) neighbours.Add(Pixels[x + 1, y]);
        if(y > 0) neighbours.Add(Pixels[x, y - 1]);
        if(y < MapSize.Y - 1) neighbours.Add(Pixels[x, y + 1]);
        return neighbours.ToArray();
    }

    public Pixel[] GetNeighboursIncludingDiagonal(Pixel pixel) {
        List<Pixel> neighbours = new List<Pixel>();
        int x = pixel.Position.X, y = pixel.Position.Y;
        if(x > 0) neighbours.Add(Pixels[x - 1, y]);
        if(x < MapSize.X - 1) neighbours.Add(Pixels[x + 1, y]);
        if(y > 0) neighbours.Add(Pixels[x, y - 1]);
        if(y < MapSize.Y - 1) neighbours.Add(Pixels[x, y + 1]);
        if(x > 0 && y > 0) neighbours.Add(Pixels[x - 1, y - 1]);
        if(x < MapSize.X - 1 && y > 0) neighbours.Add(Pixels[x + 1, y - 1]);
        if(x > 0 && y < MapSize.Y - 1) neighbours.Add(Pixels[x - 1, y + 1]);
        if(x < MapSize.X - 1 && y < MapSize.Y - 1) neighbours.Add(Pixels[x + 1, y + 1]);
        return neighbours.ToArray();
    }

    public long Update() {
        Stopwatch WholeUpdate = new Stopwatch();
        WholeUpdate.Start();
        for(int i = 2; i < Countries.Length; i++) {
            Countries[i].Update();
        }
        if(Tick % 30 == 0) {
            for(int i = 2; i < Countries.Length; i++) {
                Countries[i].MonthlyUpdate();
            }
            Parallel.For(0, MapSize.X, x => {
                for(int y = 0; y < MapSize.Y; y++) {
                    Pixels[x, y].Update();
                }
            });
        }
        if(Tick % 360 == 0) {
            for(int i = 2; i < Countries.Length; i++) {
                Countries[i].YearlyUpdate();
            }
        }
        WholeUpdate.Stop();
        GameManager.Log($"Whole Update: {WholeUpdate.ElapsedMilliseconds}ms");
        Tick++;
        return WholeUpdate.ElapsedMilliseconds;
    }

    public override void _Ready()
    {
        Instance = this;
        LoadMap(GameManager.Instance.CurrentMap);
        thread = new Thread(() => {
            while(true) {
                long updateTime = Update();
                if(PixelsToRefresh.Count  > 0) {
                    RefreshPixels();
                    CallDeferred(MethodName.QueueRedraw);
                }
                Thread.Sleep((int)Math.Max((1000 / MaxTickrate) - updateTime, 0));
            }
        });
        thread.Start();
    }

    public override void _Process(double delta)
    {
        Frame++;
        if(Input.IsActionJustPressed("increase_tickrate")) {
            MaxTickrate += 5;
        }
        if(Input.IsActionJustPressed("decrease_tickrate")) {
            if(MaxTickrate > 5)
                MaxTickrate -= 5;
        }
    }
}
