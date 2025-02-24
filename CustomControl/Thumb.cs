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

	public double Left => Bounds.Left;

	public double Width => Bounds.Width;

	public double Height => Bounds.Height;

	public double HalfWidth => Bounds.Width / 2;

	public double Top => Bounds.Top;

	public Color Color { get; set; } = Colors.White;

	public bool Draggable { get; set; }

	public Point MouseDownPos { get; set; }

	public bool MouseIsOver { get; set; }

	public void Update(Point mousePos)
	{
		if (Bounds.Contains(mousePos))
		{
			MouseIsOver = true;

			if (Draggable)
			{
				var deltaX = mousePos.X - MouseDownPos.X;

				Bounds = new Rect(
					Bounds.X + deltaX,
					Bounds.Y,
					Bounds.Width,
					Bounds.Height);
			}

			MouseDownPos = mousePos;
		}
		else
		{
			MouseIsOver = false;
		}
	}
}
