using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;

namespace WinRingLight;

public partial class MainWindow : Window
{
    private bool _isInitialized = false;
    private string _currentLang = "EN";
    private string _currentMode = "Ring";
    private LightWindow _lightWindow;

    // Global Hotkey (V20)
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    private const uint WDA_NONE = 0x00;
    private const uint WDA_EXCLUDEFROMCAPTURE = 0x11;
    private const uint MOD_ALT = 0x0001; // Alt modifier
    private const int HOTKEY_ID = 9000;
    private const int HOTKEY_LIGHT_ID = 9001;
    private const uint VK_H = 0x48;
    private const uint VK_L = 0x4C;

    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _lightWindow = new LightWindow();
        _lightWindow.Show();
        
        // Ensure menu always stays on top of the light (V26)
        this.Owner = _lightWindow;
        
        this.Topmost = true;
        
        // Position at bottom center of screen
        var desktopWorkingArea = SystemParameters.WorkArea;
        this.Left = (desktopWorkingArea.Width - this.ActualWidth) / 2;
        this.Top = desktopWorkingArea.Height - this.ActualHeight - 20;

        // Register Global Hotkeys with Alt Modifier (V24)
        var helper = new WindowInteropHelper(this);
        RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_ALT, VK_H); 
        RegisterHotKey(helper.Handle, HOTKEY_LIGHT_ID, MOD_ALT, VK_L); 
        HwndSource source = HwndSource.FromHwnd(helper.Handle);
        source.AddHook(HwndHook);

        _isInitialized = true;
        SetMode("Ring"); 
        UpdateLanguage();
        SyncLight();
        
        this.Visibility = Visibility.Visible;
        this.Activate();
    }



    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY)
        {
            if (wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleControlPanel();
                handled = true;
            }
            else if (wParam.ToInt32() == HOTKEY_LIGHT_ID)
            {
                ToggleLight();
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    protected override void OnClosed(EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        UnregisterHotKey(helper.Handle, HOTKEY_ID);
        UnregisterHotKey(helper.Handle, HOTKEY_LIGHT_ID);
        base.OnClosed(e);
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
            SoftnessSlider.Value,
            CornerRadiusSlider.Value
        );

        int sysBrightness = (int)(BrightnessSlider.Value / 0.9 * 100);
        BrightnessHelper.SetBrightness(sysBrightness);
    }

    private Color Interpolate(Color c1, Color c2, double t) => Color.FromRgb(
            (byte)(c1.R + (c2.R - c1.R) * t),
            (byte)(c1.G + (c2.G - c1.G) * t),
            (byte)(c1.B + (c2.B - c1.B) * t)
        );

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
            CaptureCheckBox.Content = "Hide from Screen Capture";
            ShortcutText.Text = "Shortcuts: 'Alt+H' (Panel), 'Alt+L' (Light), 'Esc' (Exit)";
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
            CaptureCheckBox.Content = "Ekran Kaydında Gizle";
            ShortcutText.Text = "Kısayollar: 'Alt+H' (Panel), 'Alt+L' (Işık), 'Esc' (Çıkış)";
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) 
        {
            e.Handled = true;
            Application.Current.Shutdown();
        }
    }

    private void ToggleControlPanel()
    {
        if (this.Visibility == Visibility.Visible)
        {
            this.Visibility = Visibility.Collapsed;
        }
        else
        {
            this.Visibility = Visibility.Visible;
            this.Topmost = true;
            this.Activate(); 
        }
    }

    private void ToggleLight()
    {
        if (_lightWindow == null) return;

        if (_lightWindow.Visibility == Visibility.Visible)
        {
            _lightWindow.Visibility = Visibility.Collapsed;
        }
        else
        {
            _lightWindow.Visibility = Visibility.Visible;
            _lightWindow.Topmost = true; // Refresh priority
        }
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => SyncLight();
    private void Mode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string mode) SetMode(mode);
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
    
    private void CaptureCheckBox_Click(object sender, RoutedEventArgs e)
    {
        uint affinity = (CaptureCheckBox.IsChecked == true) ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE;
        
        // CheckBox now only toggles visibility of the Control Panel (Menu) in recordings
        SetWindowDisplayAffinity(new WindowInteropHelper(this).Handle, affinity);
    }

    private void ControlPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
    }
}