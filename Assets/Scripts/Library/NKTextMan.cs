/*
 * NKTextMan.cs
 * 
 * Copyright (c) 2016 by nanoka All rights reserved.
 * Created by nanoka on 2016/06/29.
 * 
 * Qiita  ：http://qiita.com/nanokanato
 * Twitter：https://twitter.com/nanokanato
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Directory クラスに関する汎用関数を管理するクラス
/// </summary>
public static class DirectoryUtils
{
    /// <summary>
    /// 指定したパスにディレクトリが存在しない場合
    /// すべてのディレクトリとサブディレクトリを作成します
    /// </summary>
    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        Debug.Log(path);
        if (path == null) return null;
        if (path == "") return null;
        if (Directory.Exists(Application.dataPath + path))
        {
            return null;
        }
        return Directory.CreateDirectory(Application.dataPath + path);
    }
}

/*==============================================================*
 * NKTextMan : Assetsフォルダ以下で文字列データ管理
 *==============================================================*/
public class NKTextMan : MonoBehaviour
{

    /*----------------------------------------------------------*
     * saveText : Assetsフォルダ以下のpathのファイルにtextの内容を保存
     *       in : string path
     *          : string text
     *      out : bool
     *----------------------------------------------------------*/
    static public bool saveText(string path, string text)
    {
        //ストリームライターwriterに書き込む
        try
        {
            using (StreamWriter writer = new StreamWriter(Application.dataPath + path, false))
            {
                writer.Write(text);
                writer.Flush();
                writer.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
        return true;
    }

    /*----------------------------------------------------------*
     * saveText : Assetsフォルダ以下のpathのファイルからtextの内容を取得
     *       in : string path
     *      out : string
     *----------------------------------------------------------*/
    static public string readText(string path)
    {
        //ストリームリーダーsrに読み込む
        string strStream = "";
        try
        {
            //※Application.dataPathはプロジェクトデータのAssetフォルダまでのアクセスパスのこと,
            using (StreamReader sr = new StreamReader(Application.dataPath + path))
            {
                //ストリームリーダーをstringに変換
                strStream = sr.ReadToEnd();
                sr.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return strStream;
    }
}