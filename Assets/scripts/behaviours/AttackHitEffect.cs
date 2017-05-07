using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Particle effect that plays when bullet reached its destination.
/// </summary>
public class AttackHitEffect : MonoBehaviour 
{	
  public Animator AnimatorComponent;

	void Update() 
	{
    if (AnimatorComponent.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    {
      Destroy(gameObject);
    }
	}
}
