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
    // Kinda hackish way, since we assume that this call is made 
    // after CellInstance gets assigned in LevelLoader::PlaceCell()
    if (CellInstance != null && CellInstance.OwnerId == 0)
    {
      LevelLoader.Instance.RefreshTerritoryOverlay();
    }

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

  void Update() 
	{ 
    if (CellInstance != null)
    {
      CellInstance.Update();
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
  // TLDR: basically, the problem is in double delete.
  // To prevent this we introduce this flag.
  bool _isDestroying = false;
  public bool IsDestroying
  {
    get { return _isDestroying; }
  }

  /// <summary>
  /// Current object gets destroyed, instantiating temporary animation prefab in its place.
  /// Different objects may need different death prefabs (like wall).
  /// </summary>
  public void DestroySelf()
  { 
    if (_isDestroying)
    {
      return;
    }

    _isDestroying = true;

    if (CellInstance.OwnerId == 0)
    {
      LevelLoader.Instance.RefreshTerritoryOverlay();
    }

    LevelLoader.Instance.InstantiateDeathAnimationPrefab(this);

    ClearCellObject();
    DestroyGameObject();
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
