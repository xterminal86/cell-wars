using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBaseClass
{
  public int Hitpoints = 3;

  public int OwnerId = -1;

  public Int2 Coordinates = Int2.Zero;

  public GlobalConstants.CellType Type;

  public CellBehaviour BehaviourRef;
  public Transform ModelTransform;

  public virtual void Update()
  {
  }

  protected int _enemyId = 0;
  public virtual void InitBehaviour()
  {
    // Find enemy ID

    foreach (var b in LevelLoader.Instance.BaseCoordinatesByOwner)
    {
      if (OwnerId != b.Key)
      {
        _enemyId = b.Key;
        break;
      }
    }
  }

  public void ReceiveDamage(int amount)
  {
    Hitpoints -= amount;

    if (Hitpoints <= 0)
    {
      BehaviourRef.DestroySelf();
    }
  }

  Int2 _pos = Int2.Zero;
  protected CellBaseClass TryToSpawnCell(GlobalConstants.CellType cellType)
  {    
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x <= LevelLoader.Instance.MapSize - 1
          && y >= 0 && y <= LevelLoader.Instance.MapSize - 1)
        {
          if (LevelLoader.Instance.Map[x, y].SoldiersByOwnerHere[_enemyId].Count == 0 && LevelLoader.Instance.Map[x, y].CellHere == null)
          {            
            _pos.Set(x, y);

            return LevelLoader.Instance.PlaceCell(_pos, cellType, OwnerId);
          }
        }
      }
    }

    return null;
  }
}
