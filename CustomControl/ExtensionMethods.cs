using System;
using System.Globalization;
using Avalonia;
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

	/// <summary>
	/// Maps the given <paramref name="value"/> from one range to another.
	/// </summary>
	/// <param name="value">The value to map.</param>
	/// <param name="fromStart">The from starting range value.</param>
	/// <param name="fromStop">The from ending range value.</param>
	/// <param name="toStart">The to starting range value.</param>
	/// <param name="toStop">The to ending range value.</param>
	/// <returns>A value that has been mapped to a range between <paramref name="toStart"/> and <paramref name="toStop"/>.</returns>
	public static double MapValue(this double value, double fromStart, double fromStop, double toStart, double toStop)
		=> toStart + ((toStop - toStart) * ((value - fromStart) / (fromStop - fromStart)));

	/// <summary>
	/// Rounds the <see cref="double"/> value to the given number of <paramref name="decimalPlaces"/> and returns the result as a string.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <param name="decimalPlaces">The number of decimal places to round the value.</param>
	/// <returns>The string representation of the value.</returns>
	public static string ToStringRounded(this double value, uint decimalPlaces)
	{
		var roundedValue = Math.Round(value, (int)decimalPlaces).ToString(CultureInfo.InvariantCulture);

		if (roundedValue.Contains('.') && decimalPlaces > 0)
		{
			var decimalIndex = roundedValue.IndexOf('.');
			var decimalLength = roundedValue.Length - decimalIndex;

			if (decimalLength < decimalPlaces + 1)
			{
				roundedValue += new string('0', (int)decimalPlaces + 1 - decimalLength);
			}
		}
		else
		{
			if (decimalPlaces <= 0)
			{
				return roundedValue;
			}

			roundedValue += ".";
			roundedValue += new string('0', (int)decimalPlaces);
		}

		return roundedValue;
	}

	public static Color IncreaseBrightness(this Color value, double percentage)
	{
		var colorHSV = value.ToHsv();

		var newValue = colorHSV.V + (colorHSV.V * (percentage / 100));
		colorHSV = new HsvColor(colorHSV.A, colorHSV.H, colorHSV.S, newValue);

		return colorHSV.ToRgb();
	}

	public static Rect SetX(this Rect rect, double value) => new(value, rect.Y, rect.Width, rect.Height);

	public static Rect SetY(this Rect rect, double value) => new(rect.X, value, rect.Width, rect.Height);

	public static Rect SetWidth(this Rect rect, double value) => new(rect.X, rect.Y, value, rect.Height);

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
