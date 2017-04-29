using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedCellMarker : MonoBehaviour 
{
  Material _m;
  void Start()
  {
    _m = GetComponent<MeshRenderer>().material;
  }

  Color _color = Color.red;
  float _counter = 0.01f;
	void Update () 
	{
    _counter += Time.smoothDeltaTime;

    if (_counter < 1.0f)
    {
      _color.r -= Time.smoothDeltaTime;
    }
    else
    {
      _color.r += Time.smoothDeltaTime;
    }

    _m.color = _color;

    if (_counter > 2.0f)
    {
      _counter = 0.0f;
    }
	}
}
