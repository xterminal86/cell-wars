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
  public GameObject CellDefenderPrefab;
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

  Dictionary<int, int> _territoryCountByOwner = new Dictionary<int, int>();
  public Dictionary<int, int> TerritoryCountByOwner
  {
    get { return _territoryCountByOwner; }
  }

  Dictionary<int, int> _scoreCountByOwner = new Dictionary<int, int>();
  public Dictionary<int, int> ScoreCountByOwner
  {
    get { return _scoreCountByOwner; }
  }

  // How many holders influence given cell
  int [,] _locksMap;
  public int[,] LockMap
  {
    get { return _locksMap; }
  }

  CellBehaviour[,] _objectsMap;
  public CellBehaviour[,] ObjectsMap
  {
    get { return _objectsMap; }
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
  public Dictionary<int, CellBehaviour>[,] SoldiersMap;

  public override void Initialize()    
  { 
    _gridHolder = GameObject.Find("grid").transform;

    _locksMap = new int[MapSize, MapSize];
    _objectsMap = new CellBehaviour[MapSize, MapSize];

    SoldiersMap = new Dictionary<int, CellBehaviour>[MapSize, MapSize];

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        SoldiersMap[x, y] = new Dictionary<int, CellBehaviour>();

        _locksMap[x, y] = 0;
        _objectsMap[x, y] = null;

        var go = (GameObject)Instantiate(GridCellPrefab, new Vector3(x, y, 0.0f), Quaternion.identity, _gridHolder);
      }
    }

    // FIXME: ownerID magic numbers

    _buildingsCoordinatesByOwner[0] = new List<Int2>();
    _buildingsCoordinatesByOwner[1] = new List<Int2>();

    _dronesCountByOwner[0] = 0;
    _dronesCountByOwner[1] = 0;

    _scoreCountByOwner[0] = 0;
    _scoreCountByOwner[1] = 0;

    _territoryCountByOwner[0] = 0;
    _territoryCountByOwner[1] = 0;

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

      case GlobalConstants.CellType.DEFENDER:
      
        c = new CellDefender();
        go = (GameObject)Instantiate(CellDefenderPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

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
        _objectsMap[pos.X, pos.Y] = b;
      }
    }

    return c;
  }

  public bool CheckLocationToBuild(Int2 posToCheck, int ownerId)
  {
    if (_objectsMap[posToCheck.X, posToCheck.Y] != null)
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
        if (_objectsMap[x, y] == null)
        {
          continue;
        }

        if (transformedCount >= number)
        {
          return;
        }

        if (_objectsMap[x, y].CellInstance.OwnerId == ownerId
          && _objectsMap[x, y].CellInstance.Type == GlobalConstants.CellType.DRONE)
        {
          transformedCount++;

          _objectsMap[x, y].DestroySelf();
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

  public void SpawnBullet(Vector3 posToSpawn, Vector3 targetPos, CellBehaviour enemy, float bulletSpeed = GlobalConstants.DefaultBulletSpeed)
  {
    GameObject bullet = (GameObject)Instantiate(BulletPrefab, new Vector3(posToSpawn.x, posToSpawn.y, posToSpawn.z), Quaternion.identity, _gridHolder);
    bullet.GetComponent<Bullet>().SetTarget(targetPos, enemy, bulletSpeed);
  }

  void Update()
  {
    // FIXME: ownerID magic numbers

    _dronesCountByOwner[0] = 0;
    _dronesCountByOwner[1] = 0;

    _territoryCountByOwner[0] = 0;
    _territoryCountByOwner[1] = 0;

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        if (_objectsMap[x, y] != null)
        {          
          _territoryCountByOwner[_objectsMap[x, y].CellInstance.OwnerId]++;

          if (_objectsMap[x, y].CellInstance.Type == GlobalConstants.CellType.DRONE && _objectsMap[x, y].CellInstance.OwnerId == 0)
          {
            _dronesCountByOwner[0]++;
          }
          else if (_objectsMap[x, y].CellInstance.Type == GlobalConstants.CellType.DRONE && _objectsMap[x, y].CellInstance.OwnerId == 1)
          {
            _dronesCountByOwner[1]++;
          }
        }
      }
    }
  }
}
