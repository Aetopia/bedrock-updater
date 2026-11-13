using System;
using System.Linq;
using System.Threading;
using System.Windows;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;

namespace Bedrock.Updater;

static class Program
{
    const string Caption = "Bedrock Updater: Error";

    static Program() => AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
    {
        var exception = (Exception)args.ExceptionObject;
        while (exception.InnerException is { }) exception = exception.InnerException;

        if (Application.Current?.MainWindow is not { } owner)
            MessageBox.Show(exception.Message, Caption, OK, Error);
        else
            MessageBox.Show(owner, exception.Message, Caption, OK, Error);

        Environment.Exit(1);
    };

    [STAThread]
    static void Main(string[] args)
    {
        using Mutex mutex = new(false, "C7B58EAD-356C-40A1-A145-7262C3C04D00", out var value);
        if (!value) return; value = args.Any(_ => _.Equals("/preview", StringComparison.OrdinalIgnoreCase));
        new Application { ShutdownMode = ShutdownMode.OnMainWindowClose }.Run(new Window(value));
    }
}