using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AspNetCoreDashboardLibrarySamples
{
  internal static class SampleJobStore
  {
    private static int _nextId = 1;
    private static readonly List<SampleJob> Jobs = new List<SampleJob>();
    private static readonly object Sync = new object();

    public static IReadOnlyList<SampleJob> List()
    {
      lock (Sync)
      {
        return Jobs.ToList();
      }
    }

    public static SampleJob Add(string name)
    {
      lock (Sync)
      {
        var job = new SampleJob
        {
          Id = Interlocked.Increment(ref _nextId),
          Name = string.IsNullOrWhiteSpace(name) ? "job" : name.Trim(),
          Status = "running",
          CreatedAt = DateTime.UtcNow
        };
        Jobs.Add(job);
        return job;
      }
    }

    public static bool Remove(int id)
    {
      lock (Sync)
      {
        var index = Jobs.FindIndex(job => job.Id == id);
        if (index < 0)
        {
          return false;
        }

        Jobs.RemoveAt(index);
        return true;
      }
    }

    public static string ExportCsv()
    {
      lock (Sync)
      {
        var builder = new StringBuilder();
        builder.AppendLine("id,name,status,createdAt");
        foreach (var job in Jobs)
        {
          builder.Append(job.Id).Append(',')
              .Append(Escape(job.Name)).Append(',')
              .Append(job.Status).Append(',')
              .AppendLine(job.CreatedAt.ToString("O"));
        }

        return builder.ToString();
      }
    }

    private static string Escape(string value)
    {
      return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }
  }

  internal sealed class SampleJob
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }
  }
}
