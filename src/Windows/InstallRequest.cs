using System;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using Bedrock.Updater.System;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using static System.Threading.Tasks.TaskContinuationOptions;
using static Windows.ApplicationModel.Store.Preview.InstallControl.AppInstallState;

namespace Bedrock.Updater.Windows;

sealed class InstallRequest<T> where T : IProgress<AppInstallStatus>
{
    readonly T _progress;
    readonly AppInstallItem _item;
    readonly TaskCompletionSource<bool> _tcs;

    internal InstallRequest(AppInstallItem item, T progress)
    {
        _item = item;
        _progress = progress;

        _tcs = new();
        _tcs.Task.ContinueWith(OnFaulted, OnlyOnFaulted | ExecuteSynchronously);

        _item.Completed += OnCompleted;
        _item.StatusChanged += OnStatusChanged;
    }

    internal TaskAwaiter<bool> GetAwaiter()
    {
        return _tcs.Task.GetAwaiter();
    }

    internal bool Cancel()
    {
        if (_tcs.Task.IsCompleted)
            return false;

        if (_item.IsInstalled)
            _item.Pause();
        else
            _item.Cancel();

        return true;
    }

    void OnFaulted(Task<bool> task)
    {
        try { _item.Cancel(); } catch { }
    }

    void OnCompleted(AppInstallItem sender, object args)
    {
        switch (sender.GetCurrentStatus().InstallState)
        {
            case Completed:
                _tcs.TrySetResult(true);
                break;

            case Canceled:
                if (!_tcs.Task.IsFaulted)
                    _tcs.TrySetResult(false);
                break;
        }
    }

    void OnStatusChanged(AppInstallItem sender, object args)
    {
        var status = sender.GetCurrentStatus();
        switch (status.InstallState)
        {
            case Error:
                _tcs.TrySetException(status.ErrorCode);
                break;

            case Paused:
                _tcs.TrySetResult(false);
                break;

            case Pending:
            case Installing:
            case Downloading:
                _progress.Report(status);
                break;
        }
    }
}