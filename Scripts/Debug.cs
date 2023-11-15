using Godot;

namespace Kafka
{
	public partial class Debug : Node
	{
		[Export] private Narrator Narrator;
		[Export] private string GlobalPath = "test/testA:test1";
		[Export] private bool Active = false;

		public override void _Ready()
		{
			if (!Active) return;

			Narrator.load(GlobalPath, this);
		}
	}
}
