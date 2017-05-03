using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Main : MonoBehaviour 
{	  
  // Prefab for a square that highlights the grid cell
  public GameObject GridHighlighter;

  public SelectedSpot SelectedSpot;

  public RectTransform BuildButtonsGroup;
  public RectTransform CancelButtonGroup;

  // Building menu
  public Button BuildColonyButton;
  public Button BuildBarracksButton;
  public Button BuildHolderButton;
  public Button BuildDefenderButton;
  public Button CancelButton;

  // For various information text
  public Text InfoText;

  // Number of drones
  public Text DronesPlayerText;
  public Text DronesCpuText;

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

    DronesPlayerText.text = LevelLoader.Instance.DronesCountByOwner[0].ToString();
    DronesCpuText.text = LevelLoader.Instance.DronesCountByOwner[1].ToString();

    EnableButtons();

    _validSpot = LevelLoader.Instance.CheckLocationToBuild(_selectedSpotPos2D, 0);
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

      _highlighterMaterial.color = LevelLoader.Instance.CheckLocationToBuild(_cellCoords, 0) ? Color.green : Color.white;

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
  CellBaseClass _selectedCell;
  void ProcessInput()
  {   
    if (Input.GetMouseButtonDown(0) && IsValidClickPosition())
    { 
      _buildMode = true;

      _selectedCell = LevelLoader.Instance.Map[_cellCoords.X, _cellCoords.Y].CellHere;

      _selectedSpotPos.x = _cellCoords.X;
      _selectedSpotPos.y = _cellCoords.Y;
      _selectedSpotPos.z = SelectedSpot.transform.position.z;

      _selectedSpotPos2D.X = _cellCoords.X;
      _selectedSpotPos2D.Y = _cellCoords.Y;

      SelectedSpot.transform.position = _selectedSpotPos;
    }
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
    bool colonySelected = (_selectedCell != null && _selectedCell.Type == GlobalConstants.CellType.COLONY);

    BuildColonyButton.interactable = (_buildMode && _validSpot && dronesPlayer >= GlobalConstants.CellColonyHitpoints);
    BuildBarracksButton.interactable = (_buildMode && colonySelected && dronesPlayer >= GlobalConstants.CellBarracksHitpoints);
    BuildHolderButton.interactable = (_buildMode && colonySelected && dronesPlayer >= GlobalConstants.CellHolderHitpoints);
    BuildDefenderButton.interactable = (_buildMode && colonySelected && dronesPlayer >= GlobalConstants.CellDefenderHitpoints);

    CancelButton.gameObject.SetActive(_buildMode);
  }

  GlobalConstants.CellType _buildingType = GlobalConstants.CellType.NONE;
  public void BuildSelectHandler(int buildingIndex)
  {
    switch (buildingIndex)
    {
      case 0:
        _buildingType = GlobalConstants.CellType.COLONY;
        LevelLoader.Instance.Build(_selectedSpotPos2D, _buildingType, 0);
        break;

      case 1:
        _buildingType = GlobalConstants.CellType.BARRACKS;
        _selectedCell.BehaviourRef.DestroySelf();
        LevelLoader.Instance.Build(_selectedSpotPos2D, _buildingType, 0);
        break;

      case 2:
        _buildingType = GlobalConstants.CellType.HOLDER;
        _selectedCell.BehaviourRef.DestroySelf();
        LevelLoader.Instance.Build(_selectedSpotPos2D, _buildingType, 0);
        break;

      case 3:
        _buildingType = GlobalConstants.CellType.DEFENDER;
        _selectedCell.BehaviourRef.DestroySelf();
        LevelLoader.Instance.Build(_selectedSpotPos2D, _buildingType, 0);
        break;
    }

    _buildMode = false;
  }

  public void CancelBuild()
  { 
    _buildMode = false;

    SelectedSpot.transform.gameObject.SetActive(false);
    _buildingType = GlobalConstants.CellType.NONE;
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
}
