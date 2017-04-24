using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviour : MonoBehaviour 
{
  public CellBaseClass CellInstance;
  	
  public TextMesh HpText;
  public Transform ModelTransform;

	void Update () 
	{
    HpText.text = CellInstance.Hitpoints.ToString();

    CellInstance.Update();
	}

  public void DestroySelf()
  {    
    if (CellInstance.Type == GlobalConstants.CellType.DRONE)
    {
      LevelLoader.Instance.DronesCountByOwner[CellInstance.OwnerId]--;
    }
    else if (CellInstance.Type == GlobalConstants.CellType.SOLDIER)
    {
      LevelLoader.Instance.SoldiersCountByOwner[CellInstance.OwnerId]--;
      //LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].SoldierHere = null;
      LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].SoldiersByOwnerHere[CellInstance.OwnerId].Dequeue();
    }
    else if (CellInstance.Type == GlobalConstants.CellType.BARRACKS)
    {
      LevelLoader.Instance.BarracksCountByOwner[CellInstance.OwnerId]--;

      LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
    }
    else if (CellInstance.Type == GlobalConstants.CellType.COLONY)
    {
      LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
    }

    if (CellInstance.Type != GlobalConstants.CellType.SOLDIER)
    {
      LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].CellHere = null;
    }

    Destroy(gameObject);

    if (CellInstance.Type == GlobalConstants.CellType.BASE)
    {
      LevelLoader.Instance.GameOver(CellInstance.OwnerId);    
    }
  }
}
