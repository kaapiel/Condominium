using UnityEngine;
using UnityEditor;

public class CustomUnityCli : MonoBehaviour
{
    /// <summary>
    /// Used to build asset bundle via command line invocation of Unity, like so:
    /// C:\Program Files\Unity\Hub\Editor\2019.4.5f1\Editor\Unity.exe -projectPath . -quit -batchmode -nographics -username "$UNITY_USERNAME" -password "$UNITY_PASSWORD" -serial $SERIAL_NUMBER -executeMethod CustomUnityCli.BuildAssetBundles -logFile /dev/stdout "$FILE_NAME"
    /// </summary>
    static void BuildAssetBundles()
    {

        string file_name = GetFileNameArg("-executeMethod");
        string platform_name = GetPlatformArg("-executeMethod");

        if (!platform_name.Equals("Android") || !platform_name.Equals("iOS"))
        {
            Debug.Log("Platform name doesn't match. It must be Android or iOS. Found: " + platform_name);
            return;
        }

        string file_without_extension = System.IO.Path.GetFileNameWithoutExtension(file_name);
        Debug.Log("External file found: " + file_without_extension);

        //Find prefab name and get its GUID
        Debug.Log("Finding GUID from asset name: " + file_without_extension);
        string guid = AssetDatabase.FindAssets(file_without_extension, null)[0];
        Debug.Log("Asset GUID found: " + guid);

        //Retrieve file path from GUID
        Debug.Log("Retrieving asset path from asset GUID");
        string asset_path = AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log("Asset path retrieved: " + asset_path);

        Debug.Log("Extracting textures...");
        if (!file_name.EndsWith(".unitypackage"))
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(asset_path) as ModelImporter;
            modelImporter.isReadable = true;
            modelImporter.ExtractTextures("Assets/Textures/");
        }
        Debug.Log("Extracting textures finished");

        //Import asset into project
        Debug.Log("External file import started...");
        AssetDatabase.ImportAsset(asset_path);
        Debug.Log("External file import finished!");

        //Set bundle name into prefab
        Debug.Log("Set asset bundle name and variant of " + file_without_extension);
        UnityEditor.AssetImporter.GetAtPath(asset_path).SetAssetBundleNameAndVariant(file_without_extension, "");
        Debug.Log("Asset bundle name and variant set!");

        //Build iOS/Android asset bundles
        if (platform_name.Equals("Android"))
        {
            Debug.Log("Building Android asset bundle with BundleOptions NONE");
            BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.Android);
            Debug.Log("Android asset bundle built with BundleOption NONE");
        } else if (platform_name.Equals("iOS"))
        {
            Debug.Log("Building iOS asset bundle with BundleOptions NONE");
            BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.iOS);
            Debug.Log("iOS asset bundle built with BundleOption NONE");
        }
        
    }

    // Helper function for getting the command line arguments
    private static string GetFileNameArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 4)
            {
                return args[i + 4];
            }
        }
        return null;
    }

    // Helper function for getting the command line arguments
    private static string GetPlatformArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 5)
            {
                return args[i + 5];
            }
        }
        return null;
    }
}