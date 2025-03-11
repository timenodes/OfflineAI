using System.ComponentModel;
using System.Diagnostics;

namespace OfflineAI.Services
{
    /// <summary>
    /// 进程服务：
    ///     1、获取指定进程名称的进程ID
    ///     2、执行CMD命令
    ///     3、执行CMD命令(以管理员身份运行)
    ///     4、根据端口获取进程PID并释放与此有关的所有资源
    ///     5、根据端口获取PID
    /// </summary>
    public class ProcessService
    {
        /// <summary>
        /// 获取进程ID
        /// </summary>
        public static int GetProcessId(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    Debug.Print($"进程名称: {process.ProcessName}, 进程ID: {process.Id}");
                }
            }
            else
            {
                Debug.Print($"未找到名为 {processName} 的进程。");
            }
            return 0;
        }

        /// <summary>
        /// 以管理员身份运行CMD命令
        /// </summary>
        public static bool ExecuteCommandAsAdmin(string command)
        {
            // 创建一个新的进程启动信息
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",           // 设置要启动的程序为cmd.exe
                Arguments = $"/C {command}",    // 设置要执行的命令
                Verb = "runas",                 // 以管理员身份运行
                UseShellExecute = true,         // 使用操作系统shell启动进程
                CreateNoWindow = false,
            };
            try
            {
                Process process = Process.Start(processStartInfo);// 启动进程
                process.Close();          // 返回是否成功执行
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误: {ex.Message}");// 其他异常处理
                return false;
            }
        }

        /// <summary>
        /// 执行CMD指令
        /// </summary>
        public static bool ExecuteCommand(string command)
        {
            // 创建一个新的进程启动信息
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",           // 设置要启动的程序为cmd.exe
                Arguments = $"/C {command}",    // 设置要执行的命令
                UseShellExecute = true,         // 使用操作系统shell启动进程
                CreateNoWindow = false,         //不创建窗体
            };
            try
            {
                Process process = Process.Start(processStartInfo);// 启动进程
                process.WaitForExit();    // 等待进程退出
                process.Close();          // 返回是否成功执行
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误: {ex.Message}");// 其他异常处理
                return false;
            }
        }

        /// <summary>
        /// 根据端口获取进程PID并释放与此有关的所有资源
        /// </summary>
        public static void GetPIDAndCloseByPort(int port)
        {
            int pid = GetProcessIDByPort(port);
            try
            {
                if (pid != -1)
                {
                    var processStartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c taskkill /PID {pid} /F", // 使用taskkill命令关闭指定PID的进程
                        //Verb = "runas",                           // 以管理员权限运行
                        UseShellExecute = true,                   // 使用shell执行
                        CreateNoWindow = true                     // 不创建新窗口
                    };
                    Process process = Process.Start(processStartInfo);
                    process.Close();
                    process.WaitForExit(2000);
                    Debug.WriteLine($"成功终止了进程ID为 {pid} 的进程.");
                }
                else
                {
                    Debug.WriteLine($"没有找到占用端口 {port} 的进程.");
                }
            }
            catch (ArgumentException)
            {
                Debug.WriteLine($"进程ID {pid} 不存在或已经被终止.");
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
            {
                Debug.WriteLine($"无法终止进程 {pid}: 拒绝访问。请确保你有足够权限或尝试手动终止该进程。");
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine($"无法终止进程 {pid}. 进程可能已经被终止或正在终止.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生未知错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据端口获取PID
        /// </summary>
        public static int GetProcessIDByPort(int port)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "netstat";
                process.StartInfo.Arguments = "-ano";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(1000);
                //剔除"\r\n"组合，剔除连续"\r\n"避免为空
                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (line.Contains($":{port} "))
                    {
                        string[] parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 4 && int.TryParse(parts[4], out int pid))
                        {
                            Console.WriteLine($"Process ID for port {port}: {pid}");
                            if (pid != 0) return pid;
                        }
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return -1;
            }
        }
    }
}
