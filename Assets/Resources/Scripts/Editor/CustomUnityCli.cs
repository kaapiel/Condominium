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

        string arg = System.IO.Path.GetFileNameWithoutExtension(GetArg("-executeMethod"));
        Debug.Log("External file found: " + arg);

        //Find prefab name and get its GUID
        Debug.Log("Finding GUID from asset name: " + arg);
        string guid = AssetDatabase.FindAssets(arg, null)[0];
        Debug.Log("Asset GUID found: " + guid);

        //Retrieve file path from GUID
        Debug.Log("Retrieving asset path from asset GUID");
        string asset_path = AssetDatabase.GUIDToAssetPath(guid);
        Debug.Log("Asset path retrieved: " + asset_path);

        //Define its coordinates
        //GameObject building = AssetDatabase.LoadAssetAtPath(asset_path, typeof(GameObject)) as GameObject;
        //building.transform.localPosition = new Vector3(0, 0, 0);

        //Import asset into project
        //Debug.Log("External file import started...");
        //AssetDatabase.ImportAsset(asset_path);
        //Debug.Log("External file import finished!");

        //Set bundle name into prefab
        Debug.Log("Set asset bundle name and variant of " + arg);
        UnityEditor.AssetImporter.GetAtPath(asset_path).SetAssetBundleNameAndVariant(arg, "");
        Debug.Log("Asset bundle name and variant set!");

        //Build iOS asset bundles
        Debug.Log("Building iOS asset bundle with BundleOptions UncompressedAssetBundle");
        AssetBundleBuild[] build = new AssetBundleBuild[1];
        build[0] = new AssetBundleBuild();
        build[0].assetBundleName = arg;
        build[0].assetNames = new string[1] { asset_path };
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", build, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
        Debug.Log("iOS asset bundle built with BundleOption UncompressedAssetBundle");
    }

    // Helper function for getting the command line arguments
    private static string GetArg(string name)
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
}