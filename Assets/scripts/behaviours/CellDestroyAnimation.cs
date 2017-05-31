using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellDestroyAnimation : MonoBehaviour 
{  
  public void ShrinkObject()
  {    
    StartCoroutine(ShrinkObjectRoutine());
  }

  Vector3 _scale = Vector3.one;
  IEnumerator ShrinkObjectRoutine()
  {
    float timer = 1.0f;
    while (timer > 0.0f)
    {
      timer -= Time.smoothDeltaTime * 2.0f;

      _scale.Set(timer, timer, timer);

      transform.localScale = _scale;

      yield return null;
    }

    _scale.Set(0.0f, 0.0f, 0.0f);

    transform.localScale = _scale;

    Destroy(gameObject);

    yield return null;
  }
}
