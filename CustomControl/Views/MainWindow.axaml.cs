using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CustomControl.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
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
