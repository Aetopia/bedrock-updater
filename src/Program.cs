using System;
using System.Linq;
using System.Threading;
using System.Windows;
using static System.StringComparison;
using static System.Windows.MessageBoxButton;
using static System.Windows.MessageBoxImage;
using static System.Windows.ShutdownMode;

namespace Bedrock.Updater;

static class Program
{
    const string Caption = "Bedrock Updater: Error";
    const string Name = "C7B58EAD-356C-40A1-A145-7262C3C04D00";

    static Program() => AppDomain.CurrentDomain.UnhandledException += static (sender, args) =>
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
        using Mutex mutex = new(false, Name, out var created); if (created)
        {
            var value = args.Any(static _ => _.Equals("/preview", OrdinalIgnoreCase));
            new Application { ShutdownMode = OnMainWindowClose }.Run(new Window(value));
        }
    }
}