using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for cells logic.
/// </summary>
public abstract class CellBaseClass
{
  public int Hitpoints = 3;

  public int OwnerId = -1;

  public Int2 Coordinates = Int2.Zero;
  public Vector3 WorldCoordinates = Vector3.zero;

  public GlobalConstants.CellType Type;

  // Reference to a behaviour for this cell (also used for translation)
  public CellBehaviour BehaviourRef;

  // Reference to a model for various effects (mainly rotation). 
  // Usually it's child of a game object. Because we use orthographic projection,
  // to deal with Z depth for all objects on a scene, we can adjust Z value of a child object in a prefab.
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

          if (LevelLoader.Instance.Map[x, y].CellHere == null && LevelLoader.Instance.Map[x, y].NumberOfLocks == 0)
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
