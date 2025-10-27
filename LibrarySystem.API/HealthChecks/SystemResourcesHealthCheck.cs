using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace LibrarySystem.API.HealthChecks;
public class SystemResourcesHealthCheck : IHealthCheck
{
    private const long UnhealthyMemoryThresholdBytes = 2L * 1024 * 1024 * 1024; // 2GB
    private const long DegradedMemoryThresholdBytes = 1L * 1024 * 1024 * 1024; // 1GB
    private const double UnhealthyCpuThreshold = 90.0;
    private const double DegradedCpuThreshold = 75.0;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get memory information
            Process currentProcess = Process.GetCurrentProcess();
            long memoryUsed = currentProcess.WorkingSet64;
            long totalMemory = GC.GetTotalMemory(forceFullCollection: false);

            // Get CPU usage (approximate)
            double cpuUsage = GetCpuUsage();

            // Get disk space information
            DriveInfo[] drives = DriveInfo.GetDrives();
            DriveInfo? systemDrive = drives.FirstOrDefault(d =>
                d.IsReady && d.DriveType == DriveType.Fixed &&
                d.RootDirectory.FullName == Path.GetPathRoot(Environment.SystemDirectory));

            long? availableDiskSpace = systemDrive?.AvailableFreeSpace;
            long? totalDiskSpace = systemDrive?.TotalSize;
            double? diskUsagePercent = totalDiskSpace.HasValue && totalDiskSpace.Value > 0
                ? (1 - (double)availableDiskSpace!.Value / totalDiskSpace.Value) * 100
                : null;

            var data = new Dictionary<string, object>
            {
                ["memory_used_mb"] = Math.Round(memoryUsed / 1024.0 / 1024.0, 2),
                ["gc_memory_mb"] = Math.Round(totalMemory / 1024.0 / 1024.0, 2),
                ["cpu_usage_percent"] = Math.Round(cpuUsage, 2),
                ["thread_count"] = currentProcess.Threads.Count,
                ["handle_count"] = currentProcess.HandleCount
            };

            if (diskUsagePercent.HasValue)
            {
                data["disk_usage_percent"] = Math.Round(diskUsagePercent.Value, 2);
                data["disk_available_gb"] = Math.Round(availableDiskSpace!.Value / 1024.0 / 1024.0 / 1024.0, 2);
            }

            // Determine health status
            if (memoryUsed > UnhealthyMemoryThresholdBytes || cpuUsage > UnhealthyCpuThreshold)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "System resources are critically low",
                    data: data));
            }

            if (memoryUsed > DegradedMemoryThresholdBytes || cpuUsage > DegradedCpuThreshold)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    "System resources are under pressure",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "System resources are healthy",
                data));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check system resources: Invalid operation",
                ex));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check system resources: Access denied",
                ex));
        }
        catch (IOException ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check system resources: I/O error",
                ex));
        }
    }

    private static double GetCpuUsage()
    {
        try
        {
            Process currentProcess = Process.GetCurrentProcess();
            DateTime startTime = DateTime.UtcNow;
            TimeSpan startCpuTime = currentProcess.TotalProcessorTime;

            // Wait a brief moment to measure CPU
            Thread.Sleep(500);

            TimeSpan endCpuTime = currentProcess.TotalProcessorTime;
            DateTime endTime = DateTime.UtcNow;

            double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
            double totalMsPassed = (endTime - startTime).TotalMilliseconds;
            double cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
        catch (InvalidOperationException)
        {
            // Process may have exited or invalid operation on process
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            // Access denied to process information
            return 0;
        }
    }
}