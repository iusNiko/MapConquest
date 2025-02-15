using Godot;
using System;
using System.Collections.Generic;

public class Building {
    public string BuildingType;
    public Dictionary<string, long> Cost = new();
    public Dictionary<string, long> Production = new();
    public Vector2I Position;
    public Color Color;

    public Building(string buildingType, Vector2I position) {
        BuildingType = buildingType;
        Position = position;
        foreach(string resource in ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[BuildingType]["Cost"]).Keys) {
            Cost[resource] = ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[BuildingType]["Cost"])[resource];
        }
        foreach(string resource in ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[BuildingType]["Production"]).Keys) {
            Production[resource] = ((Godot.Collections.Dictionary<string, long>)GameManager.Instance.BuildingData[BuildingType]["Production"])[resource];
        }
        Color = Color.Color8((byte)GameManager.Instance.BuildingData[BuildingType]["R"], (byte)GameManager.Instance.BuildingData[BuildingType]["G"], (byte)GameManager.Instance.BuildingData[BuildingType]["B"]);
    }
}