using System;
using System.IO;
using System.Threading;

namespace AniBato.Tests
{
    internal static class CaptureFile
    {
        public static void Write(string path, byte[] bytes)
        {
            string captureRoot=Environment.GetEnvironmentVariable("ANIBATO_CAPTURE_ROOT");
            if(!string.IsNullOrEmpty(captureRoot))
            {
                string marker=Path.DirectorySeparatorChar+"screenshots"+Path.DirectorySeparatorChar;
                int at=path.IndexOf(marker,StringComparison.OrdinalIgnoreCase);
                if(at>=0){path=Path.Combine(captureRoot,path.Substring(at+marker.Length));Directory.CreateDirectory(Path.GetDirectoryName(path));}
            }
            // Previewers/sync tools may keep an old PNG mapped on Windows. Identical captures need no truncation.
            if (File.Exists(path) && bytes.AsSpan().SequenceEqual(File.ReadAllBytes(path))) return;
            for (int attempt = 0; ; attempt++)
            {
                try { File.WriteAllBytes(path, bytes); return; }
                catch (IOException) when (attempt < 9) { Thread.Sleep(100); }
            }
        }
    }
}
