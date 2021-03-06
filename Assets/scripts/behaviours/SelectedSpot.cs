﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedSpot : MonoBehaviour 
{
  Material _material;
  void Start()
  {
    _material = GetComponent<Renderer>().material;
  }

  public void SetColor(Color c)
  {
    _material.color = c;
  }

  Vector3 _scale = Vector3.one;
  float _counter = 0.0f;
	void Update () 
  {
    _counter += Time.smoothDeltaTime;

    if ((_counter > 0.0f && _counter < 0.25f) 
     || (_counter > 0.75f && _counter < 1.0f))
    {
      _scale.x -= Time.smoothDeltaTime;
      _scale.y -= Time.smoothDeltaTime;
      _scale.z -= Time.smoothDeltaTime;
    }
    else if ((_counter > 0.25f && _counter < 0.5f) 
          || (_counter > 0.5f && _counter < 0.75f))
    {
      _scale.x += Time.smoothDeltaTime;
      _scale.y += Time.smoothDeltaTime;
      _scale.z += Time.smoothDeltaTime;
    }

    if (_counter > 1.0f)
    {
      _counter = 0.0f;
    }

    _scale.x = Mathf.Clamp(_scale.x, 0.5f, 1.3f);
    _scale.y = Mathf.Clamp(_scale.y, 0.5f, 1.3f);
    _scale.z = Mathf.Clamp(_scale.z, 0.5f, 1.3f);

    transform.localScale = _scale;
	}

  void OnEnable()
  {
    _scale.Set(1.0f, 1.0f, 1.0f);

    _counter = 0.0f;
  }
}
