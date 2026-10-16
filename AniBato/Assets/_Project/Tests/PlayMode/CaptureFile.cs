using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AniBato.Tests
{
    internal static class CaptureFile
    {
        public static void WriteReport(string path,IEnumerable<string> lines)
        {
            string captureRoot=Environment.GetEnvironmentVariable("ANIBATO_CAPTURE_ROOT");
            if(!string.IsNullOrEmpty(captureRoot))
            {
                path=Path.Combine(captureRoot,Path.GetFileName(path));
                lines=lines.Select(line=>line.Replace("screenshots/",""));
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            Write(path,Encoding.UTF8.GetBytes(string.Join(Environment.NewLine,lines)+Environment.NewLine));
        }
        public static void Write(string path, byte[] bytes)
        {
            path=Path.GetFullPath(path);
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
