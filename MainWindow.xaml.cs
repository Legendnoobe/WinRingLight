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
    private Color? _currentMatrixBrushColor = null;
    
    private bool _isDraggingColorMap = false;
    private double _currentSaturation = 1.0;
    private double _currentValue = 1.0;

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
        _isInitialized = true;
        BuildMatrixGrid((int)MatrixRowsSlider.Value, (int)MatrixColsSlider.Value);
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

     private Color GetCurrentSliderColor()
    {
        double val = ColorSlider.Value;
        
        if (RgbModeCheckBox != null && RgbModeCheckBox.IsChecked == true)
        {
            double hue = HueSlider.Value;
            return ColorFromHSV(hue, _currentSaturation, _currentValue);
        }
        else
        {
            Color warm = (Color)ColorConverter.ConvertFromString("#FFFFA4");
            Color neutral = (Color)ColorConverter.ConvertFromString("#FFFFFF");
            Color cool = (Color)ColorConverter.ConvertFromString("#58B0FF");
            if (val < 0.5) {
                double t = val * 2;
                return Interpolate(warm, neutral, t);
            } else {
                double t = (val - 0.5) * 2;
                return Interpolate(neutral, cool, t);
            }
        }
    }

    private Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0) return Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
        else if (hi == 1) return Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
        else if (hi == 2) return Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
        else if (hi == 3) return Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
        else if (hi == 4) return Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
        else return Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
    }

    private void SyncLight()
    {
        if (!_isInitialized || _lightWindow == null) return;

        Color result = GetCurrentSliderColor();

        Color?[] matrixData = null;
        if (_currentMode == "Matrix" && MatrixPreviewGrid != null)
        {
            matrixData = new Color?[MatrixPreviewGrid.Children.Count];
            for (int i = 0; i < MatrixPreviewGrid.Children.Count; i++)
            {
                if (MatrixPreviewGrid.Children[i] is Button btn && btn.Tag is Color c)
                {
                    matrixData[i] = c;
                }
            }
        }

        _lightWindow.UpdateLight(
            _currentMode,
            result,
            ThicknessSlider.Value,
            OpacitySlider.Value,
            SoftnessSlider.Value,
            CornerRadiusSlider.Value,
            (int)MatrixRowsSlider.Value,
            (int)MatrixColsSlider.Value,
            matrixData
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
            ColorText.Text = "Main Colors";
            BrightnessText.Text = "Brightness (Dimmer)";
            OpacityText.Text = "Intensity (Solid)"; 
            SoftnessText.Text = "Softness (Glow)"; 
            ModesText.Text = "Light Mode & Size";
            ModeFullBtn.Content = "Full";
            ModeSidesBtn.Content = "Sides";
            ModeTopBtn.Content = "Top";
            ModeRingBtn.Content = "Ring";
            ModeRectBtn.Content = "Frame";
            if (ModeMatrixBtn != null) ModeMatrixBtn.Content = "Matrix";
            ThicknessText.Text = "Thickness";
            CornerText.Text = "Corner Radius";
            if (MatrixRowsText != null) MatrixRowsText.Text = $"Rows: {(int)MatrixRowsSlider.Value}";
            MatrixColsText.Text = $"Cols: {(int)MatrixColsSlider.Value}";
            if (MatrixBrushBtn != null) MatrixBrushBtn.Content = "Set Brush Color";
            CaptureCheckBox.Content = "Hide from Screen Capture";
            ShortcutText.Text = "Shortcuts: 'Alt+H' (Panel), 'Alt+L' (Light), 'Esc' (Exit)";
        }
        else
        {
            TitleText.Text = "Windows RingLight";
            LangBtn.Content = "EN";
            ExitBtn.Content = "Kapat";
            ColorText.Text = "Gelişmiş Renkler";
            BrightnessText.Text = "Parlaklık (Dimmer)";
            OpacityText.Text = "Işık Şiddeti (Matlık)"; 
            SoftnessText.Text = "Yumuşaklık (Işıltı)"; 
            ModesText.Text = "Işık Modu & Boyut";
            ModeFullBtn.Content = "Tam";
            ModeSidesBtn.Content = "Yanlar";
            ModeTopBtn.Content = "Üst";
            ModeRingBtn.Content = "Halka";
            ModeRectBtn.Content = "Çerçeve";
            if (ModeMatrixBtn != null) ModeMatrixBtn.Content = "Bölge";
            ThicknessText.Text = "Kalınlık";
            CornerText.Text = "Köşe Kavisi";
            if (MatrixRowsText != null) MatrixRowsText.Text = $"Satır: {(int)MatrixRowsSlider.Value}";
            MatrixColsText.Text = $"Sütun: {(int)MatrixColsSlider.Value}";
            if (MatrixBrushBtn != null) MatrixBrushBtn.Content = "Bölge Rengini Ata";
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
        if (MatrixGroup != null) MatrixGroup.Visibility = Visibility.Collapsed;

        switch (_currentMode)
        {
            case "Full":
                if (ThicknessGroup != null) ThicknessGroup.Visibility = Visibility.Collapsed;
                if (SoftnessGroup != null) SoftnessGroup.Visibility = Visibility.Collapsed;
                break;
            case "RoundedRect":
                if (CornerGroup != null) CornerGroup.Visibility = Visibility.Visible;
                break;
            case "Matrix":
                if (ThicknessGroup != null) ThicknessGroup.Visibility = Visibility.Collapsed;
                if (CornerGroup != null) CornerGroup.Visibility = Visibility.Collapsed;
                if (MatrixGroup != null) MatrixGroup.Visibility = Visibility.Visible;
                // SoftnessGroup stays visible for nice blur
                break;
        }
    }

    private void MatrixSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isInitialized || MatrixPreviewGrid == null) return;
        
        int rows = (int)MatrixRowsSlider.Value;
        int cols = (int)MatrixColsSlider.Value;
        
        if (MatrixRowsText != null) 
            MatrixRowsText.Text = _currentLang == "EN" ? $"Rows: {rows}" : $"Satır: {rows}";
        if (MatrixColsText != null) 
            MatrixColsText.Text = _currentLang == "EN" ? $"Cols: {cols}" : $"Sütun: {cols}";

        if (MatrixPreviewGrid.Rows != rows || MatrixPreviewGrid.Columns != cols)
        {
            MatrixPreviewGrid.Rows = rows;
            MatrixPreviewGrid.Columns = cols;
            BuildMatrixGrid(rows, cols);
            SyncLight();
        }
    }

    private void BuildMatrixGrid(int rows, int cols)
    {
        if (MatrixPreviewGrid == null) return;
        MatrixPreviewGrid.Children.Clear();
        int count = rows * cols;
        for (int i = 0; i < count; i++)
        {
            Button btn = new Button
            {
                Margin = new Thickness(1),
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Style = null // override defaults
            };
            btn.Click += MatrixBlock_Click;
            MatrixPreviewGrid.Children.Add(btn);
        }
    }

    private void MatrixBlock_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Tag != null)
            {
                btn.Tag = null;
                btn.Background = Brushes.Transparent;
            }
            else
            {
                Color c = _currentMatrixBrushColor.HasValue ? _currentMatrixBrushColor.Value : GetCurrentSliderColor();
                btn.Tag = c;
                btn.Background = new SolidColorBrush(c);
            }
            SyncLight();
        }
    }

    private void MatrixBrushBtn_Click(object sender, RoutedEventArgs e)
    {
        _currentMatrixBrushColor = GetCurrentSliderColor();
        MatrixBrushBtn.Background = new SolidColorBrush(_currentMatrixBrushColor.Value);
    }

    private void RgbModeCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (RgbModeCheckBox.IsChecked == true)
        {
            WhiteLightGrid.Visibility = Visibility.Collapsed;
            RgbPickerGrid.Visibility = Visibility.Visible;
            if (ColorThumbTransform != null && ColorMapGrid.ActualWidth > 0 && ColorMapGrid.ActualHeight > 0)
            {
                _currentSaturation = Math.Clamp((ColorThumbTransform.X + 6) / ColorMapGrid.ActualWidth, 0, 1);
                _currentValue = 1.0 - Math.Clamp((ColorThumbTransform.Y + 6) / ColorMapGrid.ActualHeight, 0, 1);
            }
        }
        else
        {
            WhiteLightGrid.Visibility = Visibility.Visible;
            RgbPickerGrid.Visibility = Visibility.Collapsed;
        }
        SyncLight();
    }

    private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isInitialized) return;
        Color baseHue = ColorFromHSV(HueSlider.Value, 1.0, 1.0);
        if (ColorMapHueLayer != null) ColorMapHueLayer.Background = new SolidColorBrush(baseHue);
        SyncLight();
    }

    private void ColorMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDraggingColorMap = true;
        ColorMapGrid.CaptureMouse();
        UpdateColorMapSelection(e.GetPosition(ColorMapGrid));
        e.Handled = true; // Prevent window from dragging
    }

    private void ColorMap_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingColorMap)
        {
            UpdateColorMapSelection(e.GetPosition(ColorMapGrid));
        }
    }

    private void ColorMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDraggingColorMap)
        {
            _isDraggingColorMap = false;
            ColorMapGrid.ReleaseMouseCapture();
        }
    }

    private void ColorMap_LostMouseCapture(object sender, MouseEventArgs e)
    {
        _isDraggingColorMap = false;
    }

    private void UpdateColorMapSelection(Point p)
    {
        double width = ColorMapGrid.ActualWidth;
        double height = ColorMapGrid.ActualHeight;

        if (width <= 0 || height <= 0) return;

        double x = Math.Clamp(p.X, 0, width);
        double y = Math.Clamp(p.Y, 0, height);

        if (ColorThumbTransform != null && ColorMapThumb != null)
        {
            ColorThumbTransform.X = x - (ColorMapThumb.Width / 2);
            ColorThumbTransform.Y = y - (ColorMapThumb.Height / 2);
        }

        _currentSaturation = x / width;
        _currentValue = 1.0 - (y / height);

        SyncLight();
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