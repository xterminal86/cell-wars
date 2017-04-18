using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoSingleton<LevelLoader> 
{
  public GameObject GridCellPrefab;
  public GameObject CellBasePrefab;
  public GameObject CellDronePrefab;

  Transform _gridHolder;

  public readonly int MapSize = 16;

  GridCell[,] _map;
  public GridCell[,] Map
  {
    get { return _map; }
  }

  public override void Initialize()    
  {    
    _gridHolder = GameObject.Find("grid").transform;

    _map = new GridCell[MapSize, MapSize];

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        _map[x, y] = new GridCell();

        var go = (GameObject)Instantiate(GridCellPrefab, new Vector3(x, y, 0.0f), Quaternion.identity, _gridHolder);

        _map[x, y].Coordinates.X = x;
        _map[x, y].Coordinates.Y = y;
      }
    }

    Vector3 cameraPos = new Vector3((float)MapSize / 2.0f - 0.5f, (float)MapSize / 2.0f - 0.5f, Camera.main.transform.position.z);

    Camera.main.transform.position = cameraPos;

    PlaceCell(1, 1, GlobalConstants.CellType.BASE);
  }    

  public void PlaceCell(int x, int y, GlobalConstants.CellType cellType)
  {
    CellBaseClass c = null;
    GameObject go = null;

    switch (cellType)
    {
      case GlobalConstants.CellType.BASE:
        c = new CellBase();

        c.Type = GlobalConstants.CellType.BASE;

        go = (GameObject)Instantiate(CellBasePrefab, new Vector3(x, y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.DRONE:
        c = new CellDrone();

        c.Type = GlobalConstants.CellType.DRONE;

        go = (GameObject)Instantiate(CellDronePrefab, new Vector3(x, y, 0.0f), Quaternion.identity, _gridHolder);

        break;
    }

    if (c != null)
    {      
      c.OwnerId = 0;

      c.Coordinates.X = x;
      c.Coordinates.Y = y;

      CellBehaviour b = go.GetComponent<CellBehaviour>();
      b.CellInstance = c;

      _map[x, y].CellHere = c;
    }
  }
}
