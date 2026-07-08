using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>?????????????</summary>
  public static class EmbeddedResourceCache
  {
    private const int DefaultMaxEntries = 256;
    private const long DefaultMaxBytes = 32L * 1024 * 1024;
    private const long LargeFileThresholdBytes = 1024 * 1024;

    private static readonly ConcurrentDictionary<string, CacheEntry> Cache
        = new ConcurrentDictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

    private static int _maxEntries = DefaultMaxEntries;
    private static long _maxBytes = DefaultMaxBytes;
    private static long _currentBytes;

    /// <summary>??????????????? 256?</summary>
    public static int MaxEntries
    {
      get => _maxEntries;
      set => _maxEntries = value > 0 ? value : DefaultMaxEntries;
    }

    /// <summary>???????????????? 32 MB?</summary>
    public static long MaxBytes
    {
      get => _maxBytes;
      set => _maxBytes = value > 0 ? value : DefaultMaxBytes;
    }

    /// <summary>???????????????????? null?</summary>
    public static byte[] GetBytes(Assembly assembly, string resourceName)
    {
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));
      if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException(nameof(resourceName));

      var key = assembly.FullName + "|" + resourceName;
      if (Cache.TryGetValue(key, out var existing))
        return existing.Bytes;

      var bytes = ReadResourceBytes(assembly, resourceName);
      if (bytes == null)
        return null;

      if (bytes.LongLength > LargeFileThresholdBytes || bytes.LongLength > MaxBytes || Cache.Count >= MaxEntries)
        return bytes;

      var entry = new CacheEntry(bytes);
      if (Cache.TryAdd(key, entry))
        _currentBytes += bytes.LongLength;
      else if (Cache.TryGetValue(key, out existing))
        return existing.Bytes;

      TrimIfNeeded();
      return bytes;
    }

    /// <summary>???????????</summary>
    public static MemoryStream OpenMemoryStream(Assembly assembly, string resourceName)
    {
      var bytes = GetBytes(assembly, resourceName);
      return bytes == null ? null : new MemoryStream(bytes, writable: false);
    }

    /// <summary>????????????????????</summary>
    public static Stream OpenReadStream(Assembly assembly, string resourceName)
    {
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));
      if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException(nameof(resourceName));

      var actualName = assembly.GetManifestResourceNames()
          .FirstOrDefault(name => name.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
      if (actualName == null)
        return null;

      var stream = assembly.GetManifestResourceStream(actualName);
      if (stream == null)
        return null;

      if (stream.CanSeek && stream.Length <= LargeFileThresholdBytes)
      {
        using (stream)
        {
          var bytes = GetBytes(assembly, resourceName);
          return bytes == null ? null : new MemoryStream(bytes, writable: false);
        }
      }

      return stream;
    }

    /// <summary>???????????</summary>
    public static void Clear()
    {
      Cache.Clear();
      _currentBytes = 0;
    }

    private static void TrimIfNeeded()
    {
      while ((Cache.Count > MaxEntries || _currentBytes > MaxBytes) && !Cache.IsEmpty)
      {
        var oldest = Cache.OrderBy(pair => pair.Value.CreatedUtcTicks).FirstOrDefault();
        if (oldest.Key == null)
          break;

        if (Cache.TryRemove(oldest.Key, out var removed) && removed.Bytes != null)
          _currentBytes -= removed.Bytes.LongLength;
      }
    }

    private static byte[] ReadResourceBytes(Assembly assembly, string resourceName)
    {
      var actualName = assembly.GetManifestResourceNames()
          .FirstOrDefault(name => name.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

      if (actualName == null)
        return null;

      using (var stream = assembly.GetManifestResourceStream(actualName))
      {
        if (stream == null)
          return null;

        using (var memory = new MemoryStream())
        {
          stream.CopyTo(memory);
          return memory.ToArray();
        }
      }
    }

    private sealed class CacheEntry
    {
      public CacheEntry(byte[] bytes)
      {
        Bytes = bytes;
        CreatedUtcTicks = DateTime.UtcNow.Ticks;
      }

      public byte[] Bytes { get; }
      public long CreatedUtcTicks { get; }
    }
  }
}
