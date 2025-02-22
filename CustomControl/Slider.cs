using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace CustomControl;

public class Slider : Control
{
	private double leftPadding = 3;
	private double rightPadding = 3;
	private Point mousePos;
	private Thumb minThumb;
	private Thumb maxThumb;

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

	// TODO: Add min thumb text color prop

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

	protected override void OnLoaded(RoutedEventArgs e)
	{
		var thumbWidth = 65;
		var thumbHeight = 35;
		var thumbHalfHeight = thumbHeight / 2;
		var halfHeight = Bounds.Height / 2;

		this.minThumb = new Thumb(new Point(0, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		var minHoverColorHSV = Colors.IndianRed.ToHsv();

		var newValue = minHoverColorHSV.V + (minHoverColorHSV.V * 0.2);
		minHoverColorHSV = new HsvColor(minHoverColorHSV.A, minHoverColorHSV.H, minHoverColorHSV.S, newValue);
		this.minThumb.HoverColor = minHoverColorHSV.ToRgb();
		this.minThumb.NonHoverColor = Colors.IndianRed;

		this.maxThumb = new Thumb(new Point(Bounds.Width - thumbWidth, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.maxThumb.HoverColor = Colors.Gray;
		this.maxThumb.NonHoverColor = Colors.White;

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

	private FormattedText minValueText;

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
			Brushes.White);

		var textWidth = minValueText.Width;

		if (textWidth > this.minThumb.Bounds.Width)
		{
			this.minThumb.Bounds = this.minThumb.Bounds.SetWidth(textWidth + leftPadding + rightPadding);
		}

		RenderBackground(ctx);
		RenderSliderLine(ctx);
		RenderMinThumb(ctx);
		RenderMaxThumb(ctx);
		RenderMinValueText(ctx);
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
		var borderClr = Colors.Transparent;

		ctx.DrawRectangle(this.minThumb.Bounds, this.minThumb.Color, borderClr, 5);
	}

	private void RenderMaxThumb(DrawingContext ctx)
	{
		var borderClr = Colors.Transparent;

		ctx.DrawRectangle(this.maxThumb.Bounds, this.maxThumb.Color, borderClr, 5);
	}

	private void RenderMinValueText(DrawingContext ctx)
	{
		var pos = new Point(this.minThumb.Bounds.Left + leftPadding, this.minThumb.Bounds.Top + 2);

		ctx.DrawText(minValueText, pos);
	}
}
