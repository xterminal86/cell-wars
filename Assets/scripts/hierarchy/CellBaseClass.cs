using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBaseClass
{
  public int Hitpoints = 3;

  public int OwnerId = -1;

  public Int2 Coordinates = Int2.Zero;
  public Vector3 WorldCoordinates = Vector3.zero;

  public GlobalConstants.CellType Type;

  public CellBehaviour BehaviourRef;
  public Transform ModelTransform;

  public virtual void Update()
  {
    if (BehaviourRef != null)
    {
      WorldCoordinates.Set(BehaviourRef.transform.position.x, BehaviourRef.transform.position.y, 0.0f);
    }
  }

  protected int _enemyId = 0;
  public virtual void InitBehaviour()
  {
    WorldCoordinates.Set(Coordinates.X, Coordinates.Y, 0.0f);

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

    if (Hitpoints <= 0 && BehaviourRef != null)
    {      
      BehaviourRef.DestroySelf();
    }
  }

  protected CellBaseClass SpawnCell(GlobalConstants.CellType cellType)
  {
    /*
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
          bool cellEmpty = true;

          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {
            if (kvp.Value != null && kvp.Value.OwnerId != OwnerId)
            {
              cellEmpty = false;
              break;
            }
          }
          
          if (LevelLoader.Instance.Map[x, y].CellHere != null)
          {            
            cellEmpty = false;
          }

          if (cellEmpty)
          {
            _pos.Set(x, y);

            return LevelLoader.Instance.PlaceCell(_pos, cellType, OwnerId);
          }
        }
      }
    }

    return null;
    */

    return LevelLoader.Instance.PlaceCell(_emptyCellPos, cellType, OwnerId);
  }

  Int2 _emptyCellPos = Int2.Zero;
  protected Int2 TryToFindEmptyCell()
  {
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    bool cellEmpty = true;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
          && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          cellEmpty = false;

          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {
            if (kvp.Value == null || (kvp.Value != null && kvp.Value.OwnerId == OwnerId))
            {              
              cellEmpty = true;
              break;
            }
          }

          if (LevelLoader.Instance.Map[x, y].CellHere == null)
          { 
            cellEmpty = true;
          }

          if (cellEmpty)
          {
            _emptyCellPos.Set(x, y);
            goto exitLoop;
          }
        }
      }
    }

  exitLoop:

    return (cellEmpty ? _emptyCellPos : null);
  }
}
