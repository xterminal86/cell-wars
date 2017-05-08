using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonWatcher : MonoBehaviour 
{
  public Button ButtonComponent;

  public GameObject TextNormal;
  public GameObject TextPressed;

  public AudioSource SoundDown;
  public AudioSource SoundUp;

  public UnityEvent Method;

  bool _pressed = false;
  bool _pointerExited = true;

  void OnDisable()
  { 
    _pressed = false;
    _pointerExited = true;
    _playOnce = false;
  }

  public void OnPointerDownHandler()
  { 
    if (ButtonComponent.interactable)
    {      
      _pressed = true;
      //Debug.Log("pointer down");
    }
  }

  public void OnPointerUpHandler()
  {    
    _pressed = false;

    //Debug.Log("pointer up");

    if (!_pressed && !_pointerExited && ButtonComponent.interactable)
    { 
      if (Method != null)
        Method.Invoke();      
    }
  }

  public void OnPointerExitHandler()
  {
    _pointerExited = true;
  }

  public void OnPointerEnterHandler()
  {
    _pointerExited = false;
  }

  bool _playOnce = false;
  void Update()
  {    
    if (_pressed && !_pointerExited)
    {
      if (!_playOnce)
      {
        _playOnce = true;

        if (SoundDown != null)          
          SoundDown.Play();
      }

      TextNormal.SetActive(false);
      TextPressed.SetActive(true);
    }
    else
    {
      if (_playOnce)
      {
        _playOnce = false;

        if (SoundUp != null)
          SoundUp.Play();
      }

      TextNormal.SetActive(true);
      TextPressed.SetActive(false);
    }
  }
}

