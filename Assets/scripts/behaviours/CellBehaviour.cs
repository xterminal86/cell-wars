using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to every cell prefab.
/// </summary>
public class CellBehaviour : MonoBehaviour 
{
  public CellBaseClass CellInstance;

  public Transform RadiusMarker;
  public Transform ModelTransform;

  public Image HitpointsBar;

  void Start()
  {
    RefreshTerritoryOverlay();
    StartCoroutine(GrowRoutine());
  }

  Vector3 _scale = Vector3.zero;
  IEnumerator GrowRoutine()
  {
    float timer = 0.0f;
    while (timer < 1.0f)
    {
      timer += Time.smoothDeltaTime * 2.0f;

      _scale.Set(timer, timer, timer);

      transform.localScale = _scale;

      if (_isDestroying)
      {
        yield break;
      }
    
      yield return null;
    }

    _scale.Set(1, 1, 1);

    transform.localScale = _scale;

    yield return null;
  }

  float _hitpointsBarTimer = 0.0f;
	void Update() 
	{    
    if (_showHitpointsBarFlag)
    {
      _hitpointsBarTimer += Time.smoothDeltaTime;

      if (_hitpointsBarTimer > 3.0f)
      {
        _showHitpointsBarFlag = false;
        _hitpointsBarTimer = 0.0f;
        HitpointsBar.gameObject.SetActive(false);
      }
    }

    if (CellInstance.Type != GlobalConstants.CellType.DRONE)
    { 
      CellInstance.CalculateHitpointsBar(GlobalConstants.CellHitpointsByType[CellInstance.Type]);
    }

    if (!_isDestroying)
    {
      CellInstance.Update();
    }

    if (CellInstance.Hitpoints <= 0)
    {
      DestroySelf();
    }
	}

  Int2 _positionToCheck = Int2.Zero;
  Color _overlayCellColor = Color.white;
  void RefreshTerritoryOverlay()
  {
    if (CellInstance.OwnerId == 0)
    {
      if (CellInstance.Type != GlobalConstants.CellType.DRONE)
      {
        int lx = CellInstance.Coordinates.X - 3;
        int ly = CellInstance.Coordinates.Y - 3;
        int hx = CellInstance.Coordinates.X + 3;
        int hy = CellInstance.Coordinates.Y + 3;

        for (int x = lx; x <= hx; x++)
        {
          for (int y = ly; y <= hy; y++)
          {
            if (x >= 0 && x < LevelLoader.Instance.MapSize
              && y >= 0 && y < LevelLoader.Instance.MapSize)
            {
              _positionToCheck.Set(x, y);

              if (LevelLoader.Instance.CheckLocationToBuild(_positionToCheck, 0, 1))
              {
                _overlayCellColor.r = 0.0f;
                _overlayCellColor.g = 1.0f;
                _overlayCellColor.b = 0.0f;
                _overlayCellColor.a = 0.4f;

                LevelLoader.Instance.TerritoryOverlayRenderers[x, y].material.color = _overlayCellColor;
              }
              else
              {
                _overlayCellColor.r = 0.0f;
                _overlayCellColor.g = 1.0f;
                _overlayCellColor.b = 0.0f;
                _overlayCellColor.a = 0.0f;

                LevelLoader.Instance.TerritoryOverlayRenderers[x, y].material.color = _overlayCellColor;
              }
            }
          }
        }
      }
      else
      {
        _overlayCellColor.r = 0.0f;
        _overlayCellColor.g = 1.0f;
        _overlayCellColor.b = 0.0f;
        _overlayCellColor.a = 0.0f;

        LevelLoader.Instance.TerritoryOverlayRenderers[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].material.color = _overlayCellColor;
      }
    }
  }

  void UncolorCells()
  {
    if (CellInstance.OwnerId == 0)
    {
      if (CellInstance.Type != GlobalConstants.CellType.DRONE)
      {
        int lx = CellInstance.Coordinates.X - 3;
        int ly = CellInstance.Coordinates.Y - 3;
        int hx = CellInstance.Coordinates.X + 3;
        int hy = CellInstance.Coordinates.Y + 3;

        for (int x = lx; x <= hx; x++)
        {
          for (int y = ly; y <= hy; y++)
          {
            if (x >= 0 && x < LevelLoader.Instance.MapSize
                && y >= 0 && y < LevelLoader.Instance.MapSize)
            {
              _positionToCheck.Set(x, y);

              if (LevelLoader.Instance.CheckLocationToBuild(_positionToCheck, 0, 1))
              {
                _overlayCellColor.r = 0.0f;
                _overlayCellColor.g = 1.0f;
                _overlayCellColor.b = 0.0f;
                _overlayCellColor.a = 0.0f;

                LevelLoader.Instance.TerritoryOverlayRenderers[x, y].material.color = _overlayCellColor;
              }
            }
          }
        }
      }
      else
      {
        _overlayCellColor.r = 0.0f;
        _overlayCellColor.g = 1.0f;
        _overlayCellColor.b = 0.0f;
        _overlayCellColor.a = 0.4f;

        LevelLoader.Instance.TerritoryOverlayRenderers[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].material.color = _overlayCellColor;
      }
    }
  }

  bool _showHitpointsBarFlag = false;
  public void ShowHitpointsBar()
  {
    if (HitpointsBar != null)
    {
      HitpointsBar.gameObject.SetActive(true);
      _showHitpointsBarFlag = true;
      _hitpointsBarTimer = 0.0f;
    }
  }

  // Because of cell death animation, it might be possible for
  // an object to "pick up" reference to a cell that just started playing its death animation,
  // which will invalidate a reference after several frames. 
  // For example, when we transforming colony into holder from 16 drones, drones around new built holder
  // will be destroyed via TransformDrones, but almost immediately they will also be marked for destroying
  // by holder, because previous destroy only started the animation and hasn't set reference to null yet.
  // When it happens, second destroy call (issued by holder) will crash. 
  //
  // TLDR: basically, the problem is in duplicate start of coroutine.
  // To prevent this we introduce this flag.
  bool _isDestroying = false;
  public bool IsDestroying
  {
    get { return _isDestroying; }
  }

  public void DestroySelf()
  { 
    if (_isDestroying)
    {
      return;
    }

    _isDestroying = true;

    UncolorCells();

    ClearCellObject();

    StartCoroutine(DestroyRoutine());
  }

  IEnumerator DestroyRoutine()
  {
    float timer = 1.0f;
    while (timer > 0.0f)
    {
      timer -= Time.smoothDeltaTime * 2.0f;

      _scale.Set(timer, timer, timer);

      transform.localScale = _scale;

      yield return null;
    }

    _scale.Set(0, 0, 0);

    transform.localScale = _scale;

    DestroyGameObject();

    yield return null;
  }

  void ClearCellObject()
  {
    if (CellInstance.Type != GlobalConstants.CellType.SOLDIER)
    {
      LevelLoader.Instance.ObjectsMap[CellInstance.Coordinates.X, CellInstance.Coordinates.Y] = null;
    }

    if (CellInstance.Type != GlobalConstants.CellType.NONE && CellInstance.Type != GlobalConstants.CellType.WALL && CellInstance.Type != GlobalConstants.CellType.SOLDIER)
    {
      LevelLoader.Instance.TerritoryCountByOwner[CellInstance.OwnerId]--;
    }

    switch (CellInstance.Type)
    {
      case GlobalConstants.CellType.DRONE:
        LevelLoader.Instance.DronesCountByOwner[CellInstance.OwnerId]--;
        break;

      case GlobalConstants.CellType.SOLDIER:
        (CellInstance as CellSoldier).DelistFromBarracks();
        LevelLoader.Instance.SoldiersMap[CellInstance.Coordinates.X, CellInstance.Coordinates.Y].Remove(CellInstance.GetHashCode());
        LevelLoader.Instance.SoldiersCountByOwner[CellInstance.OwnerId]--;
        break;

      case GlobalConstants.CellType.BARRACKS:
      case GlobalConstants.CellType.COLONY:
      case GlobalConstants.CellType.DEFENDER:
        LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
        break;
      case GlobalConstants.CellType.HOLDER:
        (CellInstance as CellHolder).UnlockCells();
        LevelLoader.Instance.RemoveBuildingFromDictionary(CellInstance.OwnerId, CellInstance.Coordinates);
        break;
    }
  }

  void DestroyGameObject()
  {    
    Destroy(gameObject);

    if (CellInstance.Type == GlobalConstants.CellType.BASE)
    {
      LevelLoader.Instance.GameOver(CellInstance.OwnerId, (CellInstance.OwnerId == 0) ? "CPU WON!" : "YOU WON!");    
    }

    CellInstance = null;
  }
}
