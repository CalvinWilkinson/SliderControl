﻿using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace CustomControl;

public class Slider : Control
{
	private const double DefaultHeight = 35;
	private Point mousePos;
	private Thumb minThumb;
	private Thumb maxThumb;
	private FormattedText minValueText;
	private FormattedText maxValueText;

	public static readonly StyledProperty<IBrush?> BackgroundProperty =
		AvaloniaProperty.Register<Slider, IBrush?>(nameof(Background), defaultValue: Brushes.Transparent);

	public static readonly StyledProperty<double> MinValueProperty =
		AvaloniaProperty.Register<Slider, double>(
			nameof(MinValue),
			defaultValue: 0,
			coerce: (obj, newValue) =>
			{
				if (obj is not Slider slider)
				{
					throw new Exception("The object is not a Slider.");
				}

				return newValue;
				newValue = newValue < slider.RangeMinValue
					? Math.Round(slider.RangeMinValue, (int)slider.DecimalPlaces)
					: Math.Round(newValue, (int)slider.DecimalPlaces);

				return newValue;
			});

	public static readonly StyledProperty<double> MaxValueProperty =
		AvaloniaProperty.Register<Slider, double>(
			nameof(MaxValue),
			defaultValue: 100,
			coerce: (obj, newValue) =>
			{
				if (obj is not Slider slider)
				{
					throw new Exception("The object is not a Slider.");
				}

				return newValue > slider.RangeMaxValue
					? Math.Round(slider.RangeMaxValue, (int)slider.DecimalPlaces)
					: Math.Round(newValue, (int)slider.DecimalPlaces);
			});

	public static readonly StyledProperty<uint> DecimalPlacesProperty =
		AvaloniaProperty.Register<Slider, uint>(nameof(DecimalPlaces), defaultValue: 2);

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

	public static readonly StyledProperty<Color> TrackColorProperty =
		AvaloniaProperty.Register<Slider, Color>( nameof(TrackColor), defaultValue: Colors.White);

	public Slider()
	{
		base.Width = 41;
		base.Height = DefaultHeight;
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

	public uint DecimalPlaces
	{
		get => GetValue(DecimalPlacesProperty);
		set => SetValue(DecimalPlacesProperty, value);
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

	/// <summary>
	/// Gets or sets the width of the <see cref="Slider"/>.
	/// </summary>
	/// <remarks>The width of the <see cref="Slider"/> cannot be less then the combined with of the thumbs.</remarks>
	public new double Width
	{
		get => base.Width;
		set
		{
			var minWidth = this.minThumb.Width + this.maxThumb.Width;

			base.Width = value < minWidth ? minWidth : value;
		}
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

	public Color TrackColor
	{
		get => GetValue(TrackColorProperty);
		set => SetValue(TrackColorProperty, value);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		var thumbWidth = 65;
		var thumbHeight = DefaultHeight;
		var thumbHalfHeight = thumbHeight / 2;
		var halfHeight = Bounds.Height / 2;

		this.minThumb = new Thumb(new Point(0, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.minThumb.NonHoverColor = MinThumbColor;
		this.minThumb.HoverColor = MinThumbColor.IncreaseBrightness(25);

		this.maxThumb = new Thumb(new Point(Bounds.Width - thumbWidth, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.maxThumb.NonHoverColor = MaxThumbColor;
		this.maxThumb.HoverColor = MaxThumbColor.IncreaseBrightness(25);

		var minWidth = this.minThumb.Width + this.maxThumb.Width;

		if (Width < minWidth)
		{
			base.Width = minWidth;
		}

		this.minThumb.Update(new Point());
		this.maxThumb.Update(new Point());

		InvalidateVisual();

		base.OnLoaded(e);
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		this.mousePos = e.GetCurrentPoint(null).Position;

		this.minThumb.MouseDownPos = this.mousePos;
		this.maxThumb.MouseDownPos = this.mousePos;
		this.minThumb.Draggable = this.minThumb.Bounds.Contains(this.mousePos);
		this.maxThumb.Draggable = this.maxThumb.Bounds.Contains(this.mousePos);
		this.minThumb.Update(this.mousePos);
		this.maxThumb.Update(this.mousePos);

		base.OnPointerPressed(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;
		this.minThumb.Update(this.mousePos);
		this.maxThumb.Update(this.mousePos);

		base.OnPointerReleased(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		this.mousePos = e.GetCurrentPoint(null).Position;

		this.minThumb.Update(this.mousePos);
		this.maxThumb.Update(this.mousePos);

		if (this.minThumb.Draggable)
		{
			MinValue = CalcMinValue();
		}

		if (this.maxThumb.Draggable)
		{
			MaxValue = CalcMaxValue();
		}

		ProcessCollisions();
		InvalidateVisual();

		base.OnPointerMoved(e);
		return;
		this.mousePos = e.GetCurrentPoint(null).Position;
		// this.minThumb.MousePos = this.mousePos;
		// this.maxThumb.MousePos = this.mousePos;

		this.minThumb.Update(this.mousePos);
		this.maxThumb.Update(this.mousePos);

		var rangeMinX = this.minThumb.HalfWidth;
		var rangeMaxX = this.Bounds.Width - this.maxThumb.HalfWidth;
		var minValueX = this.minThumb.Left + this.maxThumb.HalfWidth;
		var maxValueX = this.maxThumb.Left + this.maxThumb.HalfWidth;

		MinValue = minValueX.MapValue(rangeMinX, rangeMaxX, RangeMinValue, RangeMaxValue);
		MaxValue = maxValueX.MapValue(rangeMinX, rangeMaxX, RangeMinValue, RangeMaxValue);

		InvalidateVisual();

		base.OnPointerMoved(e);
	}

	protected override void OnPointerEntered(PointerEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;

		base.OnPointerEntered(e);
	}

	protected override void OnPointerExited(PointerEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;

		base.OnPointerExited(e);
	}

	protected override void OnLostFocus(RoutedEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;

		base.OnLostFocus(e);
	}

	private double CalcMinValue()
	{
		var halfWidth = this.minThumb.HalfWidth;
		var center = this.minThumb.Left + halfWidth; // X is the left side of the control

		// Calculate the minimum value by mapping the draggable width to the numerical range settings
		var newValueMin = center.MapValue(halfWidth, Width - halfWidth, RangeMinValue, RangeMaxValue);
		newValueMin = newValueMin < RangeMinValue ? RangeMinValue : newValueMin;

		return Math.Round(newValueMin, 2);
	}

	private double CalcMaxValue()
	{
		var halfWidth = this.maxThumb.HalfWidth;
		var center = this.maxThumb.Left + halfWidth; // X is the left side of the control

		// Calculate the maximum value by mapping the draggable width to the numerical range settings
		var newValueMax = center.MapValue(halfWidth, Width - halfWidth, RangeMinValue, RangeMaxValue);
		newValueMax = newValueMax > RangeMaxValue ? RangeMaxValue : newValueMax;

		return Math.Round(newValueMax, 2);
	}

	public override void Render(DrawingContext ctx)
	{
		this.minValueText = new FormattedText(
			MinValue.ToStringRounded(DecimalPlaces),
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			Typeface.Default,
			20,
			Brushes.Black);

		this.maxValueText = new FormattedText(
			MaxValue.ToStringRounded(DecimalPlaces),
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			Typeface.Default,
			20,
			Brushes.Black);

		ProcessCollisions();

		RenderBackground(ctx);
		RenderSliderTrack(ctx);
		RenderMinThumb(ctx);
		RenderMaxThumb(ctx);
		RenderMinValueText(ctx);
		RenderMaxValueText(ctx);
	}

	private void ProcessCollisions()
	{
		var minTextWidth = minValueText.Width;
		var maxTextWidth = maxValueText.Width;

		if (minTextWidth > this.minThumb.Width)
		{
			this.minThumb.Bounds = this.minThumb.Bounds.SetWidth(minTextWidth + (minTextWidth * 0.2));
		}

		if (maxTextWidth > this.maxThumb.Width)
		{
			this.maxThumb.Bounds = this.maxThumb.Bounds.SetWidth(maxTextWidth + (maxTextWidth * 0.2));
		}

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

		var noSpaceToLeftOfMinThumb = minBounds.Left <= 0;
		var noSpaceToRightOfMinThumb = minBounds.Right >= maxBounds.Left;

		if (noSpaceToLeftOfMinThumb && noSpaceToRightOfMinThumb)
		{
			minBounds = minBounds.SetX(0);
			maxBounds = maxBounds.SetX(minBounds.Right);
		}

		var noSpaceToLeftOfMaxThumb = maxBounds.Left < minBounds.Right;
		var noSpaceToRightOfMaxThumb = maxBounds.Right >= Bounds.Width;

		// If the right side of the min thumb is past the left side of the max thumb
		if (noSpaceToLeftOfMaxThumb && noSpaceToRightOfMaxThumb)
		{
			minBounds = minBounds.SetX(maxBounds.Left - minBounds.Width);
		}

		this.minThumb.Bounds = minBounds;
		this.maxThumb.Bounds = maxBounds;
	}

	private void RenderBackground(DrawingContext ctx)
	{
		ctx.DrawRectangle(new Point(0, 0), Width, Height, Background.GetColor(), Colors.Transparent);
	}

	private void RenderSliderTrack(DrawingContext ctx)
	{
		var pen = new Pen(new ImmutableSolidColorBrush(TrackColor), 2);

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
		var thumbCenterX = this.minThumb.Left + (this.minThumb.Width / 2);
		var minTextHalfWidth = this.minValueText.Width / 2;
		var pos = new Point((thumbCenterX - minTextHalfWidth), this.minThumb.Top + 2);

		ctx.DrawText(minValueText, pos);
	}

	private void RenderMaxValueText(DrawingContext ctx)
	{
		var thumbCenterX = this.maxThumb.Left + (this.maxThumb.Width / 2);
		var maxTextHalfWidth = this.maxValueText.Width / 2;
		var pos = new Point((thumbCenterX - maxTextHalfWidth), this.maxThumb.Top + 2);

		ctx.DrawText(maxValueText, pos);
	}
}
