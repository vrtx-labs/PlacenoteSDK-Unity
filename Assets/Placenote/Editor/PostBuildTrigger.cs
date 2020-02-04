using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
#if UNITY_2017_2_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif

public class PostBuildTrigger : MonoBehaviour
{
    [PostProcessBuild(500)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
#if UNITY_IOS
        Debug.Log("Post Processing IOS Build...");
        //EmbedFrameworks
        var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        var targetGuid = proj.TargetGuidByName("Unity-iPhone");
        // EmbedFrameworks cannot be added in Unity 5.6.5

#if UNITY_2017_2_OR_NEWER
        const string defaultLocationInProj = "Plugins/iOS";
        const string coreFrameworkName = "Placenote.framework";
        var framework = Path.Combine(defaultLocationInProj, coreFrameworkName);
        var fileGuid = proj.AddFile(framework, "Frameworks/Placenote/" + framework, PBXSourceTree.Sdk);
        PBXProjectExtensions.AddFileToEmbedFrameworks(proj, targetGuid, fileGuid);
        proj.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks");
#endif

        proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        proj.WriteToFile(projPath);
        //EmbedFrameworks end

        // remove any .meta files from the project build directory which may reside their due to the inclusion of the framework inside the Unity project itself.
        DeleteMetaFilesInFramework(Path.Combine(pathToBuiltProject, "Frameworks/Placenote/", framework));

#endif
    }

    private static void DeleteMetaFilesInFramework(string path)
    {
        DirectoryInfo subDirectory = new DirectoryInfo(path);
        Debug.Log("Scanning Directory for .meta files: " + subDirectory.FullName);
        var metaFiles = subDirectory.GetFiles("*.meta", SearchOption.AllDirectories);
        foreach (var metaFile in metaFiles)
            metaFile.Delete();
        Debug.Log("Deleted " + metaFiles.Length + " .meta files from the build folder.");
    }
}