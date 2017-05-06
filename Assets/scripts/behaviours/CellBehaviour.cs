using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to every cell prefab.
/// </summary>
public class CellBehaviour : MonoBehaviour 
{
  public CellBaseClass CellInstance;

  public Transform RadiusMarker;
  public Transform ModelTransform;

  public Image HitpointsBar;

	void Update() 
	{
    if (CellInstance.Type != GlobalConstants.CellType.DRONE)
    { 
      CellInstance.CalculateHitpointsBar(GlobalConstants.CellHitpointsByType[CellInstance.Type]);
    }

    CellInstance.Update();
	}

  public void DestroySelf()
  { 
    switch (CellInstance.Type)
    {
      case GlobalConstants.CellType.SOLDIER:
        (CellInstance as CellSoldier).DelistFromBarracks();
        LevelLoader.Instance.SoldiersMap[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].Remove(CellInstance.GetHashCode());
        break;

      case GlobalConstants.CellType.BARRACKS:
      case GlobalConstants.CellType.COLONY:
      case GlobalConstants.CellType.DEFENDER:
        LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
        break;
      case GlobalConstants.CellType.HOLDER:
        (CellInstance as CellHolder).UnlockCells();
        LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
        break;        
    }

    Destroy(gameObject);

    if (CellInstance.Type == GlobalConstants.CellType.BASE)
    {
      LevelLoader.Instance.GameOver(CellInstance.OwnerId);    
    }

    if (CellInstance.Type != GlobalConstants.CellType.SOLDIER)
    {
      LevelLoader.Instance.Map[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].CellHere = null;
    }

    CellInstance = null;
  }
}
