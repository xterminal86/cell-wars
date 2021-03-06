﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInstantiated : MonoBehaviour 
{	
  public TestClass TestClassRef = new TestClass();

  float _timer = 0.0f;
	void Update () 
  {
    _timer += Time.smoothDeltaTime;
		
    if (_timer > 10.0f)
    {
      Debug.Log(string.Format("Destroying (before Destroy()), behaviour: [{0}], inner reference: [{1}]", this, TestClassRef));
      Destroy(gameObject);
      TestClassRef = null;
      Debug.Log(string.Format("Destroying (after Destroy()), behaviour: [{0}], inner reference: [{1}]", this, TestClassRef));
    }
	}
}
