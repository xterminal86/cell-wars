using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Particle effect that plays when bullet reached its destination.
/// </summary>
public class AttackHitEffect : MonoBehaviour 
{	
  float _timer = 0.0f;
	void Update () 
	{
    if (_timer > 2.0f)
    {
      Destroy(gameObject);
      return;
    }

    _timer += Time.smoothDeltaTime;
	}
}
