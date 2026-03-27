using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WinRingLight;

public partial class MainWindow : Window
{
    private bool _isInitialized = false;
    private string _currentLang = "EN";
    private string _currentMode = "Ring";
    private LightWindow _lightWindow;

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Start the click-through light window
        _lightWindow = new LightWindow();
        _lightWindow.Show();
        
        // Fix Z-Order (V16): MainWindow (Panel) owned by LightWindow (Overlay)
        // In WPF, the owned window is ALWAYS in front of the owner.
        this.Owner = _lightWindow; 
        this.Topmost = true;
        this.Activate();

        _isInitialized = true;
        
        // Finalized Defaults (V13)
        SetMode("Ring"); 
        UpdateLanguage();
        SyncLight();
    }

    private void SyncLight()
    {
        if (!_isInitialized || _lightWindow == null) return;

        Color warm = (Color)ColorConverter.ConvertFromString("#FFFFA4");
        Color neutral = (Color)ColorConverter.ConvertFromString("#FFFFFF");
        Color cool = (Color)ColorConverter.ConvertFromString("#58B0FF");

        double val = ColorSlider.Value;
        Color result;
        if (val < 0.5) {
            double t = val * 2;
            result = Interpolate(warm, neutral, t);
        } else {
            double t = (val - 0.5) * 2;
            result = Interpolate(neutral, cool, t);
        }

        _lightWindow.UpdateLight(
            _currentMode,
            result,
            ThicknessSlider.Value,
            OpacitySlider.Value,
            SoftnessSlider.Value
        );

        // System Brightness (0.9 limit mapped to 0-100)
        int sysBrightness = (int)(BrightnessSlider.Value / 0.9 * 100);
        BrightnessHelper.SetBrightness(sysBrightness);
    }

    private Color Interpolate(Color c1, Color c2, double t)
    {
        return Color.FromRgb(
            (byte)(c1.R + (c2.R - c1.R) * t),
            (byte)(c1.G + (c2.G - c1.G) * t),
            (byte)(c1.B + (c2.B - c1.B) * t)
        );
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
            OpacityText.Text = "Intensity (Solid)"; 
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
        SyncLight();
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
        UpdateSliderVisibility();
        SyncLight();
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