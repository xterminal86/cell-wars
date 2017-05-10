﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour 
{  
  void Start()
  {
    AudioManager.Instance.Initialize();
  }

  public void NewGameHandler()
  {
    SceneManager.sceneLoaded += SceneLoadedHandler;
    SceneManager.LoadScene("main");
  }

  void SceneLoadedHandler(Scene scene, LoadSceneMode mode)
  { 
    SceneManager.sceneLoaded -= SceneLoadedHandler;
    //AudioManager.Instance.PlayMusic("in-game-1");
  }

  public void ExitGameHandler()
  {
    Application.Quit();
  }
}
