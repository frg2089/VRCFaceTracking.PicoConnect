using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace VRCFaceTracking.PicoConnect;
public partial class PicoConnectExtTrackingModule
{
    /// <summary>
    /// PICO 互联进程名
    /// </summary>
    private const string PICOConnect = "PICO Connect";
    /// <summary>
    /// PICO 互联可执行文件名
    /// </summary>
    private const string PICOConnectExe = $"{PICOConnect}.exe";
    /// <summary>
    /// PICO 互联注册表项
    /// </summary>
    private const string PICOConnectRegistryKey = "4c4786f6-9fbe-11ee-8c90-0242ac120002";


    /// <summary>
    /// 检查 PICO 互联
    /// </summary>
    /// <remarks>
    /// >= ^10.0.0 && < ^11.0.0
    /// </remarks>
    private void CheckPICOConnect()
    {
        // 检查PICO 互联是否已启动
        if (Process.GetProcessesByName(PICOConnect).Length is not 0)
        {
            _mode = PICOMode.PICOConnect;
            return;
        }

        LogPICOConnectProcessNotFound();

        string exePath = string.Empty;

        // 检查是否安装过PICO 互联
        if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") is RegistryKey uninstall
            && uninstall.OpenSubKey(PICOConnectRegistryKey) is RegistryKey registry
            && registry.GetValue("DisplayIcon") is string basePath)
            exePath = basePath.Split(',')[0];

        if (File.Exists(exePath))
        {
            // 已安装PICO 互联
            // 尝试启动PICO 互联

            if (StartProcess(exePath))
                _mode = PICOMode.PICOConnect;
            else
                LogPICOConnectCannotRun();
        }
        else
        {
            // 未安装PICO 互联
            LogPICOConnectNotInstalled();
        }
    }

    [LoggerMessage(1001, LogLevel.Information, $"\"{PICOConnect}\" are not installed.")]
    private partial void LogPICOConnectNotInstalled();

    [LoggerMessage(2001, LogLevel.Information, $"\"{PICOConnect}\" process was not found.")]
    private partial void LogPICOConnectProcessNotFound();

    [LoggerMessage(3001, LogLevel.Information, $"Cannot run \"{PICOConnect}\".")]
    private partial void LogPICOConnectCannotRun();
}
