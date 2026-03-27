using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WinRingLight;

public partial class MainWindow : Window
{
    private string _currentColor = "#FFFFFF";
    private bool _isInitialized = false;

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
        this.SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        UpdateGeometries();
        UpdateColorFromSlider(); 
        UpdateOpacity();
        UpdateBlur();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isInitialized) UpdateGeometries();
    }

    private void UpdateGeometries()
    {
        if (!_isInitialized) return;

        double factor = ThicknessSlider?.Value ?? 0.5;
        
        // Update Bars
        double sideWidth = this.ActualWidth * 0.4 * factor;
        double topHeight = this.ActualHeight * 0.4 * factor;

        if (SideLeft != null) SideLeft.Width = sideWidth;
        if (SideRight != null) SideRight.Width = sideWidth;
        if (TopMode != null) TopMode.Height = topHeight;

        UpdateRingGeometry();
        UpdateRoundedRectGeometry();
        UpdateBlur();
    }

    private void UpdateRingGeometry()
    {
        if (RingMode == null) return;

        double centerX = this.ActualWidth / 2;
        double centerY = this.ActualHeight / 2;
        
        double baseRadius = Math.Min(this.ActualWidth, this.ActualHeight) * 0.48;
        double thicknessFactor = ThicknessSlider?.Value ?? 0.5;
        
        // Higher Slider = Thicker Ring = Smaller Inner Radius
        double innerRadius = baseRadius * (1.0 - thicknessFactor);
        innerRadius = Math.Max(0, Math.Min(baseRadius * 0.98, innerRadius));

        if (RingMode.Data is CombinedGeometry combined)
        {
            if (combined.Geometry1 is EllipseGeometry outer)
            {
                outer.Center = new Point(centerX, centerY);
                outer.RadiusX = baseRadius;
                outer.RadiusY = baseRadius;
            }
            if (combined.Geometry2 is EllipseGeometry inner)
            {
                inner.Center = new Point(centerX, centerY);
                inner.RadiusX = innerRadius;
                inner.RadiusY = innerRadius;
            }
        }
    }

    private void UpdateRoundedRectGeometry()
    {
        if (RoundedRectMode == null || RR_Outer == null || RR_Inner == null) return;

        double width = this.ActualWidth;
        double height = this.ActualHeight;
        
        double thicknessFactor = ThicknessSlider?.Value ?? 0.5;
        // Corrected: Right (1.0) = Thick, Left (0.05) = Thin
        double margin = (width * 0.4) * (1.0 - thicknessFactor); 
        margin = Math.Max(0, margin);

        double radiusFactor = CornerRadiusSlider?.Value ?? 0.2;
        double radius = Math.Min(width, height) * 0.4 * radiusFactor;

        RR_Outer.Rect = new Rect(0, 0, width, height);
        RR_Inner.Rect = new Rect(margin, margin, Math.Max(1, width - 2 * margin), Math.Max(1, height - 2 * margin));
        RR_Inner.RadiusX = radius;
        RR_Inner.RadiusY = radius;
    }

    private void UpdateBlur()
    {
        if (LightBlur != null && SoftnessSlider != null)
        {
            // Blur radius 0 to 80
            LightBlur.Radius = SoftnessSlider.Value * 80;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Application.Current.Shutdown();
        else if (e.Key == Key.H) ToggleControlPanel();
    }

    private void ToggleControlPanel()
    {
        if (ControlPanel.Visibility == Visibility.Visible)
        {
            var anim = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
            anim.Completed += (s, e) => ControlPanel.Visibility = Visibility.Collapsed;
            ControlPanel.BeginAnimation(UIElement.OpacityProperty, anim);
        }
        else
        {
            ControlPanel.Visibility = Visibility.Visible;
            var anim = new System.Windows.Media.Animation.DoubleAnimation(1, TimeSpan.FromSeconds(0.3));
            ControlPanel.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isInitialized) return;
        
        if (sender == BrightnessSlider)
        {
            BrightnessOverlay.Opacity = 0.9 - BrightnessSlider.Value;
        }
        else if (sender == ColorSlider)
        {
            UpdateColorFromSlider();
        }
        else if (sender == OpacitySlider)
        {
            UpdateOpacity();
        }
        else if (sender == SoftnessSlider)
        {
            UpdateBlur();
        }
        else
        {
            UpdateGeometries();
        }
    }

    private void UpdateOpacity()
    {
        if (LightContainer != null && OpacitySlider != null)
        {
            LightContainer.Opacity = OpacitySlider.Value;
        }
    }

    private void UpdateColorFromSlider()
    {
        if (ColorSlider == null) return;

        double val = ColorSlider.Value;
        Color warm = (Color)ColorConverter.ConvertFromString("#FFFFA4");
        Color neutral = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        Color cool = (Color)ColorConverter.ConvertFromString("#D9EFFF");

        Color result;
        if (val < 0.5)
        {
            double t = val * 2; // 0 to 1
            result = Interpolate(warm, neutral, t);
        }
        else
        {
            double t = (val - 0.5) * 2; // 0 to 1
            result = Interpolate(neutral, cool, t);
        }

        _currentColor = result.ToString();
        UpdateBrushes();
    }

    private Color Interpolate(Color c1, Color c2, double t)
    {
        return Color.FromRgb(
            (byte)(c1.R + (c2.R - c1.R) * t),
            (byte)(c1.G + (c2.G - c1.G) * t),
            (byte)(c1.B + (c2.B - c1.B) * t)
        );
    }

    private void UpdateBrushes()
    {
        if (!_isInitialized) return;

        Color color = (Color)ColorConverter.ConvertFromString(_currentColor);
        var brush = new SolidColorBrush(color);

        FullMode.Fill = brush;
        SideLeft.Fill = brush;
        SideRight.Fill = brush;
        TopMode.Fill = brush;
        RingMode.Fill = brush;
        RoundedRectMode.Fill = brush;
    }

    private void Mode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string mode)
        {
            SetMode(mode);
        }
    }

    private void SetMode(string mode)
    {
        FullMode.Visibility = Visibility.Collapsed;
        SidesMode.Visibility = Visibility.Collapsed;
        TopMode.Visibility = Visibility.Collapsed;
        RingMode.Visibility = Visibility.Collapsed;
        RoundedRectMode.Visibility = Visibility.Collapsed;

        switch (mode)
        {
            case "Full": FullMode.Visibility = Visibility.Visible; break;
            case "Sides": SidesMode.Visibility = Visibility.Visible; break;
            case "Top": TopMode.Visibility = Visibility.Visible; break;
            case "Ring": RingMode.Visibility = Visibility.Visible; break;
            case "RoundedRect": RoundedRectMode.Visibility = Visibility.Visible; break;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void ControlPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
    }
}