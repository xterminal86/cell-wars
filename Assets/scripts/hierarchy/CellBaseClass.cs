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

  public virtual void InitBehaviour()
  {    
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
  protected void SpawnCell(GlobalConstants.CellType cellType)
  {
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x <= LevelLoader.Instance.MapSize
          && y >= 0 && y <= LevelLoader.Instance.MapSize)
        {
          if (LevelLoader.Instance.Map[x, y].SoldierHere == null && LevelLoader.Instance.Map[x, y].CellHere == null)
          {            
            _pos.Set(x, y);

            LevelLoader.Instance.PlaceCell(_pos, cellType, OwnerId);
            return;
          }
        }
      }
    }
  }
}
