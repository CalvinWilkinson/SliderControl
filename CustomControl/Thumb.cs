using Avalonia;
using Avalonia.Media;

namespace CustomControl;

public struct Thumb
{
	public Thumb(Point position, Size size)
	{
		Bounds = new Rect(position, size);
	}

	public Rect Bounds { get; set; }

	public Color Color { get; private set; } = Colors.White;

	public Color HoverColor { get; set; } = Colors.Gray;

	public Color NonHoverColor { get; set; } = Colors.White;

	public bool Draggable { get; private set; }

	public Point MousePos { get; set; }

	public Point MouseDownPos { get; set; }

	public bool MouseIsOver { get; set; }

	public void Update()
	{
		if (Bounds.Contains(MousePos))
		{
			MouseIsOver = true;
			Color = HoverColor;

			if (Draggable)
			{
				var deltaX = MousePos.X - MouseDownPos.X;

				Bounds = new Rect(
					Bounds.X + deltaX,
					Bounds.Y,
					Bounds.Width,
					Bounds.Height);
			}

			MouseDownPos = MousePos;
		}
		else
		{
			MouseIsOver = false;
			Color = NonHoverColor;
		}
	}

	public void UpdateDragState(bool isMouseDown) => Draggable = MouseIsOver && isMouseDown;
}
