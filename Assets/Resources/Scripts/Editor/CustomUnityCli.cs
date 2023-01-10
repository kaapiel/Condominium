using UnityEngine;
using UnityEditor;
using System.IO.Compression;
using System.IO;

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

        if (!platform_name.Equals("Android") && !platform_name.Equals("iOS"))
        {
            Debug.Log("Platform name doesn't match. It must be Android or iOS. Platform provided: " + platform_name);
            return;
        }

        string file_without_extension = Path.GetFileNameWithoutExtension(file_name);
        Debug.Log("External file found: " + file_without_extension);

        //Find prefab name and get its GUID
        Debug.Log("Finding GUID from asset name: " + file_without_extension);
        string guid = AssetDatabase.FindAssets(file_without_extension, null)[0];
        Debug.Log("Asset GUID found: " + guid);

        //Retrieve file path from GUID
        Debug.Log("Retrieving asset path from asset GUID");
        string asset_path = AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log("Asset path retrieved: " + asset_path);

        if (!file_name.EndsWith(".skp") && !file_name.EndsWith(".fbx") && !file_name.EndsWith(".zip"))
        {
            Debug.Log("The supported file formats are: skp, fbx and zip (containing gltf, bin and texture folder). Aborting job");
            return;
        }
        else
        {
            if (file_name.EndsWith(".zip"))
            {
                Debug.Log("Extracting zip file...");
                ZipFile.ExtractToDirectory(asset_path, "./Assets/");

                if (Directory.Exists("./Assets/__MACOSX/"))
                {
                    Directory.Delete("./Assets/__MACOSX/", true);
                    File.Delete("./Assets/__MACOSX.meta");
                }

                AssetDatabase.DeleteAsset(asset_path);

                if (Directory.Exists("./Assets/" + file_without_extension))
                {
                    Debug.Log("Initial folder exists");
                    if (Directory.GetFiles("./Assets/" + file_without_extension + "/", "*.gltf").Length == 0 ||
                        Directory.GetFiles("./Assets/" + file_without_extension + "/", "*.bin").Length == 0 ||
                        !Directory.Exists("./Assets/" + file_without_extension + "/" + "textures"))
                    {
                        Debug.Log("Wrong data structure. Expected to have gltf + bin + textures");
                        return;
                    }
                    else
                    {
                        Debug.Log("Folder contains gltf bin and textures!");
                        string gltf_file_path = Directory.GetFiles("./Assets/" + file_without_extension + "/", "*.gltf")[0];
                        File.Move(gltf_file_path, "./Assets/" + file_without_extension + "/" + file_without_extension + ".gltf");
                        Debug.Log("Renamed file from " + gltf_file_path + " to " + "./Assets/" + file_without_extension + "/" + file_without_extension + ".gltf");
                        asset_path = "./Assets/" + file_without_extension + "/" + file_without_extension + ".gltf";
                        file_without_extension = Path.GetFileNameWithoutExtension(asset_path);

                        Debug.Log("Asset path: " + asset_path);
                        Debug.Log("File name without extension: " + file_without_extension);
                    }
                }
                else
                {
                    Debug.Log("Initial folder doesn't exists");
                    if (Directory.GetFiles("./Assets/", "*.gltf").Length == 0 ||
                        Directory.GetFiles("./Assets/", "*.bin").Length == 0 ||
                        !Directory.Exists("./Assets/textures"))
                    {
                        Debug.Log("Wrong data structure. Expected to have gltf + bin + textures");
                        return;
                    }
                    else
                    {
                        Debug.Log("Folder contains gltf bin and textures!");
                        string gltf_file_path = Directory.GetFiles("./Assets/", "*.gltf")[0];
                        File.Move(gltf_file_path, "./Assets/" + file_without_extension + ".gltf");
                        Debug.Log("Renamed file from " + gltf_file_path + "to " + "./Assets/" + file_without_extension + "/" + file_without_extension + ".gltf");
                        asset_path = "./Assets/" + file_without_extension + ".gltf";
                        file_without_extension = Path.GetFileNameWithoutExtension(asset_path);

                        Debug.Log("Asset path: " + asset_path);
                        Debug.Log("File name without extension: " + file_without_extension);
                    }
                }
            }
            else
            {
                Debug.Log("Extracting textures...");
                ModelImporter modelImporter = AssetImporter.GetAtPath(asset_path) as ModelImporter;
                modelImporter.isReadable = true;
                modelImporter.ExtractTextures("Assets/Textures/");
                Debug.Log("Extracting textures finished");
            }
        }

        //Import asset into project
        Debug.Log("External file import started...");
        AssetDatabase.ImportAsset(asset_path);
        AssetDatabase.Refresh();
        Debug.Log("External file import finished!");

        //Set bundle name into prefab
        Debug.Log("Set asset bundle name and variant of " + file_without_extension);
        guid = AssetDatabase.FindAssets(file_without_extension, null)[0];

        Debug.Log("New GUID: " + guid);
        asset_path = AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log(asset_path);
        UnityEditor.AssetImporter.GetAtPath(asset_path).SetAssetBundleNameAndVariant(file_without_extension, "");
        AssetDatabase.Refresh();
        Debug.Log("Asset bundle name and variant set!");

        //Build iOS/Android asset bundles
        if (platform_name.Equals("Android"))
        {
            Debug.Log("Building Android asset bundle with BundleOptions NONE");
            BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.Android);
            Debug.Log("Android asset bundle built with BundleOption NONE");
        }
        else if (platform_name.Equals("iOS"))
        {
            Debug.Log("Building iOS asset bundle with BundleOptions NONE");
            BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.iOS);
            Debug.Log("iOS asset bundle built with BundleOption NONE");
        }
    }

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