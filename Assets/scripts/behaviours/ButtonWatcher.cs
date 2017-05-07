using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonWatcher : MonoBehaviour 
{
  public Button ButtonComponent;

  public GameObject TextNormal;
  public GameObject TextPressed;

  bool _pressed = false;
  bool _pointerExited = true;

  public void OnPointerDownHandler()
  {    
    if (ButtonComponent.interactable)
    {
      _pressed = true;
    }
  }

  public void OnPointerUpHandler()
  {
    _pressed = false;
  }

  public void OnPointerExitHandler()
  {
    _pointerExited = true;
  }

  public void OnPointerEnterHandler()
  {
    _pointerExited = false;
  }

  void Update()
  {    
    if (_pressed && !_pointerExited)
    {
      TextNormal.SetActive(false);
      TextPressed.SetActive(true);
    }
    else
    {
      TextNormal.SetActive(true);
      TextPressed.SetActive(false);
    }
  }
}

