using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AssetsDropdown : MonoBehaviour
{
	public TMP_Dropdown m_Dropdown;
	TMP_Dropdown.OptionData m_NewData;
	List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();
	string selected_option;
	GameObject asset;
	Slider x_Slider;
	Slider y_Slider;
	Slider z_Slider;
	Slider m_RotationSlider;
	Slider m_ScaleSlider;
	Button m_applyButton;

	string baseUrl = "";

	void Start()
    {
#if UNITY_IPHONE
		Debug.Log("iPhone platform found");
		baseUrl = "https://firebasestorage.googleapis.com/v0/b/condominium-assetbundles.appspot.com/o/";
#endif

#if UNITY_ANDROID
		Debug.Log("Android platform found");
		baseUrl = "https://firebasestorage.googleapis.com/v0/b/condominium-asetbundle-android.appspot.com/o/";
#endif

		x_Slider = GameObject.Find("X_Slider").GetComponent<Slider>();
		y_Slider = GameObject.Find("Y_Slider").GetComponent<Slider>();
		z_Slider = GameObject.Find("Z_Slider").GetComponent<Slider>();

		m_RotationSlider = GameObject.Find("Rotation_Slider").GetComponent<Slider>();
		m_ScaleSlider = GameObject.Find("Scale_Slider").GetComponent<Slider>();
		m_applyButton = GameObject.Find("ApplyButton").GetComponent<Button>();

		m_Dropdown = GetComponent<TMP_Dropdown>();
		m_Dropdown.ClearOptions();

		m_NewData = new TMP_Dropdown.OptionData();
		m_NewData.text = "";
		m_Messages.Add(m_NewData);

		m_Dropdown.onValueChanged.AddListener(delegate { StartCoroutine(DownloadAsset(m_Dropdown.options[m_Dropdown.value].text)); });
		m_applyButton.onClick.AddListener(delegate { CreateMeshColliderOnMeshFilter(asset.transform); });

        StartCoroutine(DownloadAssetBundlesLists());
	}

    void Update()
    {
		asset.transform.position = new Vector3(x_Slider.value, y_Slider.value, z_Slider.value);
		asset.transform.rotation = Quaternion.Euler(0, m_RotationSlider.value, 0);
		asset.transform.localScale = new Vector3(m_ScaleSlider.value, m_ScaleSlider.value, m_ScaleSlider.value);
	}

	IEnumerator DownloadAssetBundlesLists()
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl))
		{
			webRequest.SendWebRequest();

			while (!webRequest.isDone)
			{
				//loadingText.text = "Loading Buildings: " + Math.Round(webRequest.downloadProgress * 100f, 2) + "%";
				yield return null;
			}
			//loading.SetActive(false);

			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				DriveFiles json = JsonUtility.FromJson<DriveFiles>(webRequest.downloadHandler.text);

				foreach (Item jsonItem in json.items)
				{
					m_NewData = new TMP_Dropdown.OptionData();
					m_NewData.text = jsonItem.name;
					m_Messages.Add(m_NewData);
				}

				m_Dropdown.options = m_Messages;
			}
		}
	}

	IEnumerator DownloadAsset(string assetName)
    {
		using (UnityWebRequest wr = UnityWebRequest.Get(baseUrl + assetName))
        {
			selected_option = assetName;
			wr.SendWebRequest();

			while (!wr.isDone)
			{
				yield return null;
			}

			if (wr.result == UnityWebRequest.Result.Success)
			{
				DriveFileData jsonFileData = JsonUtility.FromJson<DriveFileData>(wr.downloadHandler.text);
				string fileURL = baseUrl + assetName + "?alt=media&token=" + jsonFileData.downloadTokens;

				UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(fileURL);
				www.SendWebRequest();

				while (!www.isDone)
				{
					yield return null;
				}

				if (www.result == UnityWebRequest.Result.Success)
				{
					Debug.Log("Success downloading assetbundle: " + assetName);
					GameObject assetBundle = DownloadHandlerAssetBundle.GetContent(www).LoadAsset(assetName) as GameObject;
                    GameObject camera = GameObject.Find("MainCamera");
					asset = Instantiate(assetBundle);

					asset.transform.position = camera.transform.position + camera.transform.forward * 5;
					
					x_Slider.value = asset.transform.position.x;
					y_Slider.value = 0;
					z_Slider.value = asset.transform.position.z;
				}
			}
		}

		yield return null;
	}

	private void CreateMeshColliderOnMeshFilter(Transform trans)
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
				//Rigidbody rb = c.gameObject.AddComponent<Rigidbody>();
				//rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
			}

			if (c.GetType() == typeof(Camera))
			{
				Camera cam = c as Camera;
				cam.enabled = false;
			}
		}

		//This are all the children (sub gameobjects)
		foreach (Transform child in trans)
		{
			if (child.name.Contains("Door", StringComparison.OrdinalIgnoreCase) ||
				child.name.Contains("Window", StringComparison.OrdinalIgnoreCase) ||
				child.name.Contains("Animation", StringComparison.OrdinalIgnoreCase) ||
				child.name.Contains("Water", StringComparison.OrdinalIgnoreCase) ||
				child.name.Contains("Glass", StringComparison.OrdinalIgnoreCase) ||
				child.name.Contains("Camera", StringComparison.OrdinalIgnoreCase))
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

		GameObject.Find("ImportCanvas").active = false;
		FindIncludingInactive("JoystickCanvas").active = true;
	}

	public static GameObject FindIncludingInactive(string name)
	{
		Scene scene = SceneManager.GetActiveScene();
		if (!scene.isLoaded)
		{
			//no scene loaded
			return null;
		}

		var game_objects = new List<GameObject>();
		scene.GetRootGameObjects(game_objects);

		foreach (GameObject obj in game_objects)
		{
			if (obj.transform.name == name) return obj;

			GameObject found = FindInChildrenIncludingInactive(obj, name);
			if (found) return found;
		}

		return null;
	}

	private static GameObject FindInChildrenIncludingInactive(GameObject go, string name)
	{

		for (int i = 0; i < go.transform.childCount; i++)
		{
			if (go.transform.GetChild(i).gameObject.name == name) return go.transform.GetChild(i).gameObject;
			GameObject found = FindInChildrenIncludingInactive(go.transform.GetChild(i).gameObject, name);
			if (found != null) return found;
		}

		return null;  //couldn't find crap
	}

	[System.Serializable]
	public class Item
	{
		public string name;
		public string bucket;
	}

	[System.Serializable]
	public class DriveFiles
	{
		public List<string> prefixes;
		public List<Item> items;
	}

	[System.Serializable]
	public class DriveFileData
	{
		public string name;
		public string bucket;
		public string generation;
		public string metageneration;
		public string contentType;
		public string timeCreated;
		public string updated;
		public string storageClass;
		public string size;
		public string md5Hash;
		public string contentEncoding;
		public string contentDisposition;
		public string crc32c;
		public string etag;
		public string downloadTokens;
	}
}
