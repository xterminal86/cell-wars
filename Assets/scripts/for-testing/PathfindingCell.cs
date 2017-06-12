using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingCell : MonoBehaviour 
{
  public TextMesh TextMapOverlay;
  public TextMesh TextPathOverlay;

  public void SetMapOverlay(string text)
  {
    TextMapOverlay.text = text;
  }

  public void SetPathOverlay(string text)
  {
    TextPathOverlay.text = text;
  }

  void Update()
  {
    if (Input.GetKey(KeyCode.Tab))
    {
      TextMapOverlay.gameObject.SetActive(false);
      TextPathOverlay.gameObject.SetActive(true);
    }
    else
    {
      TextMapOverlay.gameObject.SetActive(true);
      TextPathOverlay.gameObject.SetActive(false);
    }
  }
}
