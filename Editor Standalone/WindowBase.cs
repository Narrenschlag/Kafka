using Cutulu;
using Godot;

namespace Kafka.Editor
{
    public partial class WindowBase : Window
    {
        [Export] private bool clampToParentWindow = true;
        [Export] private int PixelMargin;

        private Vector2 lastPosition;
        public Node Parent;

        public virtual int topMarginForTitle() => 35;

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Connect("close_requested", this, "close");
        }

        public virtual void Setup(Node parent)
        {
            this.Parent = parent;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (clampToParentWindow && !lastPosition.Equals(Position))
            {
                if (this.ClampToViewport(Parent, topMarginForTitle(), PixelMargin)) lastPosition = Position;
            }
        }

        public virtual void close()
        {
            this.Destroy();
        }
    }
}