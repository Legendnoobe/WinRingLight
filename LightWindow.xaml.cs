using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WinRingLight;

public partial class LightWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public LightWindow()
    {
        InitializeComponent();
        this.SourceInitialized += LightWindow_SourceInitialized;
    }

    private void LightWindow_SourceInitialized(object sender, EventArgs e)
    {
        // Make the window click-through via Win32
        var helper = new WindowInteropHelper(this);
        int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
        SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    public void UpdateLight(string mode, Color color, double thickness, double opacity, double softness)
    {
        // Visibility
        FullMode.Visibility = (mode == "Full") ? Visibility.Visible : Visibility.Collapsed;
        SidesMode.Visibility = (mode == "Sides") ? Visibility.Visible : Visibility.Collapsed;
        TopMode.Visibility = (mode == "Top") ? Visibility.Visible : Visibility.Collapsed;
        RingMode.Visibility = (mode == "Ring") ? Visibility.Visible : Visibility.Collapsed;
        RoundedRectMode.Visibility = (mode == "RoundedRect") ? Visibility.Visible : Visibility.Collapsed;

        // Color
        var brush = new SolidColorBrush(color);
        FullMode.Fill = brush;
        SideLeft.Fill = brush;
        SideRight.Fill = brush;
        TopMode.Fill = brush;
        RingMode.Fill = brush;
        RoundedRectMode.Fill = brush;

        // Opacity
        LightContainer.Opacity = opacity;
        
        // Blur
        LightBlur.Radius = softness * 120;

        // Geometry
        UpdateGeometries(mode, thickness);
    }

    private void UpdateGeometries(string mode, double factor)
    {
        double width = this.ActualWidth;
        double height = this.ActualHeight;
        if (width == 0 || height == 0) return;

        // Bars
        double sideWidth = width * 0.45 * factor;
        double topHeight = height * 0.45 * factor;
        SideLeft.Width = sideWidth;
        SideRight.Width = sideWidth;
        TopMode.Height = topHeight;

        // Ring
        double minDim = Math.Min(width, height);
        double midRadius = minDim * 0.35;
        double ringThickness = minDim * 0.45 * factor;
        // Asymmetric Growth (V15): 80% Outward, 20% Inward
        RingOuter.RadiusX = midRadius + (ringThickness * 0.8);
        RingOuter.RadiusY = midRadius + (ringThickness * 0.8);
        RingInner.RadiusX = Math.Max(0, midRadius - (ringThickness * 0.2));
        RingInner.RadiusY = Math.Max(0, midRadius - (ringThickness * 0.2));
        RingOuter.Center = new Point(width / 2, height / 2);
        RingInner.Center = new Point(width / 2, height / 2);

        // Rounded Rect
        double margin = (width * 0.45) * (1.1 - factor); 
        RR_Outer.Rect = new Rect(0, 0, width, height);
        RR_Inner.Rect = new Rect(margin, margin, Math.Max(1, width - 2 * margin), Math.Max(1, height - 2 * margin));
        // Radius is hardcoded logic for now, or could be passed. I'll stick to 0.2 base for now.
        RR_Inner.RadiusX = minDim * 0.1; 
        RR_Inner.RadiusY = minDim * 0.1;
    }
}
