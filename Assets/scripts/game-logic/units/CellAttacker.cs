using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellAttacker : CellSoldier
{  
  public CellAttacker() : base()
  { 
    Type = GlobalConstants.CellType.ATTACKER;
    Hitpoints = GlobalConstants.CellAttackerHitpoints;
    Priority = GlobalConstants.CellAttackerPriority;
  }

  public override void InitBehaviour()
  {
    base.InitBehaviour();

    _phaseDuration = 2.0f / 4.0f;
    _animationSpeed = 0.2f / _phaseDuration;
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

    FindEnemies(0.0f, GlobalConstants.CellAttackerRange);

    if (_enemyFound == null)
    { 
      _resMoveSpeed = Time.smoothDeltaTime * (GlobalConstants.AttackerMoveSpeed * _moveSpeedModifier);
      _position += (_dir * _resMoveSpeed);
    }
    else
    {
      if (_attackTimer >= GlobalConstants.AttackerAttackTimeout)
      {
        _attackTimer = 0.0f;

        LockTarget(() =>
        {
          LevelLoader.Instance.SpawnBullet(_position, _enemyPosition3D, BehaviourRef, _enemyFound);
        });
      }
    }

    if (_attackTimer < GlobalConstants.AttackerAttackTimeout)
    {
      _attackTimer += Time.smoothDeltaTime;
    }

    _gridX = Mathf.Round(_position.x);
    _gridY = Mathf.Round(_position.y);

    Coordinates.Set(_gridX, _gridY);

    BehaviourRef.transform.position = _position;
  }
}
