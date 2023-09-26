using Godot;
using System;

public partial class GameManager : Node
{
	Node3D m_SelectedPawn;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		try
		{
			m_SelectedPawn = GetNode<Node3D>("pawn");
    }
		catch 
		{
			GD.PrintErr("Selected pawn not found!");
		}
  }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//public override void _Process(double delta)
	//{
	//}

	public void DoUpdate(InputActions inputs)
	{
		if (inputs.click)
			MoveSelectedPawn(inputs.cursorPosition);
	}

	private void MoveSelectedPawn(Vector3 point)
	{
		if (m_SelectedPawn == null)
			return;

		m_SelectedPawn.GlobalPosition = point;
	}
}
