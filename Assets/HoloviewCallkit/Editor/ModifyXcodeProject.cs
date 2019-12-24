using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

using System.Text.RegularExpressions;
using System.Collections.Generic;

public class ModifyXcodeProject : MonoBehaviour
{
    internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
            Directory.Delete(dstPath);
        if (File.Exists(dstPath))
            File.Delete(dstPath);

        Directory.CreateDirectory(dstPath);

        foreach (var file in Directory.GetFiles(srcPath))
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));

        foreach (var dir in Directory.GetDirectories(srcPath))
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
    }
    private static void AddLibToProject(PBXProject inst, string lib)
    {
        string target = inst.TargetGuidByName(PBXProject.GetUnityTargetName());

        string fileGuid = inst.AddFile("usr/lib/" + lib, "Frameworks/" + lib, PBXSourceTree.Sdk);
        inst.AddFileToBuild(target, fileGuid);
    }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        print(path);
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            print(projPath);

            PBXProject proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");

            // 添加系统framework
            proj.AddFrameworkToProject(target, "Security.framework", false);
            proj.AddFrameworkToProject(target, "Foundation.framework", false);
            proj.AddFrameworkToProject(target, "CFNetwork.framework", false);
            proj.AddFrameworkToProject(target, "libicucore.tbd", false);
            proj.AddFrameworkToProject(target, "libstdc++.tbd", false);


            proj.SetBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");
            proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks/Plugins/iOS");
            //proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks/ShowNowForUnity/Plugins/iOS");

            //关闭bit code
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");



            string basePath = path + "/Frameworks/";
            string frameworkPath = "HoloviewCallKit/Plugins/iOS/";
            string[] arrFrameworks = { "WebRTC.framework" };
            foreach (string framework in arrFrameworks)
            {
                AddEmbeddedFramework(ref proj, target, basePath + frameworkPath + framework, framework);
            }

            File.WriteAllText(projPath, proj.WriteToString());

            //			foreach(string framework in arrFrameworks) {
            //				string contents = File.ReadAllText(projPath);
            //				string pattern = "(?<=Embed Frameworks)(?:.*)(\\/\\* " + framework + "\\ \\*\\/)(?=; };)";
            //				string oldText = "/* " + framework + " */";
            //				string updatedText = "/* " + framework + " */; settings = {ATTRIBUTES = (CodeSignOnCopy, ); }";
            //				contents = Regex.Replace(contents, pattern, m => m.Value.Replace(oldText, updatedText));
            //				File.WriteAllText(projPath, contents);
            //			}






            //plist 添加权限
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;
            rootDict.SetString("NSCameraUsageDescription", "");
            rootDict.SetString("NSMicrophoneUsageDescription", "");

            //写入
            plist.WriteToFile(plistPath);


            //			// ファイルを追加
            //			var fileName = "my_file.xml";
            //			var filePath = Path.Combine("Assets/Lib", fileName);
            //			File.Copy(filePath, Path.Combine(path, fileName));
            //			proj.AddFileToBuild(target, proj.AddFile(fileName, fileName, PBXSourceTree.Source));
            //
            //			// Yosemiteでipaが書き出せないエラーに対応するための設定
            //			proj.SetBuildProperty(target, "CODE_SIGN_RESOURCE_RULES_PATH", "$(SDKROOT)/ResourceRules.plist");


            //			

        }
    }

    public static void AddEmbeddedFramework(ref PBXProject project, string target, string frameworkPath, string frameworkName)
    {
        string fileGuid = project.AddFile(frameworkPath, "Frameworks/" + frameworkName, PBXSourceTree.Source);
        string embedPhase = project.AddCopyFilesBuildPhase(target, "Embed Frameworks", "", "10");
        project.AddFileToBuildSection(target, embedPhase, fileGuid);
        PBXProjectExtensions.AddFileToEmbedFrameworks(project, target, fileGuid);
        project.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
        project.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", "$(SRCROOT)/Frameworks/HoloviewCallKit/Plugins/iOS/");
    }
}
