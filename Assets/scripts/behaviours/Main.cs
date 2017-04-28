using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour 
{	  
  // Prefab for a square that highlights the grid cell
  public GameObject GridHighlighter;

  // Building menu
  public Button BuildColonyButton;
  public Button BuildBarracksButton;
  public Button CancelButton;

  // For various information text
  public Text InfoText;

  // Number of drones
  public Text DronesPlayerText;
  public Text DronesCpuText;

  GameObject _highlighter;

  Material _highlighterMaterial;

  // When user clicks building button, we enter "build mode"
  bool _buildMode = false;

  // True if current spot is valid for building
  bool _validSpot = false;

  float _cameraScrollLimit = 0.0f;
  void Start()
  {    
    InfoText.text = "";
      
    _highlighter = (GameObject)Instantiate(GridHighlighter, Vector3.zero, Quaternion.identity);
    _highlighterMaterial = _highlighter.GetComponent<Renderer>().material;

    LevelLoader.Instance.Initialize();

    _cameraScrollLimit = LevelLoader.Instance.MapSize;
  }

  Vector3 _mousePosition = Vector3.zero;
  Vector3 _highligherPosition = Vector3.zero;
  void Update()
  {
    ProcessHighlighter();
    ProcessInput();
    ControlCamera();

    _highlighter.transform.position = _highligherPosition;

    // FIXME: ownerID magic numbers

    DronesPlayerText.text = LevelLoader.Instance.DronesCountByOwner[0].ToString();
    DronesCpuText.text = LevelLoader.Instance.DronesCountByOwner[1].ToString();

    EnableButtons();
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

      _validSpot = LevelLoader.Instance.CheckLocationToBuild(_cellCoords, 0);

      if (_buildMode)
      {
        _highlighterMaterial.color = _validSpot ? Color.green : Color.red;
      } 
      else
      {
        _highlighterMaterial.color = Color.white;
      }

      _highligherPosition = hitInfo.collider.transform.parent.position;
      _highligherPosition.z = -4.0f;
    }
  }

  void ProcessInput()
  {    
    if (_buildMode && _validSpot && Input.GetMouseButtonDown(0))
    {
      if (LevelLoader.Instance.DronesCountByOwner[0] < GlobalConstants.DroneCostByType[_buildingType])
      {
        PrintInfoText("Not enough drones!");
        return;
      }

      LevelLoader.Instance.Build(_cellCoords, _buildingType, 0);

      _buildMode = false;
    }
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

    BuildColonyButton.interactable = (dronesPlayer >= GlobalConstants.ColonyDronesCost);
    BuildBarracksButton.interactable = (dronesPlayer >= GlobalConstants.BarracksDronesCost);

    CancelButton.gameObject.SetActive(_buildMode);
  }

  GlobalConstants.CellType _buildingType = GlobalConstants.CellType.NONE;
  public void BuildSelectHandler(int buildingIndex)
  {
    switch (buildingIndex)
    {
      case 0:
        _buildingType = GlobalConstants.CellType.COLONY;
        break;

      case 1:
        _buildingType = GlobalConstants.CellType.BARRACKS;
        break;
    }

    _buildMode = true;
  }

  public void CancelBuild()
  {    
    _buildingType = GlobalConstants.CellType.NONE;
    _buildMode = false;
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
