using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoSingleton<LevelLoader> 
{  
  public GameObject GridCellPrefab;
  public GameObject CellBasePrefab;
  public GameObject CellColonyPrefab;
  public GameObject CellDronePrefab;
  public GameObject CellBarracksPrefab;
  public GameObject CellSoldierPrefab;

  public GameObject BulletPrefab;

  public Material CellMaterial;

  Transform _gridHolder;
  public Transform GridHolder
  {
    get { return _gridHolder; }
  }

  public int MapSize = 32;

  Dictionary<int, int> _barracksCountByOwner = new Dictionary<int, int>();
  public Dictionary<int, int> BarracksCountByOwner
  {
    get { return _barracksCountByOwner; }
  }

  Dictionary<int, int> _dronesCountByOwner = new Dictionary<int, int>();
  public Dictionary<int, int> DronesCountByOwner
  {
    get { return _dronesCountByOwner; }
  }

  Dictionary<int, int> _soldiersCountByOwner = new Dictionary<int, int>();
  public Dictionary<int, int> SoldiersCountByOwner
  {
    get { return _soldiersCountByOwner; }
  }

  GridCell[,] _map;
  public GridCell[,] Map
  {
    get { return _map; }
  }

  Dictionary<int, List<Int2>> _buildingsCoordinatesByOwner = new Dictionary<int, List<Int2>>();
  public Dictionary<int, List<Int2>> BuildingsCoordinatesByOwner
  {
    get { return _buildingsCoordinatesByOwner; }
  }

  Dictionary<int, Int2> _baseCoordinatesByOwner = new Dictionary<int, Int2>();
  public Dictionary<int, Int2> BaseCoordinatesByOwner
  {
    get { return _baseCoordinatesByOwner; }
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

    // FIXME: ownerID magic numbers

    _buildingsCoordinatesByOwner[0] = new List<Int2>();
    _buildingsCoordinatesByOwner[1] = new List<Int2>();

    _barracksCountByOwner[0] = 0;
    _barracksCountByOwner[1] = 0;

    _dronesCountByOwner[0] = 0;
    _dronesCountByOwner[1] = 0;

    _soldiersCountByOwner[0] = 0;
    _soldiersCountByOwner[1] = 0;

    Vector3 cameraPos = new Vector3((float)MapSize / 2.0f - 0.5f, (float)MapSize / 2.0f - 0.5f, Camera.main.transform.position.z);

    Camera.main.transform.position = cameraPos;

    _baseCoordinatesByOwner[0] = new Int2(1, 1);
    _baseCoordinatesByOwner[1] = new Int2(MapSize - 2, MapSize - 2);

    PlaceCell(_baseCoordinatesByOwner[0], GlobalConstants.CellType.BASE, 0);
    PlaceCell(_baseCoordinatesByOwner[1], GlobalConstants.CellType.BASE, 1);
  }    

  public void PlaceCell(Int2 pos, GlobalConstants.CellType cellType, int ownerId)
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

        _dronesCountByOwner[ownerId]++;

        break;

      case GlobalConstants.CellType.COLONY:
        c = new CellColony();

        c.Type = GlobalConstants.CellType.COLONY;

        go = (GameObject)Instantiate(CellColonyPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.BARRACKS:
        c = new CellBarracks();

        c.Type = GlobalConstants.CellType.BARRACKS;

        go = (GameObject)Instantiate(CellBarracksPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        _barracksCountByOwner[ownerId]++;

        break;

      case GlobalConstants.CellType.SOLDIER:
        c = new CellSoldier();

        c.Type = GlobalConstants.CellType.SOLDIER;

        go = (GameObject)Instantiate(CellSoldierPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        _soldiersCountByOwner[ownerId]++;

        break;
    }

    if (c != null)
    { 
      Material m = new Material(CellMaterial);
      m.color = GlobalConstants.ColorsList[ownerId][c.Type];

      go.GetComponentInChildren<Renderer>().material = m;

      if (c.Type != GlobalConstants.CellType.DRONE && c.Type != GlobalConstants.CellType.SOLDIER)
      {
        _buildingsCoordinatesByOwner[ownerId].Add(new Int2(pos));
      }

      c.OwnerId = ownerId;

      c.Coordinates.Set(pos);

      CellBehaviour b = go.GetComponent<CellBehaviour>();
      b.CellInstance = c;
      b.CellInstance.BehaviourRef = b;
      b.CellInstance.ModelTransform = b.ModelTransform;
      b.CellInstance.InitBehaviour();

      _map[pos.X, pos.Y].CellHere = c;
    }
  }

  public bool CheckLocationToBuild(Int2 posToCheck, int ownerId)
  {
    if (_map[posToCheck.X, posToCheck.Y].CellHere != null)
    {
      return false;
    }

    int d = 0;

    int checkCounter = 0;
    foreach (var item in _buildingsCoordinatesByOwner[ownerId])
    {
      d = Utils.BlockDistance(item, posToCheck);

      if (d <= GlobalConstants.BuildRangeDistance)
      {
        checkCounter++;
      }
    }

    return (checkCounter != 0);
  }

  public void Build(Int2 pos, GlobalConstants.CellType type, int ownerId)
  {
    switch (type)
    {
      case GlobalConstants.CellType.COLONY:
        TransformDrones(GlobalConstants.ColonyDronesCost, ownerId);
        PlaceCell(pos, type, ownerId);
        break;

      case GlobalConstants.CellType.BARRACKS:
        TransformDrones(GlobalConstants.BarracksDronesCost, ownerId);
        PlaceCell(pos, type, ownerId);
        break;
    }
  }

  public void TransformDrones(int number, int ownerId)
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

        if (transformedCount >= number)
        {
          return;
        }

        if (_map[x, y].CellHere.OwnerId == ownerId
         && _map[x, y].CellHere.Type == GlobalConstants.CellType.DRONE)
        {
          transformedCount++;

          _map[x, y].CellHere.BehaviourRef.DestroySelf();
        }
      }
    }
  }

  public void RemoveBuildingFromDictionary(int ownerId, Int2 pos)
  {
    for (int i = 0; i < _buildingsCoordinatesByOwner[ownerId].Count; i++)
    {
      if (_buildingsCoordinatesByOwner[ownerId][i].X == pos.X 
       && _buildingsCoordinatesByOwner[ownerId][i].Y == pos.Y)
      {
        _buildingsCoordinatesByOwner[ownerId].Remove(_buildingsCoordinatesByOwner[ownerId][i]);
        return;
      }
    }
  }

  public void GameOver(int loserId)
  {
    Time.timeScale = 0.0f;
  }

  public void SpawnBullet(Vector3 posToSpawn, Vector3 targetPos)
  {
    GameObject bullet = (GameObject)Instantiate(BulletPrefab, new Vector3(posToSpawn.x, posToSpawn.y, posToSpawn.z), Quaternion.identity, _gridHolder);
    bullet.GetComponent<Bullet>().SetTarget(targetPos);
  }
}
