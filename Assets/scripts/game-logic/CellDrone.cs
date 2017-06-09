using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drone - occupies grid cell, does nothing. 
/// Used in construction and spawning of other cells.
/// </summary>
public class CellDrone : CellBaseClass 
{
  public CellDrone()
  {    
    Type = GlobalConstants.CellType.DRONE;
    Hitpoints = GlobalConstants.CellDroneHitpoints;
    Priority = GlobalConstants.CellDronePriority;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 3.0f / 4.0f;
    _animationSpeed = 0.2f / _phaseDuration;  
  }

  public override void Update()
  {
    base.Update();

    if (!BehaviourRef.IsDestroying && !FindBaseOrColonyAround())
    {      
      BehaviourRef.DestroySelf();
    }
  }

  bool FindBaseOrColonyAround()
  {    
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          if (LevelLoader.Instance.ObjectsMap[x, y] != null
              && (LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type == GlobalConstants.CellType.COLONY
              || LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type == GlobalConstants.CellType.BASE)
              && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.OwnerId == OwnerId)
          {
            return true;
          }
        }
      }
    }

    return false;
  }
}


