using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace CustomControl;

public class Slider : Control
{
	private Point mousePos;
	private Thumb minThumb;
	private Thumb maxThumb;
	private FormattedText minValueText;
	private FormattedText maxValueText;

	public static readonly StyledProperty<IBrush?> BackgroundProperty =
		AvaloniaProperty.Register<Slider, IBrush?>(nameof(Background), defaultValue: Brushes.Transparent);

	public static readonly StyledProperty<double> MinValueProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(MinValue), defaultValue: 0);

	public static readonly StyledProperty<double> MaxValueProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(MaxValue), defaultValue: 100);

	public static readonly StyledProperty<double> RangeMinValueProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(RangeMinValue), defaultValue: 0.0);

	public static readonly StyledProperty<double> RangeMaxValueProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(RangeMaxValue), defaultValue: 100);

	public static readonly StyledProperty<double> ThumbRadiusProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(ThumbRadius), defaultValue: 3);

	public static readonly StyledProperty<Color> MinThumbColorProperty =
		AvaloniaProperty.Register<Slider, Color>(
			nameof(MinThumbColor),
			defaultValue: Colors.CornflowerBlue,
			coerce: (obj, newClr) =>
			{
				if (obj is not Slider slider)
				{
					throw new Exception("The object is not a Slider.");
				}

				slider.minThumb.NonHoverColor = newClr;
				slider.minThumb.HoverColor = newClr.IncreaseBrightness(25);

				return newClr;
			});

	public static readonly StyledProperty<Color> MaxThumbColorProperty =
		AvaloniaProperty.Register<Slider, Color>(
			nameof(MaxThumbColor),
			defaultValue: Colors.CornflowerBlue,
			coerce: (obj, newClr) =>
			{
				if (obj is not Slider slider)
				{
					throw new Exception("The object is not a Slider.");
				}

				slider.maxThumb.NonHoverColor = newClr;
				slider.maxThumb.HoverColor = newClr.IncreaseBrightness(25);

				return newClr;
			});

	public Slider()
	{
		Width = 200;
		Height = 200;
	}

	public IBrush? Background
	{
		get => GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	public double MinValue
	{
		get => GetValue(MinValueProperty);
		set => SetValue(MinValueProperty, value);
	}

	public double MaxValue
	{
		get => GetValue(MaxValueProperty);
		set => SetValue(MaxValueProperty, value);
	}

	public double RangeMinValue
	{
		get => GetValue(RangeMinValueProperty);
		set => SetValue(RangeMinValueProperty, value);
	}

	public double RangeMaxValue
	{
		get => GetValue(RangeMaxValueProperty);
		set => SetValue(RangeMaxValueProperty, value);
	}

	public double ThumbRadius
	{
		get => GetValue(ThumbRadiusProperty);
		set => SetValue(ThumbRadiusProperty, value);
	}

	public Color MinThumbColor
	{
		get => GetValue(MinThumbColorProperty);
		set => SetValue(MinThumbColorProperty, value);
	}

	public Color MaxThumbColor
	{
		get => GetValue(MaxThumbColorProperty);
		set => SetValue(MaxThumbColorProperty, value);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		var thumbWidth = 65;
		var thumbHeight = 35;
		var thumbHalfHeight = thumbHeight / 2;
		var halfHeight = Bounds.Height / 2;

		this.minThumb = new Thumb(new Point(0, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.minThumb.NonHoverColor = MinThumbColor;
		this.minThumb.HoverColor = MinThumbColor.IncreaseBrightness(25);

		this.maxThumb = new Thumb(new Point(Bounds.Width - thumbWidth, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.maxThumb.NonHoverColor = MaxThumbColor;
		this.maxThumb.HoverColor = MaxThumbColor.IncreaseBrightness(25);

		this.minThumb.Update();
		this.maxThumb.Update();

		InvalidateVisual();

		base.OnLoaded(e);
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		this.minThumb.UpdateDragState(true);
		this.maxThumb.UpdateDragState(true);

		this.mousePos = e.GetCurrentPoint(null).Position;

		this.minThumb.MouseDownPos = this.mousePos;
		this.maxThumb.MouseDownPos = this.mousePos;

		base.OnPointerPressed(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		this.minThumb.UpdateDragState(false);
		this.maxThumb.UpdateDragState(false);

		base.OnPointerReleased(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		this.mousePos = e.GetCurrentPoint(null).Position;
		this.minThumb.MousePos = this.mousePos;
		this.maxThumb.MousePos = this.mousePos;

		this.minThumb.Update();
		this.maxThumb.Update();

		ProcessCollisions();
		InvalidateVisual();

		base.OnPointerMoved(e);
	}

	public override void Render(DrawingContext ctx)
	{
		this.minValueText = new FormattedText(
			Math.Round(MinValue, 2).ToString(CultureInfo.InvariantCulture),
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			Typeface.Default,
			20,
			Brushes.Black);

		this.maxValueText = new FormattedText(
			Math.Round(MaxValue, 2).ToString(CultureInfo.InvariantCulture),
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			Typeface.Default,
			20,
			Brushes.Black);

		var minTextWidth = minValueText.Width;
		var maxTextWidth = maxValueText.Width;

		if (minTextWidth > this.minThumb.Bounds.Width)
		{
			this.minThumb.Bounds = this.minThumb.Bounds.SetWidth(minTextWidth + (minTextWidth * 0.2));
		}

		if (maxTextWidth > this.maxThumb.Bounds.Width)
		{
			this.maxThumb.Bounds = this.maxThumb.Bounds.SetWidth(maxTextWidth + (maxTextWidth * 0.2));
		}

		RenderBackground(ctx);
		RenderSliderLine(ctx);
		RenderMinThumb(ctx);
		RenderMaxThumb(ctx);
		RenderMinValueText(ctx);
		RenderMaxValueText(ctx);
	}

	private void ProcessCollisions()
	{
		var minBounds = this.minThumb.Bounds;
		var maxBounds = this.maxThumb.Bounds;

		var isOverlapping = minBounds.Intersects(maxBounds);

		if (isOverlapping)
		{
			var overlapAmount = minBounds.Intersect(maxBounds).Width;
			var overlapHalf = overlapAmount / 2;

			if (this.minThumb.Draggable)
			{
				maxBounds = maxBounds.SetX(minBounds.Right + overlapHalf);
			}
			else if (this.maxThumb.Draggable)
			{
				minBounds = minBounds.SetX((maxBounds.Left - minBounds.Width) - overlapHalf);
			}
		}

		// If the min thumb is past the left edge of the slider
		if (minBounds.Left < 0)
		{
			minBounds = minBounds.SetX(0);
		}

		// If the max thumb is past the right edge of the slider
		if (maxBounds.Right > Bounds.Width)
		{
			maxBounds = maxBounds.SetX(Bounds.Width - maxBounds.Width);
		}

		var noSpaceToLeftOfMaxThumb = maxBounds.Left < minBounds.Right;
		var noSpaceToRightOfMaxThumb = maxBounds.Right >= Bounds.Width;

		// If the right side of the min thumb is past the left side of the max thumb
		if (noSpaceToLeftOfMaxThumb && noSpaceToRightOfMaxThumb)
		{
			minBounds = minBounds.SetX(maxBounds.Left - minBounds.Width);
		}

		var noSpaceToLeftOfMinThumb = minBounds.Left <= 0;
		var noSpaceToRightOfMinThumb = minBounds.Right >= maxBounds.Left;

		if (noSpaceToLeftOfMinThumb && noSpaceToRightOfMinThumb)
		{
			minBounds = minBounds.SetX(0);
			maxBounds = maxBounds.SetX(minBounds.Right);
		}

		this.minThumb.Bounds = minBounds;
		this.maxThumb.Bounds = maxBounds;
	}

	private void RenderBackground(DrawingContext ctx)
	{
		ctx.DrawRectangle(new Point(0, 0), Width, Height, Background.GetColor(), Colors.Transparent);
	}

	private void RenderSliderLine(DrawingContext ctx)
	{
		var pen = new Pen(Brushes.White, 2);

		var halfHeight = Bounds.Height / 2;
		ctx.DrawLine(pen, new Point(0, halfHeight), new Point(Width, halfHeight));
	}

	private void RenderMinThumb(DrawingContext ctx)
	{
		ctx.DrawRectangle(this.minThumb.Bounds, this.minThumb.Color, Colors.Transparent, ThumbRadius);
	}

	private void RenderMaxThumb(DrawingContext ctx)
	{
		ctx.DrawRectangle(this.maxThumb.Bounds, this.maxThumb.Color, Colors.Transparent, ThumbRadius);
	}

	private void RenderMinValueText(DrawingContext ctx)
	{
		var thumbCenterX = this.minThumb.Bounds.Left + (this.minThumb.Bounds.Width / 2);
		var minTextHalfWidth = this.minValueText.Width / 2;
		var pos = new Point((thumbCenterX - minTextHalfWidth), this.minThumb.Bounds.Top + 2);

		ctx.DrawText(minValueText, pos);
	}

	private void RenderMaxValueText(DrawingContext ctx)
	{
		var thumbCenterX = this.maxThumb.Bounds.Left + (this.maxThumb.Bounds.Width / 2);
		var maxTextHalfWidth = this.maxValueText.Width / 2;
		var pos = new Point((thumbCenterX - maxTextHalfWidth), this.maxThumb.Bounds.Top + 2);

		ctx.DrawText(maxValueText, pos);
	}
}
