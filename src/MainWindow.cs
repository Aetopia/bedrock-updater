using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Bedrock.Updater.System;
using Bedrock.Updater.Windows;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static System.Windows.Media.Imaging.BitmapCacheOption;
using static System.Windows.Media.Imaging.BitmapCreateOptions;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;

namespace Bedrock.Updater;

sealed class MainWindow : Window, IProgress<AppInstallStatus>
{
    readonly AppMetadata[] _apps;
    readonly TextBlock _textBlock1 = new();
    readonly TextBlock _textBlock2 = new() { Text = $"{nameof(Pending)}..." };
    readonly ProgressBar _progressBar = new() { Width = 359, Height = 23, IsIndeterminate = true };

    InstallRequest<MainWindow>? _request;

    internal MainWindow(bool value)
    {
        _textBlock1.Text = $"Updating {(value ? "Preview" : "Release")}...";
        _apps = [GamingServices._, value ? MinecraftWindowsBeta._ : MinecraftUWP._];

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Application.ico");

        Title = "Bedrock Updater";
        Icon = BitmapFrame.Create(stream, PreservePixelFormat, OnLoad);

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Canvas canvas = new()
        {
            Width = 381,
            Height = 115
        };

        canvas.Children.Add(_textBlock1);
        canvas.Children.Add(_textBlock2);
        canvas.Children.Add(_progressBar);

        Canvas.SetLeft(_textBlock1, 11);
        Canvas.SetLeft(_textBlock2, 11);
        Canvas.SetLeft(_progressBar, 11);

        Canvas.SetTop(_textBlock1, 15);
        Canvas.SetTop(_textBlock2, 84);
        Canvas.SetTop(_progressBar, 46);

        Content = canvas;
    }

    protected override void OnClosing(CancelEventArgs args)
    {
        base.OnClosing(args);

        if (_request?.Cancel() ?? false)
        {
            args.Cancel = true;
            _textBlock2.Text = $"{nameof(Pending)}...";

            _progressBar.Value = 0;
            _progressBar.IsIndeterminate = true;
        }
    }

    protected override async void OnContentRendered(EventArgs args)
    {
        base.OnContentRendered(args);

        foreach (var app in _apps)
        {
            _request = await app.InstallAsync(this); try
            {
                if (_request is { } request)
                    if (!await request) break;

                _progressBar.Value = 0;
                _progressBar.IsIndeterminate = true;

                _textBlock2.Text = $"{nameof(Pending)}...";
            }
            finally
            {
                _request = null;
            }
        }

        Close();
    }

    public void Report(AppInstallStatus args) => Dispatcher.Invoke(() =>
    {
        switch (args.InstallState)
        {
            case Pending:
                _textBlock2.Text = $"{nameof(Pending)}...";
                break;

            case Installing:
                _textBlock2.Text = $"{nameof(Installing)}...";
                break;

            case Downloading:
                var current = args.BytesDownloaded;
                var total = args.DownloadSizeInBytes;
                _textBlock2.Text = $"{nameof(Downloading)}... {current.AsBytes()} / {total.AsBytes()}";
                break;
        }

        if (_progressBar.Value != args.PercentComplete)
        {
            _progressBar.IsIndeterminate = args.InstallState is Pending;
            _progressBar.Value = _progressBar.IsIndeterminate ? 0 : args.PercentComplete;
        }
    });
}