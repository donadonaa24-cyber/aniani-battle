using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AniBato.EditorTools
{
    // A report of shipped dependencies, not the contents of the source-art archive.
    public sealed class BuildSizeReport : IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;
        public void OnPostprocessBuild(BuildReport report)
        {
            string folder = Path.GetFullPath("../TestResults");
            Directory.CreateDirectory(folder);
            var entries = report.packedAssets.SelectMany(p => p.contents)
                .Where(p => !string.IsNullOrEmpty(p.sourceAssetPath))
                .GroupBy(p => p.sourceAssetPath)
                .Select(g => new Entry { path = g.Key.Replace('\\','/'), bytes = g.Sum(p => (long)p.packedSize) })
                .OrderByDescending(e => e.bytes).ToArray();
            var data = new Result { platform = report.summary.platform.ToString(), bytes = (long)report.summary.totalSize,
                seconds = report.summary.totalTime.TotalSeconds, options = report.summary.options.ToString(), entries = entries };
            File.WriteAllText(Path.Combine(folder,"size_" + report.summary.platform + ".json"), JsonUtility.ToJson(data,true));
            Debug.Log("SIZE_REPORT " + data.platform + " " + data.bytes + " bytes; " + entries.Length + " dependencies");
        }
        [Serializable] private sealed class Entry { public string path; public long bytes; }
        [Serializable] private sealed class Result { public string platform, options; public long bytes; public double seconds; public Entry[] entries; }
    }
}
