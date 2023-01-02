using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AssetsDropdown : MonoBehaviour
{
	public TMP_Dropdown m_Dropdown;
	TMP_Dropdown.OptionData m_NewData;
	List<TMP_Dropdown.OptionData> m_Messages = new List<TMP_Dropdown.OptionData>();
	string selected_option;
	GameObject asset;

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

		m_Dropdown = GetComponent<TMP_Dropdown>();
		m_Dropdown.ClearOptions();

		m_NewData = new TMP_Dropdown.OptionData();
		m_NewData.text = "";
		m_Messages.Add(m_NewData);

        StartCoroutine(DownloadAssetBundlesLists());
	}

    // Update is called once per frame
    void Update()
    {
        
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
			Debug.Log("Downloading asset bundle metadata: " + selected_option);
			selected_option = assetName;
			wr.SendWebRequest();

			while (!wr.isDone)
			{
				Debug.Log("Done Downloading metadata");
				yield return null;
			}

			if (wr.result == UnityWebRequest.Result.Success)
			{
				Debug.Log("Success downloading metadata");
				DriveFileData jsonFileData = JsonUtility.FromJson<DriveFileData>(wr.downloadHandler.text);
				string fileURL = baseUrl + assetName + "?alt=media&token=" + jsonFileData.downloadTokens;

				Debug.Log("Downloading assetbundle binary file");
				UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(fileURL);
				www.SendWebRequest();

				while (!www.isDone)
				{
					Debug.Log("Done downloading assetbundle binary file");
					yield return null;
				}

				if (www.result == UnityWebRequest.Result.Success)
				{
					Debug.Log("Success downloading assetbundle");
					AssetBundle.UnloadAllAssetBundles(true);

					asset = DownloadHandlerAssetBundle.GetContent(www).LoadAsset(assetName) as GameObject;
                    GameObject camera = GameObject.Find("MainCamera");
					asset.transform.position = new Vector3(0, 0, 0);
					Instantiate(asset);
				}
			}
		}

		yield return null;
	}

	void DropdownValueChanged(TMP_Dropdown change)
	{
		Debug.Log(change.value);
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
