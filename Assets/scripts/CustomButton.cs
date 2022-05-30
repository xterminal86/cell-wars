using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CustomButton : MonoBehaviour 
{   
  bool _previousState = true;

  public bool _Interactable = true;
  public bool Interactable
  {
    get { return _Interactable; }
    set 
    { 
      _Interactable = value; 

      UpdateButtonStatus();
    }
  }

  public GameObject SpriteNormal;
  public GameObject SpritePressed;
  public GameObject SpriteDisabled;

  public GameObject TextNormal;
  public GameObject TextPressed;
  public GameObject TextDisabled;

  public AudioSource SoundDown;
  public AudioSource SoundUp;

  public UnityEvent MethodToCall;

  bool _pressed = false;
  bool _pointerExited = true;

  void Awake()
  {    
    _previousState = _Interactable;

    UpdateButtonStatus();
  }

  void UpdateButtonStatus()
  { 
    if (_previousState == _Interactable)
    {
      return;
    }

    _previousState = _Interactable;

    if (_Interactable)
    {
      SpriteNormal.SetActive(true);
      SpritePressed.SetActive(false);
      SpriteDisabled.SetActive(false);
      TextNormal.SetActive(true);
      TextDisabled.SetActive(false);
    }
    else
    {      
      SpriteNormal.SetActive(false);
      SpritePressed.SetActive(false);
      SpriteDisabled.SetActive(true);
      TextNormal.SetActive(false);
      TextDisabled.SetActive(true);
    }
  }

  public void OnPointerDownHandler()
  { 
    //Debug.Log("pointer down");

    if (_Interactable && Input.GetMouseButtonDown(0))
    {      
      _pressed = true;

      ToggleButton(true);
    }
  }

  public void OnPointerUpHandler()
  { 
    //Debug.Log("pointer up");

    if (_pressed && !_pointerExited && _Interactable && Input.GetMouseButtonUp(0))
    {
      _pressed = false;

      //Debug.Log("method call");

      ToggleButton(false);

      if (MethodToCall != null)
        MethodToCall.Invoke();      
    }
  }

  public void OnPointerExitHandler()
  {
    //Debug.Log("pointer exit");

    if (_pressed)
    {
      _pressed = false;
      ToggleButton(false);
    }

    _pointerExited = true;
  }

  public void OnPointerEnterHandler()
  {
    //Debug.Log("pointer enter");

    _pointerExited = false;
  }

  void ToggleButton(bool status)
  {
    if (status)
    {
      if (SoundDown != null)
        SoundDown.Play();    

      SpriteNormal.SetActive(false);
      SpritePressed.SetActive(true);
      SpriteDisabled.SetActive(false);

      TextNormal.SetActive(false);
      TextPressed.SetActive(true);
    }
    else
    {
      if (SoundUp != null)
        SoundUp.Play();  

      SpriteNormal.SetActive(true);
      SpritePressed.SetActive(false);
      SpriteDisabled.SetActive(false);

      TextNormal.SetActive(true);
      TextPressed.SetActive(false);
    }
  }
}

