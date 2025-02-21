using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace CustomControl;

public static class ExtensionMethods
{
	public static TranslateTransform GetCtrlTransform(this Visual ctrl)
	{
		if (ctrl.RenderTransform is TranslateTransform transform)
		{
			return transform;
		}

		transform = new TranslateTransform();
		ctrl.RenderTransform = transform;

		return transform;
	}

	public static Rect SetX(this Rect rect, double value) => new(value, rect.Y, rect.Width, rect.Height);

	public static Color GetColor(this IBrush? brush)
	{
		if (brush is null)
		{
			return Colors.Transparent;
		}

		if (brush is IImmutableSolidColorBrush immutableSolidColorBrush)
		{
			return immutableSolidColorBrush.Color;
		}

		if (brush is SolidColorBrush solidBrush)
		{
			return solidBrush.Color;
		}

		return Colors.Transparent;
	}

	public static void DrawCircle(this DrawingContext ctx, Color fillClr, Color borderClr, Point position, double radius)
	{
		var fillBrush = new SolidColorBrush(fillClr);
		var borderBrush = new SolidColorBrush(borderClr);
		var borderPen = new Pen(borderBrush, 2);

		var center = new Point(position.X, position.Y);

		ctx.DrawEllipse(fillBrush, borderPen, center, radius, radius);
	}

	public static void DrawRectangle(
		this DrawingContext ctx,
		Point position,
		double width,
		double height,
		Color fillClr,
		Color borderClr,
		double cornerRadius = 0)
	{
		var fillBrush = new SolidColorBrush(fillClr);
		var borderBrush = new SolidColorBrush(borderClr);
		var borderPen = new Pen(borderBrush, 2);
		var rect = new Rect(position, new Size(width, height));

		ctx.DrawRectangle(fillBrush, borderPen, rect, cornerRadius, cornerRadius);
	}

	public static void DrawRectangle(this DrawingContext ctx, Rect rect, Color fillClr, Color borderClr, double cornerRadius = 0)
	{
		var fillBrush = new SolidColorBrush(fillClr);
		var borderBrush = new SolidColorBrush(borderClr);
		var borderPen = new Pen(borderBrush, 2);

		ctx.DrawRectangle(fillBrush, borderPen, rect, cornerRadius, cornerRadius);
	}
}
