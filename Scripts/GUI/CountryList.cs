using Godot;
using System;

public partial class CountryList : ItemList
{
    public override void _Ready()
    {
        ItemSelected += OnItemSelected;
    }
    public override void _Process(double delta)
	{
		if(ItemCount != Map.Instance.Countries.Length)
		{
			Clear();
			foreach(Country country in Map.Instance.Countries)
			{
				AddItem(country.Name);
			}
		}
	}
	public void OnItemSelected(long idx)
	{
		GameManager.Instance.SelectedCountry = Map.Instance.Countries[(int)idx];
	}
}
