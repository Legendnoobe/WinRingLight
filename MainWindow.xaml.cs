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
    private string _currentLang = "EN";
    private string _currentMode = "Ring";

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
        this.SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        
        // Finalized Defaults (V11)
        SetMode("Ring"); 
        UpdateLanguage();
        UpdateGeometries();
        UpdateColorFromSlider(); 
        UpdateOpacity();
        UpdateBlur();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isInitialized) UpdateGeometries();
    }

    private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
    {
        _currentLang = (_currentLang == "EN") ? "TR" : "EN";
        UpdateLanguage();
    }

    private void UpdateLanguage()
    {
        if (_currentLang == "EN")
        {
            TitleText.Text = "Windows RingLight";
            LangBtn.Content = "TR";
            ExitBtn.Content = "Exit";
            ColorText.Text = "Color Temperature";
            BrightnessText.Text = "Brightness (Dimmer)";
            OpacityText.Text = "Intensity (Solidness)"; 
            SoftnessText.Text = "Softness (Glow)"; 
            ModesText.Text = "Light Mode & Size";
            ModeFullBtn.Content = "Full";
            ModeSidesBtn.Content = "Sides";
            ModeTopBtn.Content = "Top";
            ModeRingBtn.Content = "Ring";
            ModeRectBtn.Content = "Frame";
            ThicknessText.Text = "Thickness";
            CornerText.Text = "Corner Radius";
            ShortcutText.Text = "Shortcuts: 'H' (Hide), 'Esc' (Exit) | Click-through active";
        }
        else
        {
            TitleText.Text = "Windows RingLight";
            LangBtn.Content = "EN";
            ExitBtn.Content = "Kapat";
            ColorText.Text = "Renk Sıcaklığı";
            BrightnessText.Text = "Parlaklık (Dimmer)";
            OpacityText.Text = "Işık Şiddeti (Matlık)"; 
            SoftnessText.Text = "Yumuşaklık (Işıltı)"; 
            ModesText.Text = "Işık Modu & Boyut";
            ModeFullBtn.Content = "Tam";
            ModeSidesBtn.Content = "Yanlar";
            ModeTopBtn.Content = "Üst";
            ModeRingBtn.Content = "Halka";
            ModeRectBtn.Content = "Çerçeve";
            ThicknessText.Text = "Kalınlık";
            CornerText.Text = "Köşe Kavisi";
            ShortcutText.Text = "Kısayollar: 'H' (Gizle), 'Esc' (Çıkış) | Ekrana tıklayabilirsiniz";
        }
    }

    private void UpdateGeometries()
    {
        if (!_isInitialized) return;

        double factor = ThicknessSlider?.Value ?? 0.5;
        
        // Standard "Right = More" Logic:
        // Bars expand from edges
        double sideWidth = this.ActualWidth * 0.45 * factor;
        double topHeight = this.ActualHeight * 0.45 * factor;

        if (SideLeft != null) SideLeft.Width = sideWidth;
        if (SideRight != null) SideRight.Width = sideWidth;
        if (TopMode != null) TopMode.Height = topHeight;

        UpdateRingGeometry();
        UpdateRoundedRectGeometry();
    }

    private void UpdateRingGeometry()
    {
        if (RingMode == null) return;

        double width = this.ActualWidth;
        double height = this.ActualHeight;
        double centerX = width / 2;
        double centerY = height / 2;
        
        double minDim = Math.Min(width, height);
        double midRadius = minDim * 0.35; // Center point of the ring line
        double factor = ThicknessSlider?.Value ?? 0.4;
        
        // Dual-Direction Growth (V11): Expands both outwards and inwards
        double ringThickness = minDim * 0.45 * factor;
        double outerRadius = midRadius + (ringThickness / 2);
        double innerRadius = midRadius - (ringThickness / 2);

        // Safety clamps
        outerRadius = Math.Max(10, outerRadius);
        innerRadius = Math.Max(0, innerRadius);

        if (RingMode.Data is CombinedGeometry combined)
        {
            if (combined.Geometry1 is EllipseGeometry outer)
            {
                outer.Center = new Point(centerX, centerY);
                outer.RadiusX = outerRadius;
                outer.RadiusY = outerRadius;
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
        
        double factor = ThicknessSlider?.Value ?? 0.4;
        // Right (1.0) = Smaller margin = Thicker
        double margin = (width * 0.45) * (1.1 - factor); 
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
            // Right (1.0) = Maximum Glow
            LightBlur.Radius = SoftnessSlider.Value * 120; // Increased range for better "faint" look
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
            // Right (0.9) = Brighter (Lower overlay opacity)
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
            // Right (1.0) = Opaque/Solid (1.0)
            LightContainer.Opacity = OpacitySlider.Value;
        }
    }

    private void UpdateColorFromSlider()
    {
        if (ColorSlider == null) return;

        double val = ColorSlider.Value;
        Color warm = (Color)ColorConverter.ConvertFromString("#FFFFA4");
        Color neutral = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        Color cool = (Color)ColorConverter.ConvertFromString("#58B0FF");

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
        _currentMode = mode;
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

        UpdateSliderVisibility();
        UpdateGeometries();
    }

    private void UpdateSliderVisibility()
    {
        if (!_isInitialized) return;

        if (ThicknessGroup != null) ThicknessGroup.Visibility = Visibility.Visible;
        if (SoftnessGroup != null) SoftnessGroup.Visibility = Visibility.Visible;
        if (CornerGroup != null) CornerGroup.Visibility = Visibility.Collapsed;

        switch (_currentMode)
        {
            case "Full":
                if (ThicknessGroup != null) ThicknessGroup.Visibility = Visibility.Collapsed;
                if (SoftnessGroup != null) SoftnessGroup.Visibility = Visibility.Collapsed;
                break;
            case "RoundedRect":
                if (CornerGroup != null) CornerGroup.Visibility = Visibility.Visible;
                break;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    private void ControlPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
    }
}