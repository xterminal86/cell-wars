using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBase : CellBaseClass
{
  float _timer = 0.0f;
  public override void Update()
  {
    if (_timer > GlobalConstants.CellBaseSpawnTimeoutSeconds)
    {
      _timer = 0.0f;
      SpawnDrone();
    }

    _timer += Time.smoothDeltaTime;
  }

  void SpawnDrone()
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
          if (LevelLoader.Instance.Map[x, y].CellHere == null)
          {
            LevelLoader.Instance.PlaceCell(x, y, GlobalConstants.CellType.DRONE);
            return;
          }
        }
      }
    }
  }
}
