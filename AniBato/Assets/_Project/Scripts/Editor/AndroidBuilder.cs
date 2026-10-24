using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class AndroidBuilder
    {
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("-buildTarget Android が必要です。");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.aniani.anibato");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.Android.useCustomKeystore = false; // Local trial APK; never read or create a private release signing credential.
            AppIconBuilder.ConfigureAndroid();
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
            WebGLSpritePacking.PrepareAndroid(); AssetDatabase.SaveAssets();
            const string output = "../Build/Android/AniBato-arm64.apk";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName = output, target = BuildTarget.Android, options = BuildOptions.DetailedBuildReport
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Androidビルド失敗: "+report.summary.result);
            File.Copy("../art_source/fonts/OFL-NotoCJK.txt", "../Build/Android/ThirdPartyNotices.txt", true);
            var result = new Result { result = "success", bytes = new FileInfo(output).Length, seconds = report.summary.totalTime.TotalSeconds, abi = "arm64-v8a", minimumApi = 26, package = "com.aniani.anibato", releaseSigned = false };
            string json = JsonUtility.ToJson(result, true);
            File.WriteAllText("../TestResults/android-build.json", json); Debug.Log("ANDROID_REPORT " + json);
        }
        [Serializable] private sealed class Result { public string result, abi, package; public long bytes; public double seconds; public int minimumApi; public bool releaseSigned; }
    }
}
