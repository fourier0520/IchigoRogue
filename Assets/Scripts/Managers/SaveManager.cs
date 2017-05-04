using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    class JsonBuffer
    {
        public string path = "";
        public string file = "";
        public string json = "";

        // デフォルトコンストラクタ
        public JsonBuffer(string p, string f, string j)
        {
            this.path = p;
            this.file = f;
            this.json = j;
        }
    }

    static private List<JsonBuffer> DataBuffer = new List<JsonBuffer>();
    static private List<JsonBuffer> FixedDataBuffer = new List<JsonBuffer>();

    static public string LoadFixedJson(string path)
    {
        foreach (JsonBuffer b in FixedDataBuffer)
        {
            if (b.path == path) return b.json;
        }
        TextAsset jsonAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
        string json = jsonAsset.ToString();
        FixedDataBuffer.Add(new JsonBuffer(path, "", json));

        return json;
    }

    static public string LoadJson(string path, string file)
    {
        string json = "";
        if (!TitleDataLoad.instance.InisialData)
        {
            foreach (JsonBuffer b in DataBuffer)
            {
                if (b.path == path && b.file == file) return b.json;
            }
#if true
            json = PlayerPrefs.GetString(path + file, "");
#else
        string json = NKTextMan.readText(path + file);
#endif
        }
        DataBuffer.Add(new JsonBuffer(path, file, json));
        return json;
    }

    static public void HoldJson(string path, string file, string json)
    {
        foreach (JsonBuffer b in DataBuffer)
        {
            if (b.path == path && b.file == file)
            {
                b.json = json;
                return;
            }
        }
        DataBuffer.Add(new JsonBuffer(path, file, json));
    }

    static public void SaveAllJson()
    {
        foreach (JsonBuffer b in DataBuffer)
        {
#if true
            PlayerPrefs.SetString(b.path + b.file, b.json);
#else
            DirectoryUtils.SafeCreateDirectory(b.path);
            NKTextMan.saveText(b.path + b.file, b.json);
#endif
        }
    }
}
