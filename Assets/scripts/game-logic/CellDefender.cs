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

  public override void Update()
  {
    base.Update();

    PlayAnimation();

    if (LevelLoader.Instance.IsGameOver)
    {
      return;
    }

    if (_enemyFound == null)
    {      
      _timer += Time.smoothDeltaTime;

      FindEnemies(0.0f, GlobalConstants.CellDefenderRange, GlobalConstants.CellType.WALL);
    }
    else
    {      
      if (_timer <= GlobalConstants.DefenderAttackTimeout)
      {
        _timer += Time.smoothDeltaTime;
      }
      else
      {
        FindEnemies(0.0f, GlobalConstants.CellDefenderRange, GlobalConstants.CellType.WALL);
        _timer = 0.0f;
        LevelLoader.Instance.SpawnBullet(WorldCoordinates, _enemyFound.CellInstance.WorldCoordinates, BehaviourRef, _enemyFound, GlobalConstants.DefenderBulletSpeed);
      }
    }
  }
}
