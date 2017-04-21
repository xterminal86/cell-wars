using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour 
{	  
  public GameObject GridHighlighter;

  public Button BuildColonyButton;
  public Button BuildBarracksButton;
  public Button CancelButton;

  public Text DronesNumber;
  public Text OccupyMap;

  GameObject _highlighter;

  Material _highlighterMaterial;

  bool _buildMode = false;
  bool _validSpot = false;

  void Start()
  {    
    _highlighter = (GameObject)Instantiate(GridHighlighter, Vector3.zero, Quaternion.identity);
    _highlighterMaterial = _highlighter.GetComponent<Renderer>().material;

    LevelLoader.Instance.Initialize();
  }

  Vector3 _mousePosition = Vector3.zero;
  Vector3 _highligherPosition = Vector3.zero;
  void Update()
  {
    ProcessHighlighter();
    ProcessInput();

    _highlighter.transform.position = _highligherPosition;

    // FIXME: ownerID magic numbers

    DronesNumber.text = string.Format("Player drones: {0}\nCPU drones: {1}\nPlayer soldiers: {2}\nCPU soldiers: {3}", 
      LevelLoader.Instance.DronesCountByOwner[0], LevelLoader.Instance.DronesCountByOwner[1],
      LevelLoader.Instance.SoldiersCountByOwner[0], LevelLoader.Instance.SoldiersCountByOwner[1]);

    PrintOccupyMap();

    EnableButtons();
  }

  string _occupyMap = string.Empty;
  void PrintOccupyMap()
  {   
    _occupyMap = "";
      
    for (int x = 0; x < LevelLoader.Instance.MapSize; x++)
    {
      for (int y = 0; y < LevelLoader.Instance.MapSize; y++)
      {
        if (LevelLoader.Instance.Map[x, y].SoldierHere == null)
        {
          _occupyMap += "0";
        }
        else
        {
          _occupyMap += "1";
        }
      }

      _occupyMap += "\n";
    }

    OccupyMap.text = _occupyMap;
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

      _validSpot = LevelLoader.Instance.CheckLocationToBuild(_cellCoords);

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
      LevelLoader.Instance.Build(_cellCoords, _buildingType, 0);

      _buildMode = false;
    }
  }

  void EnableButtons()
  {
    int dronesPlayer = LevelLoader.Instance.DronesCountByOwner[0];

    BuildColonyButton.interactable = (dronesPlayer >= GlobalConstants.ColonyDronesCost);
    BuildBarracksButton.interactable = (dronesPlayer >= GlobalConstants.BarracksDronesCost);

    CancelButton.gameObject.SetActive(_buildMode);
  }

  GlobalConstants.CellType _buildingType;
  public void BuildColonyHandler()
  {    
    _buildingType = GlobalConstants.CellType.COLONY;

    _buildMode = true;
  }

  public void BuildBarracksHandler()
  {
    _buildingType = GlobalConstants.CellType.BARRACKS;

    _buildMode = true;
  }

  public void CancelBuild()
  {    
    _buildMode = false;
  }
}
