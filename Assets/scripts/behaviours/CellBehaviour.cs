using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviour : MonoBehaviour 
{
  public CellBaseClass CellInstance;
  	
	void Update () 
	{
    CellInstance.Update();
	}

  public void DestroySelf()
  {
    Destroy(gameObject);

    //Debug.Log(LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].CellHere);

    LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].CellHere = null;
  }
}
