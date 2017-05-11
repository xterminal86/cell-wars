using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour 
{	  
  // Prefab for a square that highlights the grid cell
  public GameObject GridHighlighter;

  public SelectedSpot SelectedSpot;

  public GameObject MenuButtonsGroup;

  // Building menu
  public Canvas BuildButtonsGroup;
  public CustomButton BuildColonyButton;
  public CustomButton BuildBarracksButton;
  public CustomButton BuildHolderButton;
  public CustomButton BuildDefenderButton;

  // For various information text
  public Text InfoText;

  // Number of drones
  public Text TerritoryPlayerText;
  public Text TerritoryCpuText;

  public Text ScorePlayerText;
  public Text ScoreCPUText;

  // Time of round
  public Text TimerText;

  GameObject _highlighter;

  Material _highlighterMaterial;

  bool _buildMode = false;

  float _cameraScrollLimit = 0.0f;
  void Start()
  {   
    InfoText.text = "";
      
    _highlighter = (GameObject)Instantiate(GridHighlighter, Vector3.zero, Quaternion.identity);
    _highlighterMaterial = _highlighter.GetComponent<Renderer>().material;

    LevelLoader.Instance.Initialize();

    _cameraScrollLimit = LevelLoader.Instance.MapSize;
  }

  int _secondsPassed = 0;
  float _roundTimer = 0.0f;
  bool _validSpot = false;
  Vector3 _mousePosition = Vector3.zero;
  Vector3 _highligherPosition = Vector3.zero;
  void Update()
  {
    ProcessHighlighter();
    ProcessInput();
    ControlCamera();

    SelectedSpot.transform.gameObject.SetActive(_buildMode);

    _highlighter.transform.position = _highligherPosition;

    // FIXME: ownerID magic numbers

    TerritoryPlayerText.text = LevelLoader.Instance.TerritoryCountByOwner[0].ToString();
    TerritoryCpuText.text = LevelLoader.Instance.TerritoryCountByOwner[1].ToString();

    ScorePlayerText.text = LevelLoader.Instance.ScoreCountByOwner[0].ToString();
    ScoreCPUText.text = LevelLoader.Instance.ScoreCountByOwner[1].ToString();

    EnableButtons();

    _validSpot = LevelLoader.Instance.CheckLocationToBuild(_selectedSpotPos2D, 0, 1);

    _roundTimer += Time.smoothDeltaTime;

    if (_roundTimer > 1.0f)
    {
      _secondsPassed++;
      _roundTimer = 0.0f;
    }

    if (_secondsPassed >= GlobalConstants.RoundTimeSeconds)
    {      
      PrintInfoText("TIME'S UP!");
      Time.timeScale = 0.0f;
    }

    int minutes = (GlobalConstants.RoundTimeSeconds - _secondsPassed) / 60;
    int seconds = (GlobalConstants.RoundTimeSeconds - _secondsPassed) % 60;

    TimerText.text = string.Format("{0}:{1:00}", minutes, seconds);
  }

  Int2 _cellCoords = Int2.Zero;
  void ProcessHighlighter()
  {    
    _mousePosition = Input.mousePosition;

    Ray r = Camera.main.ScreenPointToRay(_mousePosition);

    RaycastHit hitInfo;
    if (Physics.Raycast(r, out hitInfo, Mathf.Infinity, LayerMask.GetMask("grid")))
    {
      _cellCoords.X = (int)hitInfo.collider.gameObject.transform.position.x;
      _cellCoords.Y = (int)hitInfo.collider.gameObject.transform.position.y;

      _highlighterMaterial.color = LevelLoader.Instance.CheckLocationToBuild(_cellCoords, 0, 1) ? Color.green : Color.white;

      _highligherPosition = hitInfo.collider.transform.parent.position;
      _highligherPosition.z = -4.0f;
    }

    if (SelectedSpot.gameObject.activeSelf)
    {
      SelectedSpot.SetColor(_validSpot ? Color.green : Color.red);
    }
  }

  Vector3 _selectedSpotPos = Vector3.zero;
  Int2 _selectedSpotPos2D = Int2.Zero;
  CellBehaviour _selectedCell;
  void ProcessInput()
  { 
    if (Input.GetMouseButtonDown(0) && IsValidClickPosition())
    { 
      if (!_buildMenuAnimationWorking)
      {
        StartCoroutine(ShowBuildButtonsRoutine());
      }

      _buildMode = true;

      _selectedCell = LevelLoader.Instance.ObjectsMap[_cellCoords.X, _cellCoords.Y];

      _selectedSpotPos.x = _cellCoords.X;
      _selectedSpotPos.y = _cellCoords.Y;
      _selectedSpotPos.z = SelectedSpot.transform.position.z;

      _selectedSpotPos2D.X = _cellCoords.X;
      _selectedSpotPos2D.Y = _cellCoords.Y;

      SelectedSpot.transform.position = _selectedSpotPos;
    }
    else if (Input.GetMouseButtonDown(1))
    {
      CancelBuild();
    }
    else if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (MenuButtonsGroup.activeSelf)
      {
        MenuButtonsGroup.SetActive(false);
        Time.timeScale = 1.0f;
      }
      else
      {
        MenuButtonsGroup.SetActive(true);
        Time.timeScale = 0.0f;
      }
    }
  }

  IEnumerator WaitRoutine(int framesToWait, Callback cb)
  {
    int frames = 0;
    while (frames < framesToWait)
    {
      frames++;
      yield return null;
    }

    if (cb != null)
      cb();
    
    yield return null;
  }

  bool _buildMenuAnimationWorking = false;
  float _buttonsMenuScale = 1.5f;
  IEnumerator ShowBuildButtonsRoutine()
  {
    _buildMenuAnimationWorking = true;

    float scaleStep = (_buttonsMenuScale / 10.0f);

    BuildButtonsGroup.GetComponent<GraphicRaycaster>().enabled = false;

    Vector3 position = BuildButtonsGroup.transform.localPosition;
    Vector3 localScale = BuildButtonsGroup.transform.localScale;
    localScale.Set(0.0f, 0.0f, 0.0f);
    position.Set(_cellCoords.X, _cellCoords.Y, Camera.main.transform.localPosition.z + 1.0f);

    BuildButtonsGroup.transform.localPosition = position;
    BuildButtonsGroup.transform.localScale = localScale;

    BuildButtonsGroup.gameObject.SetActive(true);

    float scale = 0.0f;

    while (scale < _buttonsMenuScale)
    {
      scale += scaleStep;

      localScale.Set(scale, scale, scale);
      BuildButtonsGroup.transform.localScale = localScale;

      scale = Mathf.Clamp(scale, 0.0f, _buttonsMenuScale);

      yield return null;
    }

    localScale.Set(_buttonsMenuScale, _buttonsMenuScale, _buttonsMenuScale);
    BuildButtonsGroup.transform.localScale = localScale;

    BuildButtonsGroup.GetComponent<GraphicRaycaster>().enabled = true;

    _buildMenuAnimationWorking = false;

    yield return null;
  }

  IEnumerator HideBuildButtonsRoutine()
  {
    _buildMenuAnimationWorking = true;

    float scaleStep = (_buttonsMenuScale / 10.0f);

    BuildButtonsGroup.GetComponent<GraphicRaycaster>().enabled = false;

    Vector3 localScale = BuildButtonsGroup.transform.localScale;
    localScale.Set(_buttonsMenuScale, _buttonsMenuScale, _buttonsMenuScale);

    BuildButtonsGroup.transform.localScale = localScale;

    float scale = _buttonsMenuScale;

    while (scale > 0.0f)
    {
      scale -= scaleStep;

      localScale.Set(scale, scale, scale);
      BuildButtonsGroup.transform.localScale = localScale;

      scale = Mathf.Clamp(scale, 0.0f, _buttonsMenuScale);

      yield return null;
    }

    BuildButtonsGroup.gameObject.SetActive(false);

    BuildButtonsGroup.GetComponent<GraphicRaycaster>().enabled = true;

    _buildMenuAnimationWorking = false;

    yield return null;
  }

  public void CancelBuild()
  {
    if (!_buildMenuAnimationWorking)
    {
      StartCoroutine(HideBuildButtonsRoutine());
    }

    _buildMode = false;

    SelectedSpot.transform.gameObject.SetActive(false);
    _buildingType = GlobalConstants.CellType.NONE;
  }

  bool IsValidClickPosition()
  {
    bool isOverUI = EventSystem.current.IsPointerOverGameObject();
    Vector3 mousePos = Input.mousePosition;

    Ray r = Camera.main.ScreenPointToRay(mousePos);

    RaycastHit hitInfo;
    bool boundsCheck = Physics.Raycast(r, out hitInfo, Mathf.Infinity, LayerMask.GetMask("grid"));

    return (!isOverUI && boundsCheck);
  }

  Vector3 _cameraPos = Vector3.zero;
  float _zoom = 0.0f, _cameraMoveSpeed = 0.0f;
  void ControlCamera()
  {
    _cameraPos = Camera.main.transform.position;
    _zoom = Camera.main.orthographicSize;
    _cameraMoveSpeed = GlobalConstants.CameraMoveSpeed;

    if (Input.GetKey(KeyCode.LeftShift))
    {
      _cameraMoveSpeed *= 2.0f;
    }

    if (Input.GetKey(KeyCode.A))
    {
      _cameraPos.x -= Time.smoothDeltaTime * _cameraMoveSpeed;
    }

    if (Input.GetKey(KeyCode.D))
    {
      _cameraPos.x += Time.smoothDeltaTime * _cameraMoveSpeed;
    }

    if (Input.GetKey(KeyCode.W))
    {
      _cameraPos.y += Time.smoothDeltaTime * _cameraMoveSpeed;
    }

    if (Input.GetKey(KeyCode.S))
    {
      _cameraPos.y -= Time.smoothDeltaTime * _cameraMoveSpeed;
    }

    if (Input.GetAxis("Mouse ScrollWheel") > 0.0f)
    {
      _zoom -= GlobalConstants.CameraZoomSpeed;
    }
    else if (Input.GetAxis("Mouse ScrollWheel") < 0.0f)
    {
      _zoom += GlobalConstants.CameraZoomSpeed;
    }

    _cameraPos.x = Mathf.Clamp(_cameraPos.x, 0.0f, _cameraScrollLimit);
    _cameraPos.y = Mathf.Clamp(_cameraPos.y, 0.0f, _cameraScrollLimit);

    _zoom = Mathf.Clamp(_zoom, 2.0f, Mathf.Infinity);

    Camera.main.transform.position = _cameraPos;
    Camera.main.orthographicSize = _zoom;
  }

  void EnableButtons()
  {
    int dronesPlayer = LevelLoader.Instance.DronesCountByOwner[0];
    bool colonySelected = (_selectedCell != null 
      && _selectedCell.CellInstance.Type == GlobalConstants.CellType.COLONY 
      && _selectedCell.CellInstance.OwnerId == 0);

    BuildColonyButton.Interactable = (_buildMode && _validSpot && dronesPlayer >= GlobalConstants.CellColonyHitpoints);
    BuildBarracksButton.Interactable = (_buildMode && colonySelected && dronesPlayer >= GlobalConstants.CellBarracksHitpoints);
    BuildHolderButton.Interactable = (_buildMode && colonySelected && dronesPlayer >= GlobalConstants.CellHolderHitpoints);
    BuildDefenderButton.Interactable = (_buildMode && colonySelected && dronesPlayer >= GlobalConstants.CellDefenderHitpoints);
  }

  // Since we wait for transforming of drones before actual build,
  // it is possible to order a building and then click on other cell
  // which will erase old build coordinates.
  // To prevent this, we remember building position.
  Int2 _buildPos = Int2.Zero;

  GlobalConstants.CellType _buildingType = GlobalConstants.CellType.NONE;
  public void BuildSelectHandler(int buildingIndex)
  {
    switch (buildingIndex)
    {
      case 0:
        StartCoroutine(HideBuildButtonsRoutine());
        _buildingType = GlobalConstants.CellType.COLONY;
        LevelLoader.Instance.TransformDrones(GlobalConstants.CellColonyHitpoints, 0);
        LevelLoader.Instance.PlaceCell(_selectedSpotPos2D, _buildingType, 0);
        break;

      case 1:
        _buildPos.Set(_selectedSpotPos2D);
        StartCoroutine(HideBuildButtonsRoutine());
        _buildingType = GlobalConstants.CellType.BARRACKS;
        LevelLoader.Instance.TransformDrones(GlobalConstants.CellBarracksHitpoints, 0);
        _selectedCell.DestroySelf();
        StartCoroutine(WaitForDestroyRoutine(() =>
        {          
          LevelLoader.Instance.PlaceCell(_buildPos, _buildingType, 0);
        }));
        break;

      case 2:
        _buildPos.Set(_selectedSpotPos2D);
        StartCoroutine(HideBuildButtonsRoutine());
        _buildingType = GlobalConstants.CellType.HOLDER;
        LevelLoader.Instance.TransformDrones(GlobalConstants.CellHolderHitpoints, 0);
        _selectedCell.DestroySelf();
        StartCoroutine(WaitForDestroyRoutine(() =>
        {          
          LevelLoader.Instance.PlaceCell(_buildPos, _buildingType, 0);
        }));
        break;

      case 3:
        _buildPos.Set(_selectedSpotPos2D);
        StartCoroutine(HideBuildButtonsRoutine());
        _buildingType = GlobalConstants.CellType.DEFENDER;
        LevelLoader.Instance.TransformDrones(GlobalConstants.CellDefenderHitpoints, 0);
        _selectedCell.DestroySelf();
        StartCoroutine(WaitForDestroyRoutine(() =>
        {          
          LevelLoader.Instance.PlaceCell(_buildPos, _buildingType, 0);
        }));
        break;
    }

    _buildMode = false;
  }

  IEnumerator WaitForDestroyRoutine(Callback cb)
  {
    while (_selectedCell != null)
    {
      yield return null;
    }

    if (cb != null)
      cb();

    yield return null;
  }

  public void PrintInfoText(string text)
  {
    StartCoroutine(PrintAndFadeTextRoutine(text));
  }

  IEnumerator PrintAndFadeTextRoutine(string textToShow)
  {
    InfoText.text = textToShow;

    float _timer = 0.0f;
    while (_timer < 3.0f)
    {
      _timer += Time.smoothDeltaTime;

      yield return null;
    }

    InfoText.text = "";
  }

  public void ResumeGameHandler()
  {
    StartCoroutine(WaitRoutine(3, () =>
    {
      MenuButtonsGroup.SetActive(false);
      Time.timeScale = 1.0f;
    }));
  }

  public void ReturnToTitleHandler()
  {
    #if !UNITY_EDITOR
    AudioManager.Instance.StopMusic();
    #endif

    Time.timeScale = 1.0f;
    SceneManager.LoadScene("title");
  }
}
