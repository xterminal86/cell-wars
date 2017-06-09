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
  public GameObject CellAcademyPrefab;
  public GameObject CellArsenalPrefab;
  public GameObject CellHolderPrefab;
  public GameObject CellDefenderPrefab;
  public GameObject CellSoldierPrefab;
  public GameObject CellSniperPrefab;
  public GameObject CellHeavyPrefab;
  public GameObject CellWallPrefab;
  public GameObject CellDestroyAnimationPrefab;
  public GameObject WallDestroyAnimationPrefab;

  public GameObject BulletPrefab;
  public GameObject BulletSplashPrefab;
  public GameObject GameOverExplosionPrefab;

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

  Renderer[,] _territoryOverlayRenderers;
  public Renderer[,] TerritoryOverlayRenderers
  {
    get { return _territoryOverlayRenderers; }
  }

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
    _territoryOverlayRenderers = new Renderer[MapSize, MapSize];

    SoldiersMap = new Dictionary<int, CellBehaviour>[MapSize, MapSize];

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        SoldiersMap[x, y] = new Dictionary<int, CellBehaviour>();

        _locksMap[x, y] = 0;
        _objectsMap[x, y] = null;

        InstantiateCellPrefab(x, y);

        GameObject go = Instantiate(TerritoryOverlayPrefab, new Vector3(x, y, -2.0f), Quaternion.identity, _territoryOverlayHolder);
        Material m = new Material(TerritoryOverlayMaterial);
        m.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        go.GetComponent<Renderer>().material = m;
        _territoryOverlayRenderers[x, y] = go.GetComponent<Renderer>();
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

    PlaceRandomWalls(100);   
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
        go = Instantiate(CellBasePrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.DRONE:

        _dronesCountByOwner[ownerId]++;

        c = new CellDrone();
        go = Instantiate(CellDronePrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.COLONY:
        
        c = new CellColony();
        go = Instantiate(CellColonyPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.SPAWNER:
        
        c = new CellSpawner();
        go = Instantiate(CellBarracksPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.ACADEMY:

        c = new CellAcademy();
        go = Instantiate(CellAcademyPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.ARSENAL:

        c = new CellArsenal();
        go = Instantiate(CellArsenalPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.ATTACKER:
        
        c = new CellAttacker();
        go = Instantiate(CellSoldierPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.SNIPER:

        c = new CellSniper();
        go = Instantiate(CellSniperPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.HEAVY:

        c = new CellHeavy();
        go = Instantiate(CellHeavyPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.HOLDER:
        
        c = new CellHolder();
        go = Instantiate(CellHolderPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;

      case GlobalConstants.CellType.DEFENDER:
      
        c = new CellDefender();
        go = Instantiate(CellDefenderPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

        break;            
    }

    if (c != null)
    {       
      Material m = new Material(CellMaterial);
      m.color = GlobalConstants.ColorsList[ownerId][c.Type];

      go.GetComponentInChildren<Renderer>().material = m;

      if (c.Type != GlobalConstants.CellType.DRONE && c.IsStationary)
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

      if (c.IsStationary)
      {        
        _objectsMap[pos.X, pos.Y] = b;
        _territoryCountByOwner[ownerId]++;
      }
    }

    return c;
  }

  void PlaceRandomWalls(int tryToPlaceAmount)
  {
    int[,] wallsMap = new int[MapSize, MapSize];
    List<Int2> wallsValidPositions = new List<Int2>();

    // Excluding base area of player and CPU (6x6x2)
    int wallsMax = Mathf.Clamp(tryToPlaceAmount, 1, MapSize * MapSize - 72);

    for (int i = 0; i < wallsMax; i++)
    {
      wallsValidPositions.Clear();

      for (int x = 0; x < MapSize; x++)
      {
        for (int y = 0; y < MapSize; y++)
        {
          if ((x >= 0 && x <= 5 && y >= 0 && y <= 5)
            || (x >= MapSize - 6 && x <= MapSize - 1 && y >= MapSize - 6 && y <= MapSize - 1))
          {
            continue;
          }

          if (wallsMap[x, y] != 1)
          {
            wallsValidPositions.Add(new Int2(x, y));
          }
        }
      }

      int index = Random.Range(0, wallsValidPositions.Count);

      Int2 pos = wallsValidPositions[index];
      CellWall c = new CellWall();
      var go = Instantiate(CellWallPrefab, new Vector3(pos.X, pos.Y, 0.0f), Quaternion.identity, _gridHolder);

      c.OwnerId = -1;

      c.Coordinates.Set(pos);

      CellBehaviour b = go.GetComponent<CellBehaviour>();

      b.CellInstance = c;
      b.CellInstance.BehaviourRef = b;
      b.CellInstance.ModelTransform = b.ModelTransform;
      b.CellInstance.InitBehaviour();

      _objectsMap[pos.X, pos.Y] = b;

      wallsMap[pos.X, pos.Y] = 1;
    }
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

    StartCoroutine(GameOverRoutine(loserId));
  }

  IEnumerator GameOverRoutine(int loserId)
  {    
    // Show some explosions
    if (loserId != -1)
    {
      Vector3 basePosition = Vector3.zero;

      if (loserId == 0)
      {
        basePosition.Set(1, 1, 0.0f);
      }
      else
      {
        basePosition.Set(MapSize - 2, MapSize - 2, 0.0f);
      }
        
      int explosionsCounter = 0;
      int explosionsNumber = 7;
      float timer = 0.0f;
      while (explosionsCounter < explosionsNumber)
      {
        timer += Time.smoothDeltaTime;

        if (timer > 0.1f)
        {
          float randX = Random.Range(-0.5f, 0.5f);
          float randY = Random.Range(-0.5f, 0.5f);

          Vector3 position = new Vector3(basePosition.x + randX, basePosition.y + randY, basePosition.z);

          Instantiate(GameOverExplosionPrefab, position, Quaternion.identity, _gridHolder);

          explosionsCounter++;
          timer = 0.0f;
        }

        yield return null;
      }

      // Wait for last animation to finish
      while (timer < 1.0f)
      {
        timer += Time.smoothDeltaTime;

        yield return null;
      }
    }

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

    Time.timeScale = 0.0f;

    yield return StartCoroutine(ShowGameOverFormRoutine());
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

  public void SpawnBullet(Vector3 posToSpawn, Vector3 targetPos, CellBehaviour owner, CellBehaviour enemy, int baseDamage = 1, float bulletSpeed = GlobalConstants.DefaultBulletSpeed)
  {
    GameObject bullet = Instantiate(BulletPrefab, new Vector3(posToSpawn.x, posToSpawn.y, posToSpawn.z), Quaternion.identity, _gridHolder);
    bullet.GetComponent<Bullet>().SetTarget(targetPos, owner, enemy, bulletSpeed, baseDamage);
  }

  public void SpawnSplashBullet(Vector3 posToSpawn, Vector3 targetPos, CellBehaviour owner, CellBehaviour enemy, int baseDamage, float splashRadius, float bulletSpeed = GlobalConstants.DefaultBulletSpeed)
  {
    GameObject bullet = Instantiate(BulletSplashPrefab, new Vector3(posToSpawn.x, posToSpawn.y, posToSpawn.z), Quaternion.identity, _gridHolder);
    bullet.GetComponent<Bullet>().SetTargetSplash(targetPos, owner, enemy, bulletSpeed, baseDamage, splashRadius);
  }

  public void InstantiateDeathAnimationPrefab(CellBehaviour cellToBeDestroyed)
  {
    var go = Instantiate(cellToBeDestroyed.CellInstance.Type == GlobalConstants.CellType.WALL ? WallDestroyAnimationPrefab : CellDestroyAnimationPrefab, cellToBeDestroyed.CellInstance.WorldCoordinates, Quaternion.identity, _gridHolder);
    go.GetComponentInChildren<Renderer>().material = cellToBeDestroyed.GetComponentInChildren<Renderer>().material;
    go.GetComponent<CellDestroyAnimation>().ShrinkObject();
  }

  void Update()
  {
    // If we press "Return to Title" reference to object is no longed present (new scene was loaded)
    if (_territoryOverlayHolder != null)
    {
      _territoryOverlayHolder.gameObject.SetActive(Input.GetKey(KeyCode.Tab));  
    }
  }

  Color _overlayColor = Color.green;
  public void RefreshTerritoryOverlay()
  {
    // Couldn't optimize it, so fuck it.

    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        if (_objectsMap[x, y] == null)
        {
          if (CheckLocationToBuild(new Int2(x, y), 0, 1))
          {
            _overlayColor.a = 0.4f;
          }
          else
          {
            _overlayColor.a = 0.0f;
          }
        }
        else
        {
          _overlayColor.a = 0.0f;
        }

        _territoryOverlayRenderers[x, y].material.color = _overlayColor;
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
