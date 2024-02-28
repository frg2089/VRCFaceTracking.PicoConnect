using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace VRCFaceTracking.PicoConnect;
public partial class PicoConnectExtTrackingModule
{
    /// <summary>
    /// 串流助手进程名
    /// </summary>
    private const string StreamingAssistant = "Streaming Assistant";
    /// <summary>
    /// 串流助手可执行文件名
    /// </summary>
    private const string StreamingAssistantExe = $"{StreamingAssistant}.exe";
    /// <summary>
    /// 串流助手注册表项
    /// </summary>
    private const string StreamingAssistantRegistryKey = "{3C15F822-6BED-41E4-A3B6-13E3069726E3}_is1";


    /// <summary>
    /// 检查 PICO 串流助手
    /// </summary>
    /// <remarks>
    /// >= ^9.4.15.2 && < ^10.0.0
    /// <br/>
    /// Note: 最低的支持眼追的串流助手的版本号尚不明确<br/>
    /// Note: 不同版本用的端口好像也不一样<br/>
    /// </remarks>
    private void CheckStreamingAssistant()
    {
        // 检查串流助手是否已启动
        if (Process.GetProcessesByName(StreamingAssistant).Length is not 0)
        {
            _mode = PICOMode.StreamingAssistant;
            return;
        }

        LogStreamingAssistantProcessNotFound();

        string exePath = string.Empty;

        // 检查是否安装过串流助手
        if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall") is RegistryKey uninstall
            && uninstall.OpenSubKey(StreamingAssistantRegistryKey) is RegistryKey registry
            && registry.GetValue("InstallLocation") is string basePath)
        {
            exePath = Path.Combine(basePath, StreamingAssistantExe);
            var conf = Path.Combine(basePath, "driver", "bin", "win64", "Tracking.ini");
            if (File.Exists(conf))
            {
                var data = File.ReadAllLines(conf).FirstOrDefault(i => i.StartsWith("driverport"))?.Split('=');
                if (data is not null and { Length: > 1 } && ushort.TryParse(data[1], out var port))
                    _port = port;
            }
        }

        if (File.Exists(exePath))
        {
            // 已安装串流助手
            // 尝试启动串流助手

            if (StartProcess(exePath))
                _mode = PICOMode.StreamingAssistant;
            else
                LogPICOConnectCannotRun();
        }
        else
        {
            // 未安装串流助手
            LogStreamingAssistantNotInstalled();
        }
    }

    [LoggerMessage(1000, LogLevel.Information, $"\"{StreamingAssistant}\" are not installed.")]
    private partial void LogStreamingAssistantNotInstalled();

    [LoggerMessage(2000, LogLevel.Information, $"\"{StreamingAssistant}\" process was not found.")]
    private partial void LogStreamingAssistantProcessNotFound();

    [LoggerMessage(3000, LogLevel.Information, $"Cannot run \"{StreamingAssistant}\".")]
    private partial void LogStreamingAssistantCannotRun();

}
