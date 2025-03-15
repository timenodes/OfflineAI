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
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",          
                Arguments = $"/C {command}",    
                Verb = "runas",   
                UseShellExecute = true,        
                CreateNoWindow = false,
            };
            try
            {
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                process.Close();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误: {ex.Message}");// 其他异常处理
                return false;
            }
        }

        /// <summary>
        /// 执行CMD指令:不创建窗体启用Shell执行指定命令
        /// </summary>
        public static bool ExecuteCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = true,        
                CreateNoWindow = false,         
            };
            try
            {
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();   
                process.Close();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 通过端口号关闭进程（管理员参数：Verb = "runas"）：
        ///     1、通过netstat命令获取端口占用的进程ID
        ///     2、使用taskkill命令终止进程
        /// </summary>
        public static void CloseProcessByPort(int port)
        {
            int pid = GetPidByPort(port);
            try
            {
                if (pid != -1)
                {
                    var processStartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c taskkill /PID {pid} /F",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    };
                    Process process = Process.Start(processStartInfo);
                    process.WaitForExit();
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
        public static int GetPidByPort(int port)
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
