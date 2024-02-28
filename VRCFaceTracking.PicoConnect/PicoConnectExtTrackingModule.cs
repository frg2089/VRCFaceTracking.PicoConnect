using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using VRCFaceTracking.Core.Library;

namespace VRCFaceTracking.PicoConnect;

[SupportedOSPlatform("windows")]
public sealed partial class PicoConnectExtTrackingModule : ExtTrackingModule
{
    private PICOMode _mode;
    private UdpClient? _client;
    private bool _eyeAvailable;
    private bool _expressionAvailable;
    private PxrFTInfo _data;
    private CancellationTokenSource? _cancellation;
    private int _faceTrackingTransferProtocol = 2;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        (_eyeAvailable, _expressionAvailable) = (eyeAvailable, expressionAvailable);
        if (eyeAvailable is false && expressionAvailable is false)
        {
            // 未启用追踪，初始化失败
            LogNotAvailable();
            return (false, false);
        }

        // 检查环境
        CheckPICOConnect();
        if (_mode is PICOMode.Disabled)
            CheckStreamingAssistant();
        if (_mode is PICOMode.Disabled)
        {
            // 找不到 PICO 互联 或 串流助手，初始化失败
            LogNoStreamingTool();
            return (false, false);
        }

        _cancellation = new();
        int retry = 0;
        while (!Initialize())
        {
            if (retry >= 3)
                return (false, false);

            retry++;
            KillPicoEtFtBtBridge();

            Task.Delay(retry * 1000).Wait();
        }

        ModuleInformation.Name = "Pico 4 Pro / Enterprise";

        if (typeof(PicoConnectExtTrackingModule).Assembly.GetManifestResourceStream("hmd") is Stream stream)
            ModuleInformation.StaticImages.Insert(0, stream);

        if (!eyeAvailable)
            LogDisabledEye();
        if (!expressionAvailable)
            LogDisabledExpression();

        return (eyeAvailable, expressionAvailable);
    }

    public override void Teardown()
    {
        _cancellation?.Cancel();
        _cancellation = null;
        _client?.Dispose();
        _client = null;
    }

    public override async void Update()
    {
        if (Status is not ModuleState.Active || _client is null || _cancellation is null)
        {
            Thread.Sleep(100);
            return;
        }

        try
        {
            var result = await _client.ReceiveAsync(_cancellation.Token).ConfigureAwait(false);
            if (ParsePxrData(result.Buffer))
                UpdateFromPxrFTInfo();
        }
        catch (SocketException ex) when (ex.ErrorCode is 10060)
        {
            LogSocketException(ex);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private bool Initialize()
    {
        try
        {
            ushort port = _faceTrackingTransferProtocol switch
            {
                1 => 29763,
                2 => 29765,
                _ => 29765,
            };

            _client = new(port);
            _client.Client.ReceiveTimeout = 5000;
            LogInfo(port, _client.Client.ReceiveTimeout, _faceTrackingTransferProtocol);

            return true;
        }
        catch (SocketException ex) when (ex.ErrorCode is 10048)
        {
            return false;
        }
    }

    /// <summary>
    /// Magic<br/>
    /// Close the pico_et_ft_bt_bridge.exe process and reinitialize it.<br/>
    /// It will listen to UDP port 29763 before pico_et_ft_bt_bridge.exe runs.<br/>
    /// Note: exclusively to simplify older versions of the FT bridge,<br/>
    /// the bridge now works without any need for process killing.
    /// </summary>
    private void KillPicoEtFtBtBridge()
    {
        Process proc = new()
        {
            StartInfo =
            {
                FileName = "taskkill.exe",
                ArgumentList = {
                    "/f",
                    "/t",
                    "/im",
                    "pico_et_ft_bt_bridge.exe"
                },
                CreateNoWindow = true
            }
        };
        proc.Start();
        proc.WaitForExit();
    }

    private static bool StartProcess(string exePath)
    {
        Process proc = new()
        {
            StartInfo =
                {
                    FileName = exePath,
                    UseShellExecute = true,
                }
        };

        return proc.Start();
    }


    [LoggerMessage(-1, LogLevel.Warning, "An uncaught exception occurred.")]
    private partial void LogException(Exception exception);

    [LoggerMessage(0, LogLevel.Information, "Skip the initialization as it is not available now.")]
    private partial void LogNotAvailable();

    [LoggerMessage(1, LogLevel.Information, "Skip the initialization because can't found the streaming tool.")]
    private partial void LogNoStreamingTool();

    [LoggerMessage(2, LogLevel.Information, "Host end-point: {port}.\r\nInitialization Timeout: {timeout}ms.\r\nFaceTrackingTransferProtocol Version: {faceTrackingTransferProtocol}")]
    private partial void LogInfo(ushort port, int timeout, int faceTrackingTransferProtocol);

    [LoggerMessage(3, LogLevel.Information, "Eye tracking already in use, disabling eye data.")]
    private partial void LogDisabledEye();

    [LoggerMessage(4, LogLevel.Information, "Expression Tracking already in use, disabling expression data.")]
    private partial void LogDisabledExpression();
    [LoggerMessage(5, LogLevel.Information, "Data was not sent within the timeout.")]
    private partial void LogSocketException(SocketException exception);

}
