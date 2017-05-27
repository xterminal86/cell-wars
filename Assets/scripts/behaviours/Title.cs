using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour 
{  
  public RectTransform FormInstructions;

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
    #if !UNITY_EDITOR
    AudioManager.Instance.PlayMusic("in-game-1");
    #endif
  }

  public void ExitGameHandler()
  {
    Application.Quit();
  }

  bool _animationPlaying = false;
  public void OnInstructionsHandler()
  {
    if (_animationPlaying)
    {
      return;
    }

    FormInstructions.gameObject.SetActive(true);

    StartCoroutine(GrowFormRoutine());
  }

  IEnumerator GrowFormRoutine()
  {
    _animationPlaying = true;

    Vector3 scale = FormInstructions.localScale;

    float scaleCounter = 0.0f;
    while (scaleCounter < 1.0f)
    {
      scaleCounter += 0.1f;

      scale.x = scaleCounter;
      scale.y = scaleCounter;
      scale.z = scaleCounter;

      FormInstructions.transform.localScale = scale;

      yield return null;
    }

    scaleCounter = Mathf.Clamp(scaleCounter, 0.0f, 1.0f);
    scale.Set(scaleCounter, scaleCounter, scaleCounter);

    FormInstructions.transform.localScale = scale;

    _animationPlaying = false;

    yield return null;
  }

  public void OnInstructionsCloseHandler()
  {
    if (_animationPlaying)
    {
      return;
    }

    StartCoroutine(ShrinkFormRoutine());
  }

  IEnumerator ShrinkFormRoutine()
  {
    _animationPlaying = true;

    Vector3 scale = FormInstructions.localScale;

    float scaleCounter = 1.0f;
    while (scaleCounter > 0.0f)
    {
      scaleCounter -= 0.1f;

      scale.x = scaleCounter;
      scale.y = scaleCounter;
      scale.z = scaleCounter;

      FormInstructions.transform.localScale = scale;

      yield return null;
    }

    scaleCounter = Mathf.Clamp(scaleCounter, 0.0f, 1.0f);
    scale.Set(scaleCounter, scaleCounter, scaleCounter);

    FormInstructions.transform.localScale = scale;

    _animationPlaying = false;

    FormInstructions.gameObject.SetActive(false);

    yield return null;
  }
}
