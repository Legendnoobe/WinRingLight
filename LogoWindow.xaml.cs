using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WinRingLight;

public partial class LogoWindow : Window
{
    public Action OnLogoClick { get; set; }

    public LogoWindow()
    {
        InitializeComponent();
        this.Loaded += LogoWindow_Loaded;
    }

    private void LogoWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Absolute Path Loading (V20) to ensure visibility
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (File.Exists(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                LogoImage.Source = bitmap;
            }
        }
        catch { /* Fallback to XAML source if any */ }
    }

    private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        OnLogoClick?.Invoke();
    }
}
