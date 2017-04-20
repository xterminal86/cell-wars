using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoSingleton<LevelLoader> 
{
  public GameObject GridCellPrefab;
  public GameObject CellBasePrefab;
  public GameObject CellColonyPrefab;
  public GameObject CellDronePrefab;

  Transform _gridHolder;

  public readonly int MapSize = 16;

  int _dronesCount = 0;
  public int DronesCount
  {
    get { return _dronesCount; }
  }

  GridCell[,] _map;
  public GridCell[,] Map
  {
    get { return _map; }
  }

  List<Int2> _coloniesBuiltCoordinates = new List<Int2>();

  public readonly Int2 BaseCoordinates = new Int2(1, 1);

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

    PlaceCell(BaseCoordinates, GlobalConstants.CellType.BASE);
  }    

  public void PlaceCell(Int2 pos, GlobalConstants.CellType cellType)
  {
    CellBaseClass c = null;
    GameObject go = null;

    switch (cellType)
    {
      case GlobalConstants.CellType.BASE:
        c = new CellBase();

        c.Type = GlobalConstants.CellType.BASE;

        go = (GameObject)Instantiate(CellBasePrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.DRONE:
        c = new CellDrone();

        c.Type = GlobalConstants.CellType.DRONE;

        go = (GameObject)Instantiate(CellDronePrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        _dronesCount++;

        break;

      case GlobalConstants.CellType.COLONY:
        c = new CellColony();

        c.Type = GlobalConstants.CellType.COLONY;

        go = (GameObject)Instantiate(CellColonyPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;
    }

    if (c != null)
    {      
      if (c.Type != GlobalConstants.CellType.DRONE && c.Type != GlobalConstants.CellType.SOLDIER)
      {
        _coloniesBuiltCoordinates.Add(new Int2(pos));
      }

      c.OwnerId = 0;

      c.Coordinates.Set(pos);

      CellBehaviour b = go.GetComponent<CellBehaviour>();
      b.CellInstance = c;

      _map[pos.X, pos.Y].CellHere = c;
      _map[pos.X, pos.Y].CellBehaviourHere = go;
    }
  }

  public bool CheckLocationToBuild(Int2 posToCheck)
  {
    if (_map[posToCheck.X, posToCheck.Y].CellHere != null)
    {
      return false;
    }

    int d = 0;

    int checkCounter = 0;
    foreach (var item in _coloniesBuiltCoordinates)
    {
      d = Utils.BlockDistance(item, posToCheck);

      if (d <= GlobalConstants.BuildRangeDistance)
      {
        checkCounter++;
      }
    }

    return (checkCounter != 0);
  }

  public void Build(Int2 pos, GlobalConstants.CellType type)
  {
    switch (type)
    {
      case GlobalConstants.CellType.COLONY:
        TransformDrones(GlobalConstants.ColonyDronesCost);
        PlaceCell(pos, type);
        break;
    }
  }

  void TransformDrones(int number)
  {
    int transformedCount = 0;
    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        if (_map[x, y].CellHere == null)
        {
          continue;
        }

        if (transformedCount > number)
        {
          return;
        }

        if (_map[x, y].CellHere.OwnerId == 0
         && _map[x, y].CellHere.Type == GlobalConstants.CellType.DRONE)
        {
          transformedCount++;

          _dronesCount--;

          Destroy(_map[x, y].CellBehaviourHere);

          _map[x, y].CellBehaviourHere = null;
          _map[x, y].CellHere = null;
        }
      }
    }
  }
}
