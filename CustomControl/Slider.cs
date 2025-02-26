using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace CustomControl;

// TODO: Make sure that the width of the control is not less than the combined width of the thumbs

public class Slider : Control
{
	private const double DefaultHeight = 35;
	private const double TextPaddingPercent = 0.2;
	private Point mousePos;
	private Thumb minThumb;
	private Thumb maxThumb;
	private FormattedText minValueText;
	private FormattedText maxValueText;
	private bool skipMaxValueCoerce;
	private bool isLeftMouseDown;

	public static readonly StyledProperty<IBrush?> BackgroundProperty =
		AvaloniaProperty.Register<Slider, IBrush?>(nameof(Background), defaultValue: Brushes.Transparent);

	public static readonly StyledProperty<double> MinValueProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(MinValue), defaultValue: 0);

	public static readonly StyledProperty<double> MaxValueProperty =
		AvaloniaProperty.Register<Slider, double>(nameof(MaxValue), defaultValue: 100);

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

				slider.minThumb.Color = newClr;

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

				slider.maxThumb.Color = newClr;

				return newClr;
			});

	public static readonly StyledProperty<Color> TrackColorProperty =
		AvaloniaProperty.Register<Slider, Color>( nameof(TrackColor), defaultValue: Colors.White);

	public static readonly StyledProperty<Color> MinThumbHoverColorProperty =
		AvaloniaProperty.Register<Slider, Color>( nameof(MinThumbHoverColor), defaultValue: Color.FromArgb(30, 255, 255, 255));

	public static readonly StyledProperty<Color> MaxThumbHoverColorProperty =
		AvaloniaProperty.Register<Slider, Color>( nameof(MaxThumbHoverColor), defaultValue: Color.FromArgb(30, 255, 255, 255));

	public static readonly StyledProperty<double> ValueTextSizeProperty =
		AvaloniaProperty.Register<Slider, double>( nameof(ValueTextSize), defaultValue: 20);

	public static readonly StyledProperty<double> TrackThicknessProperty =
		AvaloniaProperty.Register<Slider, double>( nameof(TrackThickness), defaultValue: 2);

	private bool skipWidthCoerce;

	public static readonly StyledProperty<double> WidthProperty = AvaloniaProperty.Register<Slider, double>(
		nameof(Width),
		coerce: (obj, newValue) =>
		{
			if (obj is not Slider slider)
			{
				throw new Exception("The object is not a Slider.");
			}

			if (slider.skipWidthCoerce)
			{
				return newValue;
			}

			var minWidth = slider.minThumb.Width + slider.maxThumb.Width;

			newValue = newValue < minWidth
				? minWidth
				: newValue;

			return newValue;
		});

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

	public new double Width
	{
		get => GetValue(WidthProperty);
		set
		{
			SetValue(WidthProperty, value);
			base.Width = value;
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

	public Color MinThumbHoverColor
	{
		get => GetValue(MinThumbHoverColorProperty);
		set => SetValue(MinThumbHoverColorProperty, value);
	}

	public Color MaxThumbHoverColor
	{
		get => GetValue(MaxThumbHoverColorProperty);
		set => SetValue(MaxThumbHoverColorProperty, value);
	}

	public double ValueTextSize
	{
		get => GetValue(ValueTextSizeProperty);
		set => SetValue(ValueTextSizeProperty, value);
	}

	public double TrackThickness
	{
		get => GetValue(TrackThicknessProperty);
		set => SetValue(TrackThicknessProperty, value);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.Width = 200;
		base.Height = DefaultHeight;

		const int thumbWidth = 65;
		const double thumbHeight = DefaultHeight;
		const double thumbHalfHeight = thumbHeight / 2;

		var halfHeight = base.Height / 2;

		this.minThumb = new Thumb(new Point(0, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.minThumb.Color = MinThumbColor;

		this.maxThumb = new Thumb(new Point(base.Width - thumbWidth, halfHeight - thumbHalfHeight), new Size(thumbWidth, thumbHeight));
		this.maxThumb.Color = MaxThumbColor;

		var minWidth = this.minThumb.Width + this.maxThumb.Width;

		if (base.Width < minWidth)
		{
			base.Width = minWidth;
		}

		var minPosX = CalcPosFromValue(MinValue);
		var maxPosX = CalcPosFromValue(MaxValue);

		minThumb.SetCenterX(minPosX);
		maxThumb.SetCenterX(maxPosX);

		Width = Width;

		ProcessCollisions();
		InvalidateVisual();

		if (VisualRoot is TopLevel topLevel)
		{
			topLevel.PointerExited += TopLevelOnPointerExited;
		}

		base.OnLoaded(e);
	}

	protected override void OnUnloaded(RoutedEventArgs e)
	{
		if (VisualRoot is TopLevel topLevel)
		{
			topLevel.PointerExited -= TopLevelOnPointerExited;
		}

		base.OnUnloaded(e);
	}

	private void TopLevelOnPointerExited(object? sender, PointerEventArgs e)
	{
		this.isLeftMouseDown = false;
		this.minThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);
		this.maxThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);

		if (e.Pointer.Captured is not null)
		{
			e.Pointer.Capture(null);
		}

		InvalidateVisual();
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		this.isLeftMouseDown = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
		this.mousePos = e.GetCurrentPoint(this).Position;

		this.minThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);
		this.maxThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);

		if (this.minThumb.Draggable || this.maxThumb.Draggable)
		{
			e.Pointer.Capture(this);
		}

		base.OnPointerPressed(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		this.isLeftMouseDown = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
		this.minThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);
		this.maxThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);

		e.Pointer.Capture(null);

		base.OnPointerReleased(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		this.mousePos = e.GetCurrentPoint(this).Position;
		this.minThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);
		this.maxThumb.UpdateNew(this.mousePos, this.isLeftMouseDown);

		if (this.minThumb.Draggable)
		{
			var leftThumbCanMove = this.minThumb.Right < this.maxThumb.Left;
			var rightThumbCanMove = this.maxThumb.Right < Bounds.Width;

			if (leftThumbCanMove || rightThumbCanMove)
			{
				MinValue = CalcMinValueFromMinPos();
			}
		}

		if (this.maxThumb.Draggable)
		{
			var leftThumbCanMove = this.minThumb.Left > 0;
			var rightThumbCanMove = this.maxThumb.Left > this.minThumb.Right;

			if (leftThumbCanMove || rightThumbCanMove)
			{
				MinValue = CalcMinValueFromMinPos();
				MaxValue = CalcMaxValueFromMaxPos();
			}
		}

		ProcessCollisions();
		InvalidateVisual();

		base.OnPointerMoved(e);
	}

	protected override void OnPointerEntered(PointerEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;
		e.Pointer.Capture(null);

		base.OnPointerEntered(e);
	}

	protected override void OnPointerExited(PointerEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;
		e.Pointer.Capture(null);

		base.OnPointerExited(e);
	}

	protected override void OnLostFocus(RoutedEventArgs e)
	{
		this.minThumb.Draggable = false;
		this.maxThumb.Draggable = false;

		base.OnLostFocus(e);
	}

	private double CalcMinValueFromMinPos()
	{
		var halfWidth = this.minThumb.HalfWidth;
		var center = this.minThumb.Left + halfWidth; // X is the left side of the control

		// Calculate the minimum value by mapping the draggable width to the numerical range settings
		var newValueMin = center.MapValue(halfWidth, Width - halfWidth, RangeMinValue, RangeMaxValue);
		newValueMin = newValueMin < RangeMinValue ? RangeMinValue : newValueMin;

		return Math.Round(newValueMin, 2);
	}

	private double CalcMaxValueFromMaxPos()
	{
		var halfWidth = this.maxThumb.HalfWidth;
		var center = this.maxThumb.Left + halfWidth; // X is the left side of the control

		// Calculate the maximum value by mapping the draggable width to the numerical range settings
		var newValueMax = center.MapValue(halfWidth, Width - halfWidth, RangeMinValue, RangeMaxValue);
		newValueMax = newValueMax > RangeMaxValue ? RangeMaxValue : newValueMax;

		return Math.Round(newValueMax, 2);
	}

	private double CalcPosFromValue(double value)
	{
		var pixelStartX = this.minThumb.HalfWidth;
		var pixelStopX = Bounds.Width - this.maxThumb.HalfWidth;

		var leftPosX = value.MapValue(RangeMinValue, RangeMaxValue, pixelStartX, pixelStopX);

		return leftPosX;
	}

	private double CalcValueFromPos(double posX)
	{
		var halfWidth = this.maxThumb.HalfWidth;

		// Calculate the maximum value by mapping the draggable width to the numerical range settings
		var newValueMax = posX.MapValue(halfWidth, Width - halfWidth, RangeMinValue, RangeMaxValue);
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
			ValueTextSize,
			Brushes.Black);

		this.maxValueText = new FormattedText(
			MaxValue.ToStringRounded(DecimalPlaces),
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			Typeface.Default,
			ValueTextSize,
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

		// If the width of the min value text is larger than the thumb width, increase the size of the
		// thumb width to accommodate the text
		if (minTextWidth > this.minThumb.Width)
		{
			this.minThumb.SetWidth(minTextWidth + (minTextWidth * TextPaddingPercent));
		}

		// If the width of the max value text is larger than the thumb width, increase the size of the
		// thumb width to accommodate the text
		if (maxTextWidth > this.maxThumb.Width)
		{
			this.maxThumb.SetWidth(maxTextWidth + (maxTextWidth * TextPaddingPercent));
		}

		var isOverlapping = this.minThumb.Bounds.Intersects(this.maxThumb.Bounds);

		if (isOverlapping)
		{
			var overlapAmount = this.minThumb.Bounds.Intersect(this.maxThumb.Bounds).Width;

			var isSpaceToLeftOfMinThumb = this.minThumb.Left > 0;
			var isSpaceToRightOfMaxThumb = this.maxThumb.Right < Bounds.Width;
			var noSpaceBetween = this.minThumb.Right > this.maxThumb.Left && this.minThumb.Left < this.maxThumb.Left;

			if (this.minThumb.Draggable)
			{
				// Push the max thumb to the right
				this.maxThumb.SetLeft(this.minThumb.Right);
			}
			else if (this.maxThumb.Draggable)
			{
				// Push the min thumb to the left
				this.minThumb.SetRight(this.maxThumb.Left);
			}

			var notDraggingAnyThumbs = !this.minThumb.Draggable && !this.maxThumb.Draggable;

			if (notDraggingAnyThumbs && isSpaceToLeftOfMinThumb && isSpaceToRightOfMaxThumb && noSpaceBetween)
			{
				var overlapHalf = overlapAmount / 2;

				this.minThumb.SetRight(this.minThumb.Right - overlapHalf);
				this.maxThumb.SetLeft(this.maxThumb.Left + overlapHalf);
			}
		}

		// If the min thumb is past the left edge of the slider
		if (this.minThumb.Left < 0)
		{
			this.minThumb.SetLeft(0);
		}

		// If the max thumb is past the right edge of the slider
		if (this.maxThumb.Right > Bounds.Width)
		{
			this.maxThumb.SetRight(Bounds.Width);
		}

		var noSpaceToLeftOfMinThumb = this.minThumb.Left <= 0;
		var noSpaceToRightOfMinThumb = this.minThumb.Right >= this.maxThumb.Left;

		// If there is no space to the left or right side of the min thumb
		if (noSpaceToLeftOfMinThumb && noSpaceToRightOfMinThumb)
		{
			this.minThumb.SetLeft(0);
			this.maxThumb.SetLeft(this.minThumb.Right);
		}

		var noSpaceToLeftOfMaxThumb = this.maxThumb.Left < this.minThumb.Right;
		var noSpaceToRightOfMaxThumb = this.maxThumb.Right >= Bounds.Width;

		// If there is no space to the left or right side of the max thumb
		if (noSpaceToLeftOfMaxThumb && noSpaceToRightOfMaxThumb)
		{
			this.maxThumb.SetRight(Bounds.Width);
			this.minThumb.SetRight(this.maxThumb.Left);
		}
	}

	private void RenderBackground(DrawingContext ctx)
	{
		ctx.DrawRectangle(new Point(0, 0), Width, Height, Background.GetColor(), Colors.Transparent);
	}

	private void RenderSliderTrack(DrawingContext ctx)
	{
		var pen = new Pen(new ImmutableSolidColorBrush(TrackColor), TrackThickness);

		var halfHeight = Bounds.Height / 2;
		ctx.DrawLine(pen, new Point(0, halfHeight), new Point(Width, halfHeight));
	}

	private void RenderMinThumb(DrawingContext ctx)
	{
		ctx.DrawRectangle(this.minThumb.Bounds, this.minThumb.Color, Colors.Transparent, ThumbRadius);

		if (this.minThumb.MouseIsOver)
		{
			// Draw an over loy over the control to simulate a brighter hover effect
			ctx.DrawRectangle(this.minThumb.Bounds, MinThumbHoverColor, Colors.Transparent, ThumbRadius);
		}
	}

	private void RenderMaxThumb(DrawingContext ctx)
	{
		ctx.DrawRectangle(this.maxThumb.Bounds, this.maxThumb.Color, Colors.Transparent, ThumbRadius);

		// Draw an over loy over the control to simulate a brighter hover effect
		if (this.maxThumb.MouseIsOver)
		{
			ctx.DrawRectangle(this.maxThumb.Bounds, MaxThumbHoverColor, Colors.Transparent, ThumbRadius);
		}
	}

	private void RenderMinValueText(DrawingContext ctx)
	{
		var centerX = this.minThumb.Left + (this.minThumb.Width / 2);
		var centerY = this.minThumb.Top + (this.minThumb.Height / 2);

		var textHalfWidth = this.minValueText.Width / 2;
		var textHalfHeight = this.minValueText.Height / 2;

		var pos = new Point((centerX - textHalfWidth), (centerY - textHalfHeight) - 2);

		ctx.DrawText(minValueText, pos);
	}

	private void RenderMaxValueText(DrawingContext ctx)
	{
		var centerX = this.maxThumb.Left + (this.maxThumb.Width / 2);
		var centerY = this.minThumb.Top + (this.minThumb.Height / 2);

		var textHalfWidth = this.maxValueText.Width / 2;
		var textHalfHeight = this.minValueText.Height / 2;

		var pos = new Point((centerX - textHalfWidth), (centerY - textHalfHeight) - 2);

		ctx.DrawText(maxValueText, pos);
	}
}
