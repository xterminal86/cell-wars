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

    DronesNumber.text = string.Format("Drones: {0}", LevelLoader.Instance.DronesCount);

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
      LevelLoader.Instance.Build(_cellCoords, _buildingType);
    }
  }

  void EnableButtons()
  {
    int drones = LevelLoader.Instance.DronesCount;

    BuildColonyButton.interactable = (drones >= GlobalConstants.ColonyDronesCost);
    BuildBarracksButton.interactable = (drones >= GlobalConstants.BarracksDronesCost);

    CancelButton.gameObject.SetActive(_buildMode);
  }

  GlobalConstants.CellType _buildingType;
  public void BuildColonyHandler()
  {    
    _buildingType = GlobalConstants.CellType.COLONY;

    _buildMode = true;
  }

  public void CancelBuild()
  {    
    _buildMode = false;
  }
}
