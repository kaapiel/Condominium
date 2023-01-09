using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;

namespace SlimUI.ModernMenu
{

	public class CheckMusicVolume : MonoBehaviour
	{

		private string baseUrl = "";

		public void Start()
		{

#if UNITY_IPHONE
			Debug.Log("iPhone platform found");
			baseUrl = "https://firebasestorage.googleapis.com/v0/b/condominium-assetbundles.appspot.com/o/";
#endif

#if UNITY_ANDROID
	Debug.Log("Android platform found");
	baseUrl = "https://firebasestorage.googleapis.com/v0/b/condominium-asetbundle-android.appspot.com/o/";
#endif

			// remember volume level from last time
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
			StartCoroutine(DownloadAssetBundles());
		}

		public void UpdateVolume()
		{
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
		}

		IEnumerator DownloadAssetBundles()
		{
			using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl))
			{
				webRequest.SendWebRequest();

				GameObject loading = GameObject.Find("Loading_Text");
				Text loadingText = loading.GetComponent<Text>();
				while (!webRequest.isDone)
				{
					loadingText.text = "Loading Buildings: " + Math.Round(webRequest.downloadProgress * 100f, 2) + "%";
					yield return null;
				}
				loading.SetActive(false);

				if (webRequest.result == UnityWebRequest.Result.Success)
				{
					DriveFiles json = JsonUtility.FromJson<DriveFiles>(webRequest.downloadHandler.text);

					GameObject menuList = GameObject.Find("MenuOptionsVerticalLayout");

					foreach (Item jsonItem in json.items)
					{

						string menuOptionName = jsonItem.name;

						GameObject btnPrefab = Instantiate(Resources.Load("Prefabs/MenuButton")) as GameObject;
						btnPrefab.transform.SetParent(menuList.transform);
						btnPrefab.transform.localScale = new Vector3(1, 1, 1);
						btnPrefab.transform.localPosition = new Vector3(0, 0, 0);
						btnPrefab.transform.localRotation = Quaternion.Euler(0, 0, 0);
						btnPrefab.name = "Btn_" + menuOptionName;

						Transform[] ts = btnPrefab.GetComponentsInChildren<Transform>();
						ts[2].GetComponent<Text>().text = jsonItem.name;

						Button btn = btnPrefab.GetComponent<Button>();
						btn.onClick.AddListener(delegate { StartCoroutine(openExternalFile(menuOptionName)); });

#if UNITY_EDITOR
						//Texture2D texture = UnityEditor.AssetPreview.GetAssetPreview(Resources.Load("Models/" + menuOptionName.ToUpper()));
						//Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.0f, 0.0f));
						//sprite.name = "Image_" + menuOptionName;

						//Transform[] ts = btnPrefab.GetComponentsInChildren<Transform>();
						//ts[0].GetComponent<Image>().sprite = sprite;
						//ts[1].GetComponent<Image>().sprite = sprite;
#endif

						// This is not working. Here we're trying to set the scroll bar initially at top
						//GameObject scrollBar = GameObject.Find("Scrollbar Vertical");
						//scrollBar.transform.GetComponent<Scrollbar>().value = 1f;
					}
				}
			}
		}

		IEnumerator openExternalFile(string cadName)
		{

			GameObject btnObject = GameObject.Find("Btn_" + cadName);
			Transform[] transform = btnObject.GetComponentsInChildren<Transform>();

			using (UnityWebRequest wr = UnityWebRequest.Get(baseUrl + cadName))
			{
				wr.SendWebRequest();

				GameObject mainMenu = GameObject.Find("Main_Menu_New");
				Transform[] trs = mainMenu.GetComponentsInChildren<Transform>(true);
				GameObject loadingScreen = null;
				foreach (Transform t in trs)
				{
					if (t.name == "LoadingScreen")
					{
						loadingScreen = t.gameObject;
						t.gameObject.SetActive(true);
						break;
					}
				}

				GameObject downloadPercentage = GameObject.Find("LoadingPercentage");
				GameObject loadingBar = GameObject.Find("LoadingBar");

				while (!wr.isDone)
				{
					//downloadPercentage.transform.GetComponent<Text>().text = Math.Round(wr.downloadProgress * 100f, 2) + "%";
					//loadingBar.transform.GetComponent<Slider>().value = (float) Math.Round(wr.downloadProgress, 2);
					yield return null;
				}

				if (wr.result == UnityWebRequest.Result.Success)
				{
					DriveFileData jsonFileData = JsonUtility.FromJson<DriveFileData>(wr.downloadHandler.text);
					string fileURL = baseUrl + cadName + "?alt=media&token=" + jsonFileData.downloadTokens;

					UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(fileURL);
					www.SendWebRequest();

					while (!www.isDone)
					{
						downloadPercentage.transform.GetComponent<Text>().text = Math.Round(www.downloadProgress * 100f + 10, 2) + "%";
						loadingBar.transform.GetComponent<Slider>().value = (float)Math.Round(www.downloadProgress * 100f + 10, 2);
						yield return null;
					}

					if (www.result == UnityWebRequest.Result.Success)
					{

						//default character
						GameObject controls = Resources.Load("Prefabs/Characters/FemaleCharacter") as GameObject;

						if (GameObject.Find("MALELINE") != null)
						{
							controls = Resources.Load("Prefabs/Characters/MaleCharacter") as GameObject;
						}
						else if (GameObject.Find("FEMALELINE") != null)
						{
							controls = Resources.Load("Prefabs/Characters/FemaleCharacter") as GameObject;
						}
						else if (GameObject.Find("ADVENTURERLINE") != null)
						{
							controls = Resources.Load("Prefabs/Characters/AdventurerCharacter") as GameObject;
						}

						GameObject.Find("EventSystem").SetActive(false);
						GameObject.Find("Canv_Options").SetActive(false);
						GameObject.Find("Canv_Main").SetActive(false);
						GameObject.Find("Camera").SetActive(false);

						Scene scene = SceneManager.CreateScene(cadName);
						SceneManager.SetActiveScene(scene);

						Material skyMaterial = Resources.Load("Materials/SkyboxLiteWarm", typeof(Material)) as Material;
						RenderSettings.skybox = skyMaterial;

						GameObject lightGameObject = new GameObject("Sun");
						Light lightComp = lightGameObject.AddComponent<Light>();
						lightComp.type = LightType.Directional;
						lightComp.transform.localPosition = new Vector3(0, 1000, 0);
						lightComp.transform.localRotation = Quaternion.EulerAngles(-90, 0, 170);
						lightComp.shadows = LightShadows.Hard;

						//RenderSettings.sun = lightComp;

						GameObject moonGameObject = new GameObject("Moon");
						Light moonComp = moonGameObject.AddComponent<Light>();
						moonComp.type = LightType.Directional;
						moonComp.transform.localPosition = new Vector3(0, 1000, 0);
						moonComp.transform.localRotation = Quaternion.EulerRotation(90, -180, 0);
						moonComp.shadows = LightShadows.Hard;
						moonComp.color = new Color(121f / 255f, 159f / 255f, 159f / 255f);

						LightingPreset lightingPreset = ScriptableObject.CreateInstance("LightingPreset") as LightingPreset;
						lightingPreset.AmbientColor = new Gradient();
						lightingPreset.DirectionalColor = new Gradient();
						lightingPreset.FogColor = new Gradient();

						Transform[] ts = controls.GetComponentsInChildren<Transform>();
						//PlayerArmature
						ts[3].localPosition = new Vector3(0, 15, 0);

						//Download assetbundle
						GameObject asset = DownloadHandlerAssetBundle.GetContent(www).LoadAsset(cadName) as GameObject;

						//We set the layer only to the children object so that the selection work properly
						//foreach (Transform t in asset.transform)
						//{
						//	t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
						//}
						Debug.Log("Downloaded asset bundle: " + asset);

						//Uncomment this method to quit ghost mode
						CreateMeshColliderOnMeshFilter(asset.transform);

						//Set within assetbundle the script to attach the sun
						asset.AddComponent<LightingManager>();

						Component[] components = asset.GetComponents<Component>();

						//Search for LightManager in order to edit it
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

								//Set the moon and sun
								directionalLightSunFieldInfo.SetValue(c, lightComp);
								directionalLightMoonFieldInfo.SetValue(c, moonComp);

								//Set the preset for day and night
								presetFieldInfo.SetValue(c, lightingPreset);
							}
						}

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

						Instantiate(asset);
						Instantiate(controls);
					}

					loadingScreen.SetActive(false);
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
}