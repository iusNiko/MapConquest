using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

public partial class Pixel {
    //Appearance
    public Color Color;
    //Data
    public Dictionary<string, long> Production = new();
    public Building Building = null;
    //Game Logic
    public Vector2I Position;
    public string PixelType;
    public int Owner = 0; // 0 -> No owner, 1 -> Wasteland
    public Pixel(Vector2I position, string pixelType = "Plains",  int owner = 0) {
        Position = position;
        PixelType = pixelType;
        Owner = owner;
        foreach(string resource in ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.PixelData[PixelType]["Production"]).Keys) {
            Production[resource] = ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.PixelData[PixelType]["Production"])[resource];
        }
    }

    public void SetNewType(string type) {
        PixelType = type;
        Production.Clear();
        foreach(string resource in ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.PixelData[PixelType]["Production"]).Keys) {
            Production[resource] = ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.PixelData[PixelType]["Production"])[resource];
        }
        if(type != "Plains") {
            Building = null;
        }
        Color = Color.Color8((byte)GameManager.Instance.PixelData[PixelType]["R"], (byte)GameManager.Instance.PixelData[PixelType]["G"], (byte)GameManager.Instance.PixelData[PixelType]["B"]);
        QueueToRefresh();
    }

    public void Update() {
        if(Owner > 1) {
            foreach(string resource in Production.Keys) {
                Map.Instance.Countries[Owner].Production[resource] += Production[resource];
            }
            if(Building != null) {
                foreach(string resource in Building.Production.Keys) {
                    Map.Instance.Countries[Owner].BuildingProduction[resource] += Building.Production[resource];
                }
            }
        }
    }

    public void QueueToRefresh() {
        Color baseColor = Color.Color8((byte)GameManager.Instance.PixelData[PixelType]["R"], (byte)GameManager.Instance.PixelData[PixelType]["G"], (byte)GameManager.Instance.PixelData[PixelType]["B"]);
        if(Building != null) {
            baseColor = Building.Color;
        }
        Color countryColor = new Color(1, 1, 1, 0.2f);
        if(Owner > 0) {
            countryColor = Map.Instance.Countries[Owner].Color;
            countryColor.A = 0.2f;
        }
        Color = baseColor * countryColor;
        Map.Instance.PixelsToRefresh.Add(this);
    }
}