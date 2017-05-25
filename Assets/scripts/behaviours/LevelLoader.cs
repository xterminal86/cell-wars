using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Basically holds all information about game.
/// </summary>
public class LevelLoader : MonoSingleton<LevelLoader> 
{  
  public GameObject GridCellFullPrefab;
  public GameObject GridCellCornerPrefab;
  public GameObject GridCellTPrefab;

  public GameObject CellBasePrefab;
  public GameObject CellColonyPrefab;
  public GameObject CellDronePrefab;
  public GameObject CellBarracksPrefab;
  public GameObject CellHolderPrefab;
  public GameObject CellDefenderPrefab;
  public GameObject CellSoldierPrefab;
  public GameObject CellWallPrefab;

  public GameObject BulletPrefab;

  public Material CellMaterial;
  public Material TerritoryOverlayMaterial;

  public GameObject TerritoryOverlayPrefab;

  public RectTransform GameOverWindow;
  public Text GameOverTitleText;
  public Text PlayerTerritoryText;
  public Text CPUTerritoryText;
  public Text PlayerScoreText;
  public Text CPUScoreText;

  // Holds all objects inside one transform for organizing.
  Transform _gridHolder;
  public Transform GridHolder
  {
    get { return _gridHolder; }
  }

  Transform _territoryOverlayHolder;

  GameObject[,] _territoryOverlay;

  public int MapSize = 32;

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

  [HideInInspector]
  public bool IsGameOver = false;

  public override void Initialize()    
  { 
    System.GC.Collect();

    GameOverWindow.gameObject.SetActive(false);

    IsGameOver = false;

    _gridHolder = GameObject.Find("grid").transform;
    _territoryOverlayHolder = GameObject.Find("territory-overlay-holder").transform;

    _locksMap = new int[MapSize, MapSize];
    _objectsMap = new CellBehaviour[MapSize, MapSize];
    _territoryOverlay = new GameObject[MapSize, MapSize];

    SoldiersMap = new Dictionary<int, CellBehaviour>[MapSize, MapSize];

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        SoldiersMap[x, y] = new Dictionary<int, CellBehaviour>();

        _locksMap[x, y] = 0;
        _objectsMap[x, y] = null;

        InstantiateCellPrefab(x, y);

        GameObject go = (GameObject)Instantiate(TerritoryOverlayPrefab, new Vector3(x, y, -2.0f), Quaternion.identity, _territoryOverlayHolder);
        Material m = new Material(TerritoryOverlayMaterial);
        m.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        go.GetComponent<Renderer>().material = m;
        _territoryOverlay[x, y] = go;
      }
    }

    // FIXME: ownerID magic numbers

    _buildingsCoordinatesByOwner[0] = new List<Int2>();
    _buildingsCoordinatesByOwner[1] = new List<Int2>();

    _dronesCountByOwner[0] = 0;
    _dronesCountByOwner[1] = 0;

    _soldiersCountByOwner[0] = 0;
    _soldiersCountByOwner[1] = 0;

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

    PlaceRandomWalls(60);   
  }    

  void InstantiateCellPrefab(int x, int y)
  {
    int lx = x - 1;
    int ly = y - 1;
    int hx = x + 1;
    int hy = y + 1;

    Vector3 eulerAngles = Vector3.zero;

    GameObject prefabToInstantiate = null;

    // Corner, T shaped and full cells

    if ((lx < 0 && ly < 0)
        || (hx > MapSize - 1 && ly < 0)
        || (hx > MapSize - 1 && hy > MapSize - 1)
        || (lx < 0 && hy > MapSize - 1))
    {
      prefabToInstantiate = GridCellCornerPrefab;

      if (lx < 0 && ly < 0)
      {
        eulerAngles.z = 0.0f;
      }
      else if (hx > MapSize - 1 && ly < 0)
      {
        eulerAngles.z = 90.0f;
      }
      else if (hx > MapSize - 1 && hy > MapSize - 1)
      {
        eulerAngles.z = 180.0f;
      }
      else if (lx < 0 && hy > MapSize - 1)
      {
        eulerAngles.z = 270.0f;
      }
    }
    else if (ly < 0 || hx > MapSize - 1 || hy > MapSize - 1 || lx < 0)
    {
      prefabToInstantiate = GridCellTPrefab;

      if (ly < 0)
      {
        eulerAngles.z = 0.0f;
      }
      else if (hx > MapSize - 1)
      {
        eulerAngles.z = 90.0f;
      }
      else if (hy > MapSize - 1)
      {
        eulerAngles.z = 180.0f;
      }
      else if (lx < 0)
      {
        eulerAngles.z = 270.0f;
      }
    }
    else
    {
      prefabToInstantiate = GridCellFullPrefab;
    }

    Quaternion rotation = Quaternion.Euler(eulerAngles);

    Instantiate(prefabToInstantiate, new Vector3(x, y, 0.0f), rotation, _gridHolder);
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

        _dronesCountByOwner[ownerId]++;

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
      _territoryCountByOwner[ownerId]++;

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

  void PlaceRandomWalls(int tryToPlaceAmount)
  {    
    int counter = 0;

    for (int i = 0; i < tryToPlaceAmount; i++)
    {
      int x = Random.Range(0, MapSize);
      int y = Random.Range(0, MapSize);

      // If we already placed wall on this spot or we are too close to the base area - skip.
      if (((x >= 0 && x <= 5 && y >= 0 && y <= 5) 
        || (x >= MapSize - 6 && x <= MapSize - 1 && y >= MapSize - 6 && y <= MapSize - 1)) 
        || _objectsMap[x, y] != null)
      {
        continue;
      }

      CellWall c = new CellWall();
      var go = (GameObject)Instantiate(CellWallPrefab, new Vector3(x, y, 0.0f), Quaternion.identity, _gridHolder);

      c.OwnerId = -1;

      c.Coordinates.Set(x, y);

      CellBehaviour b = go.GetComponent<CellBehaviour>();

      b.CellInstance = c;
      b.CellInstance.BehaviourRef = b;
      b.CellInstance.ModelTransform = b.ModelTransform;
      b.CellInstance.InitBehaviour();

      _objectsMap[x, y] = b;

      counter++;
    }

    Debug.Log("Spawned " + counter + " walls for your pleasure");
  }

  public bool CheckLocationToBuild(Int2 posToCheck, int ownerId, int enemyId)
  {
    if (_objectsMap[posToCheck.X, posToCheck.Y] != null)
    {
      return false;
    }

    int d = 0;

    int checkCounterSelf = 0, checkCounterEnemy = 0;
    foreach (var item in _buildingsCoordinatesByOwner[ownerId])
    {
      d = Utils.BlockDistance(item, posToCheck);

      if (d <= GlobalConstants.BuildRangeDistance)
      {
        checkCounterSelf++;
      }
    }

    foreach (var item in _buildingsCoordinatesByOwner[enemyId])
    {
      d = Utils.BlockDistance(item, posToCheck);

      if (d <= GlobalConstants.DMZRange)
      {
        checkCounterEnemy++;
      }
    }

    return (checkCounterSelf != 0 && checkCounterEnemy == 0);
  }

  /// <summary>
  /// Destroys number of drones of ownerId player.
  /// </summary>
  /// <param name="number">Number of drones to destroy</param>
  /// <param name="ownerId">Owner (player) identifier</param>
  public void TransformDrones(int number, int ownerId)
  {
    int transformedCount = 0;

    int x = 0, y = 0;

    // For player drones are transformed starting from the NE side of the map,
    // for the CPU it's SW.
    int sx = (ownerId == 0) ? (MapSize - 1) : 0;
    int sy = (ownerId == 0) ? (MapSize - 1) : 0;

    for (x = sx; ((ownerId == 0) ? (x >= 0) : (x < MapSize)); x += (ownerId == 0) ? -1 : 1)
    {
      for (y = sy; ((ownerId == 0) ? (y >= 0) : (y < MapSize)); y += (ownerId == 0) ? -1 : 1)
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

  public void GameOver(int loserId, string message)
  {
    IsGameOver = true;
    Time.timeScale = 0.0f;

    // Time's up
    if (loserId == -1)
    {
      GameOverTitleText.text = "TIME'S UP!";
    }
    else
    {
      GameOverTitleText.text = (loserId == 0) ? "YOU LOST!" : "YOU WON!";
    }

    PlayerTerritoryText.text = _territoryCountByOwner[0].ToString();
    CPUTerritoryText.text = _territoryCountByOwner[1].ToString();
    PlayerScoreText.text = _scoreCountByOwner[0].ToString();
    CPUScoreText.text = _scoreCountByOwner[1].ToString();

    GameOverTitleText.color = (loserId == 0) ? Color.red : Color.white;

    PlayerTerritoryText.color = (_territoryCountByOwner[0] > _territoryCountByOwner[1]) ? Color.green : Color.white;
    CPUTerritoryText.color = (_territoryCountByOwner[1] > _territoryCountByOwner[0]) ? Color.green : Color.white;
    PlayerScoreText.color = (_scoreCountByOwner[0] > _scoreCountByOwner[1]) ? Color.green : Color.white;
    CPUScoreText.color = (_scoreCountByOwner[1] > _scoreCountByOwner[0]) ? Color.green : Color.white;

    StartCoroutine(ShowGameOverFormRoutine());
  }

  IEnumerator ShowGameOverFormRoutine()
  {
    Vector3 windowScale = GameOverWindow.localScale;

    GameOverWindow.localScale = Vector3.zero;

    float scale = 0.0f;

    GameOverWindow.gameObject.SetActive(true);

    while (scale < 1.0f)
    {
      scale += 0.1f;

      windowScale.Set(scale, scale, scale);

      GameOverWindow.localScale = windowScale;

      yield return null;
    }

    GameOverWindow.localScale = Vector3.one;

    yield return null;
  }

  public void SpawnBullet(Vector3 posToSpawn, Vector3 targetPos, CellBehaviour enemy, float bulletSpeed = GlobalConstants.DefaultBulletSpeed)
  {
    GameObject bullet = (GameObject)Instantiate(BulletPrefab, new Vector3(posToSpawn.x, posToSpawn.y, posToSpawn.z), Quaternion.identity, _gridHolder);
    bullet.GetComponent<Bullet>().SetTarget(targetPos, enemy, bulletSpeed);
  }

  void Update()
  {
    /*
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
    */

    if (Input.GetKey(KeyCode.Tab))
    {
      RefreshTerritoryOverlay();
    }
  }

  Int2 _overlayCellPos = Int2.Zero;
  Color _overlayCellColor = Color.white;
  void RefreshTerritoryOverlay()
  {
    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {        
        _overlayCellPos.Set(x, y);

        if (CheckLocationToBuild(_overlayCellPos, 0, 1))
        {
          _overlayCellColor.r = 0.0f;
          _overlayCellColor.g = 1.0f;
          _overlayCellColor.b = 0.0f;
          _overlayCellColor.a = 0.4f;
        }
        else if (CheckLocationToBuild(_overlayCellPos, 1, 0))
        {
          _overlayCellColor.r = 1.0f;
          _overlayCellColor.g = 0.0f;
          _overlayCellColor.b = 0.0f;
          _overlayCellColor.a = 0.4f;
        }
        else
        {
          _overlayCellColor.r = 1.0f;
          _overlayCellColor.g = 1.0f;
          _overlayCellColor.b = 1.0f;
          _overlayCellColor.a = 0.0f;
        }

        if (_territoryOverlay[x, y] != null)
        {
          _territoryOverlay[x, y].GetComponent<Renderer>().material.color = _overlayCellColor;
        }
      }
    }
  }

  public void RestartGameHandler()
  {
    #if !UNITY_EDITOR
    AudioManager.Instance.StopMusic();
    #endif

    Time.timeScale = 1.0f;
    SceneManager.LoadScene("main");
  }

  public void ReturnToTitleHandler()
  {
    GameOverWindow.gameObject.SetActive(false);

    #if !UNITY_EDITOR
    AudioManager.Instance.StopMusic();
    #endif

    Time.timeScale = 1.0f;
    SceneManager.LoadScene("title");
  }
}
