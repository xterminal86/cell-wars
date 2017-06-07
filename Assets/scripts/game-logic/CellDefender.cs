using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellDefender : CellBaseClass 
{
  public CellDefender()
  {
    Type = GlobalConstants.CellType.DEFENDER;
    Hitpoints = GlobalConstants.CellDefenderHitpoints;
    Priority = GlobalConstants.CellDefenderPriority;
  }
  
  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 1.0f;
    _animationSpeed = 0.1f;

    Vector3 newScale = new Vector3(GlobalConstants.CellDefenderRange * 2, GlobalConstants.CellDefenderRange * 2, GlobalConstants.CellDefenderRange * 2);
    BehaviourRef.RadiusMarker.localScale = newScale;
  }

  float _timer = 0.0f;

  CellBehaviour _enemyFound;
  public override void Update()
  {
    base.Update();

    PlayAnimation();

    if (_enemyFound == null)
    {      
      _timer += Time.smoothDeltaTime;

      FindEnemies();
    }
    else
    {      
      if (_timer <= GlobalConstants.DefenderAttackTimeout)
      {
        _timer += Time.smoothDeltaTime;
      }
      else
      {
        SearchForPriorityTarget();
        _timer = 0.0f;
        LevelLoader.Instance.SpawnBullet(WorldCoordinates, _enemyFound.CellInstance.WorldCoordinates, _enemyFound, GlobalConstants.DefenderBulletSpeed);
      }
    }
  }

  // FIXME: lots of duplicate code
  void SearchForPriorityTarget()
  {
    _distance = 0.0f;

    int lx = Coordinates.X - Mathf.CeilToInt(GlobalConstants.CellDefenderRange);
    int ly = Coordinates.Y - Mathf.CeilToInt(GlobalConstants.CellDefenderRange);
    int hx = Coordinates.X + Mathf.CeilToInt(GlobalConstants.CellDefenderRange);
    int hy = Coordinates.Y + Mathf.CeilToInt(GlobalConstants.CellDefenderRange);

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
          && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          // Check soldiers first

          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {           
            if (kvp.Value == null)
            {
              continue;
            }

            _distance = Vector3.Distance(WorldCoordinates, kvp.Value.CellInstance.WorldCoordinates);

            if (kvp.Value.CellInstance.OwnerId != OwnerId && _distance <= GlobalConstants.CellDefenderRange
              && _enemyFound != null && kvp.Value.CellInstance.Priority > _enemyFound.CellInstance.Priority 
              && !kvp.Value.IsDestroying)
            {               
              _enemyFound = kvp.Value;
              return;
            }
          }

          // Check other cells second

          if (LevelLoader.Instance.ObjectsMap[x, y] != null)
          {
            _distance = Vector3.Distance(WorldCoordinates, LevelLoader.Instance.ObjectsMap[x, y].CellInstance.WorldCoordinates);

            if (_distance <= GlobalConstants.CellDefenderRange 
              && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type != GlobalConstants.CellType.WALL
              && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.OwnerId != OwnerId
              && _enemyFound != null && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Priority > _enemyFound.CellInstance.Priority 
              && !LevelLoader.Instance.ObjectsMap[x, y].IsDestroying
              && !LevelLoader.Instance.ObjectsMap[x, y].CellInstance.IsBeingAttacked)
            {                      
              _enemyFound = LevelLoader.Instance.ObjectsMap[x, y];
              return;
            }
          }
        }
      }
    }
  }

  float _distance = 0.0f;
  void FindEnemies()
  {
    _distance = 0.0f;

    int lx = Coordinates.X - Mathf.CeilToInt(GlobalConstants.CellDefenderRange);
    int ly = Coordinates.Y - Mathf.CeilToInt(GlobalConstants.CellDefenderRange);
    int hx = Coordinates.X + Mathf.CeilToInt(GlobalConstants.CellDefenderRange);
    int hy = Coordinates.Y + Mathf.CeilToInt(GlobalConstants.CellDefenderRange);

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
          && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          // Check soldiers first

          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {           
            if (kvp.Value == null)
            {
              continue;
            }
            
            _distance = Vector3.Distance(WorldCoordinates, kvp.Value.CellInstance.WorldCoordinates);

            if (kvp.Value.CellInstance.OwnerId != OwnerId 
              && _distance <= GlobalConstants.CellDefenderRange 
              && !kvp.Value.IsDestroying)
            {               
              _enemyFound = kvp.Value;
              return;
            }
          }

          // Check other cells second

          if (LevelLoader.Instance.ObjectsMap[x, y] != null)
          {
            _distance = Vector3.Distance(WorldCoordinates, LevelLoader.Instance.ObjectsMap[x, y].CellInstance.WorldCoordinates);

            if (_distance <= GlobalConstants.CellDefenderRange 
              && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type != GlobalConstants.CellType.WALL
              && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.OwnerId != OwnerId 
              && !LevelLoader.Instance.ObjectsMap[x, y].IsDestroying
              && !LevelLoader.Instance.ObjectsMap[x, y].CellInstance.IsBeingAttacked)
            {                      
              _enemyFound = LevelLoader.Instance.ObjectsMap[x, y];
              return;
            }
          }
        }
      }
    }
  }
}
