using Godot;
using System;

public partial class ExitButton : TextureButton
{
	public override void _Ready()
	{
		this.ButtonDown += this.ExitThisApp;
	}
	public void ExitThisApp()
	{
		GetTree().Quit();
	}
}
