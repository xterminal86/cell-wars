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
    if (CellInstance.Type == GlobalConstants.CellType.DRONE)
    {
      LevelLoader.Instance.DronesCountByOwner[CellInstance.OwnerId]--;
    } else if (CellInstance.Type == GlobalConstants.CellType.SOLDIER)
    {
      LevelLoader.Instance.SoldiersCountByOwner[CellInstance.OwnerId]--;
    } 
    else if (CellInstance.Type == GlobalConstants.CellType.BARRACKS)
    {
      LevelLoader.Instance.BarracksCount--;
    }

    Destroy(gameObject);

    //Debug.Log(LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].CellHere);

    LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].CellHere = null;
  }
}
