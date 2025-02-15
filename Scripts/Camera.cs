using Godot;
using System;

public partial class Camera : Camera2D
{
	[Export] public float PanDistance = 20f;
	[Export] public float PanSpeed = 400f;
    [Export] public float MinZoom = 1f;
    [Export] public float MaxZoom = 12f;
    [Export] float _zoomStep = 0.25f;
    public float ZoomStep {
        get {
            return _zoomStep * Zoom.X;
        }
        set {
            _zoomStep = value;
        }
    }

    public override void _Process(double delta)
    {
		Vector2 direction = Vector2.Zero;

		if(GetViewport().GetMousePosition().X > GetViewportRect().Size.X - PanDistance) {
			direction.X = 1;
		}
		if(GetViewport().GetMousePosition().X < 0 + PanDistance) {
			direction.X = -1;
		}
		if(GetViewport().GetMousePosition().Y > GetViewportRect().Size.Y - PanDistance) {
			direction.Y = 1;
		}
		if(GetViewport().GetMousePosition().Y < 0 + PanDistance) {
			direction.Y = -1;
		}

		Position += direction * PanSpeed * (float) delta * 1/Zoom.X;

		if(Input.IsActionJustPressed("home")) {
			GlobalPosition = Vector2.Zero;
		}

        Vector2 zoom = Zoom;

        if(Input.IsActionJustPressed("scroll_up")) {
            zoom.X += ZoomStep;
            zoom.Y += ZoomStep;
        }
        if(Input.IsActionJustPressed("scroll_down")) {
            zoom.X -= ZoomStep;
            zoom.Y -= ZoomStep;
        }

        Zoom = new Vector2(
            Mathf.Clamp(zoom.X, MinZoom, MaxZoom),
            Mathf.Clamp(zoom.Y, MinZoom, MaxZoom)
        );
    }
	
}
