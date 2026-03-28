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

    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    private const uint WDA_EXCLUDEFROMCAPTURE = 0x11;

    public LightWindow()
    {
        InitializeComponent();
        this.SourceInitialized += LightWindow_SourceInitialized;
    }

    private void LightWindow_SourceInitialized(object sender, EventArgs e)
    {
        // Restore 100% Reliable Click-Through (V19)
        var helper = new WindowInteropHelper(this);
        int exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
        SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);

        // Permanent Fix for Black Screen (V25): Always exclude the maximized light surface from capture
        SetWindowDisplayAffinity(helper.Handle, WDA_EXCLUDEFROMCAPTURE);
    }

    public void UpdateLight(string mode, Color color, double thickness, double opacity, double softness, double cornerRadius)
    {
        FullMode.Visibility = (mode == "Full") ? Visibility.Visible : Visibility.Collapsed;
        SidesMode.Visibility = (mode == "Sides") ? Visibility.Visible : Visibility.Collapsed;
        TopMode.Visibility = (mode == "Top") ? Visibility.Visible : Visibility.Collapsed;
        RingMode.Visibility = (mode == "Ring") ? Visibility.Visible : Visibility.Collapsed;
        RoundedRectMode.Visibility = (mode == "RoundedRect") ? Visibility.Visible : Visibility.Collapsed;

        var brush = new SolidColorBrush(color);
        FullMode.Fill = brush;
        SideLeft.Fill = brush;
        SideRight.Fill = brush;
        TopMode.Fill = brush;
        RingMode.Fill = brush;
        RoundedRectMode.Fill = brush;

        LightContainer.Opacity = opacity;
        LightBlur.Radius = softness * 120;
        UpdateGeometries(mode, thickness, cornerRadius);
    }

    private void UpdateGeometries(string mode, double factor, double radiusFactor)
    {
        double width = this.ActualWidth;
        double height = this.ActualHeight;
        if (width == 0 || height == 0) return;

        double sideWidth = width * 0.45 * factor;
        double topHeight = height * 0.45 * factor;
        SideLeft.Width = sideWidth;
        SideRight.Width = sideWidth;
        TopMode.Height = topHeight;

        double minDim = Math.Min(width, height);
        double midRadius = minDim * 0.35;
        double ringThickness = minDim * 0.45 * factor;
        RingOuter.RadiusX = midRadius + (ringThickness * 0.8);
        RingOuter.RadiusY = midRadius + (ringThickness * 0.8);
        RingInner.RadiusX = Math.Max(0, midRadius - (ringThickness * 0.2));
        RingInner.RadiusY = Math.Max(0, midRadius - (ringThickness * 0.2));
        RingOuter.Center = new Point(width / 2, height / 2);
        RingInner.Center = new Point(width / 2, height / 2);

        double margin = (minDim * 0.45) * factor; 
        RR_Outer.Rect = new Rect(0, 0, width, height);
        RR_Inner.Rect = new Rect(margin, margin, Math.Max(1, width - 2 * margin), Math.Max(1, height - 2 * margin));
        double radius = (minDim * 0.4) * radiusFactor;
        RR_Inner.RadiusX = radius;
        RR_Inner.RadiusY = radius;
    }
}
