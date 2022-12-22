/*
    A simple little editor extension to copy and paste all components
    Help from http://answers.unity3d.com/questions/541045/copy-all-components-from-one-character-to-another.html
    license: WTFPL (http://www.wtfpl.net/)
    author: aeroson
    advise: ChessMax
    editor: frekons
*/

#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class CondominiumCustomScripts : MonoBehaviour
{

    static List<Transform> transforms_skp = new List<Transform>(); 
    static List<Transform> transforms = new List<Transform>();

    [MenuItem("Condominium/Build AssetBundles")]
    static void BuildAssetBundles()
    {
        //string fileName = "/Assets/Resources/Models/DW06_copy.skp";
        //string file_without_extension = System.IO.Path.GetFileNameWithoutExtension(fileName);

        //string guid = AssetDatabase.FindAssets(fileName, null)[0];
        //Debug.Log("Asset GUID found: " + guid);

        ////Retrieve file path from GUID
        //Debug.Log("Retrieving asset path from asset GUID");
        //string asset_path = AssetDatabase.GUIDToAssetPath(guid);
        //Debug.Log("Asset path retrieved: " + asset_path);

        //Debug.Log("Extracting textures...");
        //ModelImporter modelImporter = AssetImporter.GetAtPath(asset_path) as ModelImporter;
        //modelImporter.isReadable = true;
        //modelImporter.ExtractTextures("Assets/Textures/");
        //Debug.Log("Extracting textures finished");

        ////Import asset into project
        //Debug.Log("External file import started...");
        //AssetDatabase.ImportAsset(asset_path);
        //Debug.Log("External file import finished!");

        ////Set bundle name into prefab
        //UnityEditor.AssetImporter.GetAtPath(asset_path).SetAssetBundleNameAndVariant(file_without_extension, "");
        //Debug.Log("Asset bundle name and variant set!");

        string assetsBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetsBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.iOS);

    }

    [MenuItem("Condominium/Add dynamic day and night into asset")]
    static void AddScriptIntoAsset()
    {
        GameObject activeGameObject = Selection.activeGameObject;
        activeGameObject.AddComponent<LightingManager>();
        Component[] components = activeGameObject.GetComponents<Component>();

        Material skyMaterial = Resources.Load("Materials/SkyboxLiteWarm", typeof(Material)) as Material;
        RenderSettings.skybox = skyMaterial;

        GameObject lightGameObject = new GameObject("Sun");
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.transform.localPosition = new Vector3(0, 1000, 0);
        lightComp.transform.localRotation = Quaternion.EulerAngles(-90, 0, 170);
        lightComp.shadows = LightShadows.Hard;

        GameObject moonGameObject = new GameObject("Moon");
        Light moonComp = moonGameObject.AddComponent<Light>();
        moonComp.type = LightType.Directional;
        moonComp.transform.localPosition = new Vector3(0, 1000, 0);
        moonComp.transform.localRotation = Quaternion.EulerRotation(90, -180, 0);
        moonComp.shadows = LightShadows.Hard;
        moonComp.color = new Color(121f/255f, 159f/255f, 159f/255f);

        LightingPreset lightingPreset = ScriptableObject.CreateInstance("LightingPreset") as LightingPreset;
        lightingPreset.AmbientColor = new Gradient();
        lightingPreset.DirectionalColor = new Gradient();
        lightingPreset.FogColor = new Gradient();

        //Create a floor :)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localPosition = new Vector3(0, -0.25f, 0);
        floor.transform.localScale = new Vector3(500, 0.5f, 500);

        Material waterMaterial = Resources.Load("Imported/AQUAS-Lite/Materials/AQUAS_Lite_Water", typeof(Material)) as Material;
        Material waterBackFaceMaterial = Resources.Load("Imported/AQUAS-Lite/Materials/AQUAS_Lite_Water_Backface", typeof(Material)) as Material;

        Component[] comps = floor.GetComponents<Component>();
        foreach (Component comp in comps)
        {
            if (comp.GetType() == typeof(MeshRenderer))
            {
                Material[] mats = new Material[2];
                mats[0] = waterMaterial;
                mats[1] = waterBackFaceMaterial;
                ((MeshRenderer)comp).materials = mats;
            }
        }

        foreach (Component c in components)
        {
            if (c.GetType() == typeof(LightingManager))
            {

                FieldInfo directionalLightSunFieldInfo =
                    c.GetType().GetField(
                        "DirectionalLightSun",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    );

                FieldInfo directionalLightMoonFieldInfo =
                    c.GetType().GetField(
                        "DirectionalLightMoon",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    );

                FieldInfo presetFieldInfo =
                    c.GetType().GetField(
                        "Preset",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    );

                directionalLightSunFieldInfo.SetValue(c, lightComp);
                directionalLightMoonFieldInfo.SetValue(c, moonComp);
                presetFieldInfo.SetValue(c, lightingPreset);
            }
        }
    }

    [MenuItem("Condominium/Copy all components and paste on DW06")]
    static void Copy()
    {
        DisplayChildren(Selection.activeGameObject.transform);
        DisplayChildren2(GameObject.Find("DW06").transform);

        foreach (Transform skp_transform in transforms_skp) {
            foreach (Transform broken_transform in transforms) {
                if (skp_transform.name == broken_transform.name) {
                    Component[] skp_components = skp_transform.GetComponents(typeof(Component));
                    foreach(Component skp_component in skp_components) {
                        if (skp_component.GetType() != typeof(Transform)) {
                            DestroyImmediate(broken_transform.gameObject.GetComponent(skp_component.GetType()));
                            ReplaceComponent(skp_component, broken_transform.gameObject);
                        } else {
                            ReplaceComponent(skp_component, broken_transform.gameObject);
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Condominium/Remove Empty Mesh Colliders")]
    static void Remove()
    {
        RemoveAllEmptyMeshColliders(Selection.activeGameObject.transform);
    }

    static void RemoveAllEmptyMeshColliders(Transform trans)
    {
        foreach (Transform child in trans)
        {
            if (child.childCount > 0)
            {
                Component[] components = child.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c.GetType() == typeof(MeshCollider))
                    {
                        if (((MeshCollider)c).sharedMesh == null)
                        {
                            DestroyImmediate(c);
                        }
                    }
                }
                DisplayChildren(child);
            }
            else
            {
                Component[] components = child.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c.GetType() == typeof(MeshCollider))
                    {
                        if (((MeshCollider)c).sharedMesh == null)
                        {
                            DestroyImmediate(c);
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Condominium/Create MeshCollider on MeshFilter")]
    static void CreateMeshCollider()
    {
        CreateMeshColliderOnMeshFilter(Selection.activeGameObject.transform);
    }

    static void CreateMeshColliderOnMeshFilter(Transform trans)
    {
        //This is the first object (parent)
        Component[] comps = trans.GetComponents<Component>();
        foreach (Component c in comps)
        {
            if (c.GetType() == typeof(MeshFilter))
            {
                DestroyImmediate(trans.gameObject.GetComponent<MeshCollider>());
                MeshCollider m = trans.gameObject.AddComponent<MeshCollider>();
                m.sharedMesh = ((MeshFilter)c).sharedMesh;
                //m.convex = true;
                //Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                //rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            }

            if (c.GetType() == typeof(Camera))
            {
                Camera cam = c as Camera;
                cam.enabled = false;
            }
        }

        //This is all the children (sub gameobjects)
        foreach (Transform child in trans)
        {
            if (child.name.Contains("Door", StringComparison.OrdinalIgnoreCase) ||
                    child.name.Contains("Window", StringComparison.OrdinalIgnoreCase) ||
                    child.name.Contains("Animation", StringComparison.OrdinalIgnoreCase) ||
                    child.name.Contains("Water", StringComparison.OrdinalIgnoreCase) ||
                    child.name.Contains("Glass", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (child.childCount > 0)
            {
                Component[] components = child.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c.GetType() == typeof(MeshFilter))
                    {
                        DestroyImmediate(child.gameObject.GetComponent<MeshCollider>());
                        MeshCollider m = child.gameObject.AddComponent<MeshCollider>();
                        m.sharedMesh = ((MeshFilter)c).sharedMesh;
                        //m.convex = true;
                        //Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                        //rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    }

                    if (c.GetType() == typeof(Camera))
                    {
                        Camera cam = c as Camera;
                        cam.enabled = false;
                    }
                }
                CreateMeshColliderOnMeshFilter(child);
            }
            else
            {
                Component[] components = child.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c.GetType() == typeof(MeshFilter))
                    {
                        DestroyImmediate(child.gameObject.GetComponent<MeshCollider>());
                        MeshCollider m = child.gameObject.AddComponent<MeshCollider>();
                        m.sharedMesh = ((MeshFilter)c).sharedMesh;
                        //m.convex = true;
                        //Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                        //rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    }

                    if (c.GetType() == typeof(Camera))
                    {
                        Camera cam = c as Camera;
                        cam.enabled = false;
                    }
                }
            }
        }
    }
    
    static void AttachMeshes(Transform trans)
    {
        foreach (Transform child in trans)
        {
            if (child.childCount > 0)
            {
                Component[] components = child.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c.GetType() == typeof(MeshCollider))
                    {
                        DestroyImmediate(c);
                        child.gameObject.AddComponent<MeshCollider>();
                    }
                }
                AttachMeshes(child);
            }
            else
            {
                Component[] components = child.GetComponents<Component>();
                foreach (Component c in components)
                {
                    if (c.GetType() == typeof(MeshCollider))
                    {
                        DestroyImmediate(c);
                        child.gameObject.AddComponent<MeshCollider>();
                    }
                }
            }
        }
    }

    static void ReplaceComponent(Component skp_component, GameObject broken_transform_gameObject) {
        
        System.Type type = skp_component.GetType();

        if (type == typeof(Transform)) {
            ((Transform) broken_transform_gameObject.GetComponent(type)).localScale = ((Transform) skp_component).localScale;
            ((Transform) broken_transform_gameObject.GetComponent(type)).rotation = ((Transform) skp_component).rotation;
            return;
        }

        Component copy = broken_transform_gameObject.AddComponent(type);

        if (type == typeof(MeshFilter)) {
            ((MeshFilter) copy).sharedMesh = ((MeshFilter) skp_component).sharedMesh;
        } else if (type == typeof(Renderer)) {
            ((Renderer) copy).sharedMaterial = ((Renderer) skp_component).sharedMaterial;
        } else if (type == typeof(MeshRenderer)) {
            ((MeshRenderer) copy).sharedMaterials = ((MeshRenderer) skp_component).sharedMaterials;
            ((MeshRenderer) copy).shadowCastingMode = ((MeshRenderer) skp_component).shadowCastingMode;
            ((MeshRenderer) copy).receiveShadows = ((MeshRenderer) skp_component).receiveShadows;
            ((MeshRenderer) copy).lightProbeUsage = ((MeshRenderer) skp_component).lightProbeUsage;
            ((MeshRenderer) copy).reflectionProbeUsage = ((MeshRenderer) skp_component).reflectionProbeUsage;
            ((MeshRenderer) copy).allowOcclusionWhenDynamic = ((MeshRenderer) skp_component).allowOcclusionWhenDynamic;
        } else {

            //Copied fields can be restricted with BindingFlags
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            
            foreach (var pinfo in pinfos) 
            {
                if (pinfo.CanWrite) 
                {
                    try 
                    {
                        pinfo.SetValue(copy, pinfo.GetValue(skp_component, null), null);
                    }
                    catch
                    {
                         /*
                          * In case of NotImplementedException being thrown.
                          * For some reason specifying that exception didn't seem to catch it,
                          * so I didn't catch anything specific.
                          */
                    }
                }
            }
            
            System.Reflection.FieldInfo[] fields = type.GetFields(); 
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(skp_component));
            }

        }
    }

    static void DisplayChildren(Transform trans)
    {
        foreach(Transform child in trans) {
            //Debug.Log(child.name);
            if (child.childCount > 0) {
                transforms_skp.Add(child);
                DisplayChildren(child);
            } else {
                transforms_skp.Add(child);
            }
        }
    }

    static void DisplayChildren2(Transform trans)
    {
        foreach(Transform child in trans) {
            //Debug.Log(child.name);
            if (child.childCount > 0) {
                transforms.Add(child);
                DisplayChildren2(child);
            } else {
                transforms.Add(child);
            }
        }
    }

}
#endif