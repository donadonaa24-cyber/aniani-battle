using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--phase6-tables") return PhaseSixReport.Write();
        if (args.Length > 0 && args[0] == "--phase6b-camera") return PhaseSixBCameraReport.Write();
        if (args.Length > 0 && args[0] == "--cpu-report") return CpuReport.Write(args.Length > 1 ? args[1] : "TestResults/phase5-cpu");
        if (args.Length > 0 && args[0] == "--cpu-report-before") return CpuReport.Write(args.Length > 1 ? args[1] : "TestResults/phase5-cpu-before", true);
        if (args.Length > 0 && args[0] == "--cpu-trace") return CpuReport.Trace(int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]), args.Length > 4 ? int.Parse(args[4]) : 2);
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(Assembly.GetExecutingAssembly(), new Dictionary<string, object>());
        var result = runner.Run(TestListener.NULL, TestFilter.Empty);
        string path = args.Length > 0 ? args[0] : "TestResults/dotnet-battle.xml";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        File.WriteAllText(path, result.ToXml(true).OuterXml);
        Console.WriteLine("純C#代替検証: " + result.PassCount + " 成功 / " + result.FailCount + " 失敗 / " + result.SkipCount + " スキップ");
        PrintFailures(result);
        Console.WriteLine("Unityでの実行結果ではありません。結果: " + Path.GetFullPath(path));
        return result.FailCount == 0 ? 0 : 1;
    }
    private static void PrintFailures(NUnit.Framework.Interfaces.ITestResult result)
    {
        if (!result.HasChildren && result.ResultState.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            Console.WriteLine(result.FullName + "\n" + result.Message + "\n" + result.StackTrace);
        foreach (var child in result.Children) PrintFailures(child);
    }
}
