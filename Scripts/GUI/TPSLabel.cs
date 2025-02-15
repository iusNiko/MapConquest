using Godot;
using System;

public partial class TPSLabel : Label
{
	double _time = 0;
	long LastTicks = 0;
	long tps = 0;
	public override void _Process(double delta)
	{
		Text = $"Max TPS: {Map.Instance.MaxTickrate}, TPS: {tps}";
		_time += delta;
		if(_time >= 1) {
			tps = Map.Instance.Tick - LastTicks;
			_time = 0;
			LastTicks = Map.Instance.Tick;
		}
	}
}
