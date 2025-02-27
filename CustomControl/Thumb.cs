using System;
using Avalonia;
using Avalonia.Dialogs;
using Avalonia.Media;

namespace CustomControl;

public struct Thumb
{
	private double dragOffsetX;

	public Thumb(Point position, Size size)
	{
		Bounds = new Rect(position, size);
	}

	public Rect Bounds { get; set; }

	public double Left => Bounds.Left;

	public double Right => Bounds.Right;

	public double Width => Bounds.Width;

	public double Height => Bounds.Height;

	public double HalfWidth => Bounds.Width / 2;

	public double CenterX => Bounds.Center.X;

	public double Top => Bounds.Top;

	public Color Color { get; set; } = Colors.White;

	public bool Draggable { get; set; }

	public bool MouseIsOver { get; set; }

	public void SetLeft(double value) => Bounds = Bounds.SetX(value);

	public void SetRight(double value) => Bounds = Bounds.SetX(value - Width);

	public void SetWidth(double value) => Bounds = Bounds.SetWidth(value);

	public void SetCenterX(double value) => Bounds = Bounds.SetX(value - HalfWidth);

	public void UpdateNew(Point mousePos, bool isMouseDown)
	{
		if (Bounds.Contains(mousePos))
		{
			MouseIsOver = true;

			if (!isMouseDown)
			{
				Draggable = false;

				return;
			}

			if (!Draggable)
			{
				Draggable = true;
				dragOffsetX = mousePos.X - Bounds.X;
			}

			Console.WriteLine("DRAGGING THE THUMB");
			SetLeft(mousePos.X - dragOffsetX);
		}
		else
		{
			MouseIsOver = false;

			if (!isMouseDown)
			{
				Draggable = false;
			}
		}
	}
}
