using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to every cell prefab.
/// </summary>
public class CellBehaviour : MonoBehaviour 
{
  public CellBaseClass CellInstance;

  public Transform RadiusMarker;
  public TextMesh HpText;
  public Transform ModelTransform;

	void Update() 
	{
    HpText.text = CellInstance.Hitpoints.ToString();

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
        LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
        break;
      case GlobalConstants.CellType.HOLDER:
        (CellInstance as CellHolder).UnlockCells();
        LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
        break;        
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
