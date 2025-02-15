using Godot;
using System;

public partial class ShowHideUI : Button
{
	bool isHidden = false;
    public override void _Pressed()
    {
        if (isHidden)
		{
			foreach (Control child in GUI.Instance.GetChildren())
			{
				if(child == this) continue;
				child.Visible = true;
			}
			isHidden = false;
		}
		else
		{
			foreach (Control child in GUI.Instance.GetChildren())
			{
				if(child == this) continue;
				child.Visible = false;
			}
			isHidden = true;
		}
    }
}
