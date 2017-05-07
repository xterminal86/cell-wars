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
    base.InitBehaviour();

    _phaseDuration = 1.0f;
    _animationSpeed = 0.1f;
    _zRotationSpeed = 5.0f;

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
          if (LevelLoader.Instance.ObjectsMap[x, y] != null
           && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type == GlobalConstants.CellType.DRONE)
          {
            LevelLoader.Instance.ObjectsMap[x, y].DestroySelf();
          }

          /*
          // Destroy all drones around
          if (LevelLoader.Instance.Map[x, y].CellHere != null
            && LevelLoader.Instance.Map[x, y].CellHere.Type == GlobalConstants.CellType.DRONE)
          {
            LevelLoader.Instance.Map[x, y].CellHere.BehaviourRef.DestroySelf();
          }
          */

          LevelLoader.Instance.Map[x, y].NumberOfLocks++;
        }
      }
    }
  }

  public override void Update()
  {
    base.Update();

    PlayAnimation();
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