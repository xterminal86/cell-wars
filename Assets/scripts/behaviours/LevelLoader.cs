using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basically holds all information about game.
/// </summary>
public class LevelLoader : MonoSingleton<LevelLoader> 
{  
  public GameObject GridCellPrefab;
  public GameObject CellBasePrefab;
  public GameObject CellColonyPrefab;
  public GameObject CellDronePrefab;
  public GameObject CellBarracksPrefab;
  public GameObject CellHolderPrefab;
  public GameObject CellSoldierPrefab;

  public GameObject BulletPrefab;

  public Material CellMaterial;

  // Holds all objects inside one transform for organizing.
  Transform _gridHolder;
  public Transform GridHolder
  {
    get { return _gridHolder; }
  }

  public int MapSize = 32;

  Dictionary<int, int> _dronesCountByOwner = new Dictionary<int, int>();
  public Dictionary<int, int> DronesCountByOwner
  {
    get { return _dronesCountByOwner; }
  }

  GridCell[,] _map;
  public GridCell[,] Map
  {
    get { return _map; }
  }

  // Used for getting targets for automatic moving for attackers
  Dictionary<int, List<Int2>> _buildingsCoordinatesByOwner = new Dictionary<int, List<Int2>>();
  public Dictionary<int, List<Int2>> BuildingsCoordinatesByOwner
  {
    get { return _buildingsCoordinatesByOwner; }
  }

  // Kinda same as above (basically not used right now)
  Dictionary<int, Int2> _baseCoordinatesByOwner = new Dictionary<int, Int2>();
  public Dictionary<int, Int2> BaseCoordinatesByOwner
  {
    get { return _baseCoordinatesByOwner; }
  }

  // Shows how many attackers occupies given [x,y] cell. 
  // Indiced by attacker's class instance hash code.
  public Dictionary<int, CellBaseClass>[,] SoldiersMap;

  public override void Initialize()    
  { 
    _gridHolder = GameObject.Find("grid").transform;

    _map = new GridCell[MapSize, MapSize];

    SoldiersMap = new Dictionary<int, CellBaseClass>[MapSize, MapSize];

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        SoldiersMap[x, y] = new Dictionary<int, CellBaseClass>();

        _map[x, y] = new GridCell();

        var go = (GameObject)Instantiate(GridCellPrefab, new Vector3(x, y, 0.0f), Quaternion.identity, _gridHolder);

        _map[x, y].Coordinates.X = x;
        _map[x, y].Coordinates.Y = y;
      }
    }

    // FIXME: ownerID magic numbers

    _buildingsCoordinatesByOwner[0] = new List<Int2>();
    _buildingsCoordinatesByOwner[1] = new List<Int2>();

    _dronesCountByOwner[0] = 0;
    _dronesCountByOwner[1] = 0;

    Vector3 cameraPos = new Vector3((float)MapSize / 2.0f - 0.5f, (float)MapSize / 2.0f - 0.5f, Camera.main.transform.position.z);

    Camera.main.transform.position = cameraPos;

    _baseCoordinatesByOwner[0] = new Int2(1, 1);
    _baseCoordinatesByOwner[1] = new Int2(MapSize - 2, MapSize - 2);

    PlaceCell(_baseCoordinatesByOwner[0], GlobalConstants.CellType.BASE, 0);
    PlaceCell(_baseCoordinatesByOwner[1], GlobalConstants.CellType.BASE, 1);
  }    

  /// <summary>
  /// Places the given type of cell on a map.
  /// </summary>
  /// <returns>The cell instance or null</returns>
  /// <param name="pos">Position to place at</param>
  /// <param name="cellType">Cell type</param>
  /// <param name="ownerId">Owner (player) identifier</param>
  public CellBaseClass PlaceCell(Int2 pos, GlobalConstants.CellType cellType, int ownerId)
  {
    CellBaseClass c = null;
    GameObject go = null;

    switch (cellType)
    {
      case GlobalConstants.CellType.BASE:
        
        c = new CellBase();
        go = (GameObject)Instantiate(CellBasePrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.DRONE:
        
        c = new CellDrone();
        go = (GameObject)Instantiate(CellDronePrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.COLONY:
        
        c = new CellColony();
        go = (GameObject)Instantiate(CellColonyPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.BARRACKS:
        
        c = new CellBarracks();
        go = (GameObject)Instantiate(CellBarracksPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.SOLDIER:
        
        c = new CellSoldier();
        go = (GameObject)Instantiate(CellSoldierPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.HOLDER:
        
        c = new CellHolder();
        go = (GameObject)Instantiate(CellHolderPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

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

      if (c.Type != GlobalConstants.CellType.SOLDIER)
      {
        _map[pos.X, pos.Y].CellHere = c;
      }
    }

    return c;
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
        TransformDrones(GlobalConstants.CellColonyHitpoints, ownerId);
        PlaceCell(pos, type, ownerId);
        break;

      case GlobalConstants.CellType.BARRACKS:
        TransformDrones(GlobalConstants.CellBarracksHitpoints, ownerId);
        PlaceCell(pos, type, ownerId);
        break;

      case GlobalConstants.CellType.HOLDER:
        TransformDrones(GlobalConstants.CellHolderHitpoints, ownerId);
        PlaceCell(pos, type, ownerId);
        break;
    }
  }

  /// <summary>
  /// Destroys number of drones of ownerId player.
  /// </summary>
  /// <param name="number">Number of drones to destroy</param>
  /// <param name="ownerId">Owner (player) identifier</param>
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

    DronesCountByOwner[ownerId] -= number;
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

  // TODO: stub
  public void GameOver(int loserId)
  {
    Time.timeScale = 0.0f;
  }

  public void SpawnBullet(Vector3 posToSpawn, Vector3 targetPos, CellBaseClass enemy)
  {
    GameObject bullet = (GameObject)Instantiate(BulletPrefab, new Vector3(posToSpawn.x, posToSpawn.y, posToSpawn.z), Quaternion.identity, _gridHolder);
    bullet.GetComponent<Bullet>().SetTarget(targetPos, enemy);
  }

  void Update()
  {
    // FIXME: ownerID magic numbers

    _dronesCountByOwner[0] = 0;
    _dronesCountByOwner[1] = 0;

    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {
        if (LevelLoader.Instance.Map[x, y].CellHere != null)
        {          
          if (LevelLoader.Instance.Map[x, y].CellHere.Type == GlobalConstants.CellType.DRONE && LevelLoader.Instance.Map[x, y].CellHere.OwnerId == 0)
          {
            _dronesCountByOwner[0]++;
          }
          else if (LevelLoader.Instance.Map[x, y].CellHere.Type == GlobalConstants.CellType.DRONE && LevelLoader.Instance.Map[x, y].CellHere.OwnerId == 1)
          {
            _dronesCountByOwner[1]++;
          }
        }
      }
    }
  }
}
