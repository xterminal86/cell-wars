using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerInspector : Editor
{
  string _musicPath = "Assets/music";
  string _soundsPath = "Assets/sounds";

  AudioManager _am;

  public override void OnInspectorGUI()
  {
    _am = target as AudioManager;

    if (_am == null) return;

    _am.AudioSourcePrefab = (AudioSource)EditorGUILayout.ObjectField("Audio Source Template Prefab", _am.AudioSourcePrefab, typeof(AudioSource));

    _am.SoundVolume = EditorGUILayout.Slider("Sound Volume", _am.SoundVolume, 0.0f, 1.0f);
    _am.MusicVolume = EditorGUILayout.Slider("Music Volume", _am.MusicVolume, 0.0f, 1.0f);    

    if (GUILayout.Button("Generate Music List"))
    {
      BuildMediaList(_am.MusicTracks, _musicPath);
    }

    PrintListContents(_am.MusicTracks);

    /*
    if (GUILayout.Button("Generate Sounds List"))
    {
      BuildMediaList(_sm.SoundEffects, _soundsPath);
    }

    PrintListContents(_sm.SoundEffects);
    */

    if (GUI.changed)
    {
      EditorUtility.SetDirty(_am);
      AssetDatabase.SaveAssets();
    }
  }

  void BuildMediaList(List<AudioClip> listToPopulate, string pathToDirWithFiles)
  {
    listToPopulate.Clear();

    string[] dirs = Directory.GetDirectories(pathToDirWithFiles, "*", SearchOption.AllDirectories);
    if (dirs.Length == 0)
    {
      LoadFiles(listToPopulate, pathToDirWithFiles, "*.ogg");
    }
    else
    {
      for (int i = 0; i < dirs.Length; i++)
      {
        string dirSlashesFixed = dirs[i].Replace("\\", "/");

        LoadFiles(listToPopulate, dirSlashesFixed, "*.ogg");
      }
    }
  }

  void PrintListContents(List<AudioClip> listToPrint)
  {
    if (listToPrint.Count != 0)
    {
      string text = string.Empty;

      int counter = 0;
      foreach (var item in listToPrint)
      {
        if (item != null)
        {
          text += string.Format("{0}: {1}\n", counter, item.name);
          counter++;
        }
      }

      EditorGUILayout.HelpBox(text, MessageType.None);
    }
  }

  void LoadFiles(List<AudioClip> listToAdd, string path, string filter)
  {
    string[] files = Directory.GetFiles(path, filter);
    for (int j = 0; j < files.Length; j++)
    {      
      string fileSlashFixed = files[j].Replace("\\", "/");

      AudioClip clip = AssetDatabase.LoadAssetAtPath(fileSlashFixed, typeof(AudioClip)) as AudioClip;
      listToAdd.Add(clip);
    }
  }
}
