using System.Diagnostics;

namespace Chase.PerformanceMonitor;
public class CPUMonitor
{
    private Process? _process = null;
    private bool systemWide = false;

    public CPUMonitor(Process process)
    {
        systemWide = false;
        _process = process;
    }
    public CPUMonitor(MonitorRestraint restraint)
    {
        if (!OperatingSystem.IsWindows() || restraint == MonitorRestraint.CurrentProcess)
        {
            _process = Process.GetCurrentProcess();
        }
        else
        {
            systemWide = true;
        }
    }

    public double Usage
    {
        get
        {
            if (systemWide)
            {
                if (OperatingSystem.IsWindows())
                {
                    PerformanceCounter cpuCounter = new()
                    {
                        CategoryName = "Processor",
                        CounterName = "% Processor Time",
                        InstanceName = "_Total"
                    };
                    // Get Current Cpu Usage
                    string currentCpuUsage = cpuCounter.NextValue() + "%";
                }
            }
            else
            {
                if (_process != null && !_process.HasExited)
                {
                    return Task.Run(async () =>
                    {
                        try
                        {
                            DateTime startTime = DateTime.UtcNow;
                            TimeSpan startCpuUsage = _process.TotalProcessorTime;
                            await Task.Delay(500);
                            if (_process == null || _process.HasExited)
                            {
                                return 0;
                            }

                            return Math.Round((_process.TotalProcessorTime - startCpuUsage).TotalMilliseconds / (Environment.ProcessorCount * (DateTime.UtcNow - startTime).TotalMilliseconds) * 100, 2);
                        }
                        catch { return 0; }
                    }).Result;
                }
            }

            return 0;
        }
    }
}