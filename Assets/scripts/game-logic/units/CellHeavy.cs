using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellHeavy : CellSoldier
{ 
  public CellHeavy()
  {    
    Type = GlobalConstants.CellType.HEAVY;
    Hitpoints = GlobalConstants.CellHeavyHitpoints;
    Priority = GlobalConstants.CellHeavyPriority;
    IsStationary = false;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 2.0f / 4.0f;
    _animationSpeed = 0.1f / _phaseDuration;
    _zRotationSpeed = 20.0f;
  }

  float _resMoveSpeed = 0.0f;
  Vector3 _position = Vector3.zero;
  public override void Update()
  {
    base.Update();

    if (LevelLoader.Instance.IsGameOver)
    {
      return;
    }

    _position = BehaviourRef.transform.position;

    _heading = (_destination - BehaviourRef.transform.position);
    _magnitude = _heading.magnitude;
    _dir = _heading / _magnitude;

    FindEnemies(GlobalConstants.CellHeavyMinRange, GlobalConstants.CellHeavyMaxRange);

    if (_enemyFound == null)
    { 
      _resMoveSpeed = Time.smoothDeltaTime * (GlobalConstants.HeavyMoveSpeed * _moveSpeedModifier);
      _position += (_dir * _resMoveSpeed);
    }
    else
    {
      if (_attackTimer >= GlobalConstants.HeavyAttackTimeout)
      {
        _attackTimer = 0.0f;

        LockTarget(()=>
        {
          LevelLoader.Instance.SpawnSplashBullet(_position, _enemyPosition3D, BehaviourRef, _enemyFound, GlobalConstants.CellHeavyDamage, GlobalConstants.CellHeavySplashRadius);
        });
      }
    }

    if (_attackTimer < GlobalConstants.HeavyAttackTimeout)
    {
      _attackTimer += Time.smoothDeltaTime;
    }

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    Coordinates.Set(_gridX, _gridY);

    BehaviourRef.transform.position = _position;
  }
}
