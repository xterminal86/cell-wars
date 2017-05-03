﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellHolder : CellBaseClass
{
  public CellHolder()
  {
    Type = GlobalConstants.CellType.HOLDER;
    Hitpoints = GlobalConstants.CellHolderHitpoints;
  }

  public override void InitBehaviour()
  {
    Vector3 newScale = new Vector3(GlobalConstants.CellHolderRange * 2, GlobalConstants.CellHolderRange * 2, GlobalConstants.CellHolderRange * 2);
    BehaviourRef.RadiusMarker.localScale = newScale;

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
          // Destroy all drones around
          if (LevelLoader.Instance.Map[x, y].CellHere != null
            && LevelLoader.Instance.Map[x, y].CellHere.Type == GlobalConstants.CellType.DRONE)
          {
            LevelLoader.Instance.Map[x, y].CellHere.BehaviourRef.DestroySelf();
          }

          LevelLoader.Instance.Map[x, y].NumberOfLocks++;
        }
      }
    }
  }

  public override void Update()
  {
    base.Update();

    ModelTransform.Rotate(Vector3.forward, Time.smoothDeltaTime * 10.0f);
  }

  public void UnlockCells()
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
          LevelLoader.Instance.Map[x, y].NumberOfLocks--;
          LevelLoader.Instance.Map[x, y].NumberOfLocks = Mathf.Clamp(LevelLoader.Instance.Map[x, y].NumberOfLocks, 0, int.MaxValue);
        }
      }
    }
  }
}
