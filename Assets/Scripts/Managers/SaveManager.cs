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

    static public string LoadJson(string path, string file)
    {
        foreach (JsonBuffer b in DataBuffer)
        {
            if (b.path == path && b.file == file) return b.json;
        }
        string json = NKTextMan.readText(path + file);
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
            DirectoryUtils.SafeCreateDirectory(b.path);
            NKTextMan.saveText(b.path + b.file, b.json);
        }
    }
}
