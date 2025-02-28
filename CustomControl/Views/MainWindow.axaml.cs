using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CustomControl.Views;

public partial class MainWindow : Window
{
	public static readonly StyledProperty<double> ValueMinProperty =
		AvaloniaProperty.Register<MainWindow, double>(nameof(MinValue), defaultValue: 0f);

	public static readonly StyledProperty<double> ValueMaxProperty =
		AvaloniaProperty.Register<MainWindow, double>(nameof(MaxValue), defaultValue: 100f);

	public MainWindow()
	{
		InitializeComponent();
	}

	public double MinValue
	{
		get => GetValue(ValueMinProperty);
		set => SetValue(ValueMinProperty, value);
	}

	public double MaxValue
	{
		get => GetValue(ValueMaxProperty);
		set => SetValue(ValueMaxProperty, value);
	}

	private void Add_OnClick(object? sender, RoutedEventArgs e)
	{
		Slider.Width += 10;
	}

	private void Sub_OnClick(object? sender, RoutedEventArgs e)
	{
		Slider.Width -= 10;
	}
}
