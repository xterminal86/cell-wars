using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attacker's bullet.
/// </summary>
public class Bullet : MonoBehaviour 
{
  public GameObject BulletHitEffectPrefab;

  CellBehaviour _enemy;

  int _bulletOwnerId = -1;
  int _baseDamage = 0;

  bool _isSlpash = false;

  float _splashRadius = 1.0f;

  Vector3 _targetPos = Vector3.zero;
  Vector3 _startingPos = Vector3.zero;
  public void SetTarget(Vector3 targetPos, CellBehaviour owner, CellBehaviour enemy, float bulletSpeed, int baseDamage)
  {
    _targetPos = targetPos;
    _startingPos = transform.position;

    _gridX = Mathf.Round(_targetPos.x);
    _gridY = Mathf.Round(_targetPos.y);

    _gridPos.Set(_gridX, _gridY);

    _enemy = enemy;
    _bulletSpeed = bulletSpeed;

    _bulletOwnerId = owner.CellInstance.OwnerId;
    _baseDamage = baseDamage;
  }

  public void SetTargetSplash(Vector3 targetPos, CellBehaviour owner, CellBehaviour enemy, float bulletSpeed, int baseDamage, float splashRadius)
  {
    _isSlpash = true;

    _targetPos = targetPos;
    _startingPos = transform.position;

    _gridX = Mathf.Round(_targetPos.x);
    _gridY = Mathf.Round(_targetPos.y);

    _gridPos.Set(_gridX, _gridY);

    _enemy = enemy;
    _bulletSpeed = bulletSpeed;

    _bulletOwnerId = owner.CellInstance.OwnerId;
    _baseDamage = baseDamage;
    _splashRadius = splashRadius;
  }

  Vector3 _position = Vector3.zero;
  float _gridX = 0.0f, _gridY = 0.0f, _bulletSpeed;
  Int2 _gridPos = Int2.Zero;
  float _interpolant = 0.0f;
	void Update () 
	{
    // If FPS drops, bullet can overshoot, so we use lerp and rely on interpolant value
    // instead of distance magnitude between start and end points.
    _position = Vector3.Lerp(_startingPos, _targetPos, _interpolant);

    transform.position = _position;

    _interpolant += Time.smoothDeltaTime * _bulletSpeed;

    if (_interpolant >= 1.0f)
    {
      Instantiate(BulletHitEffectPrefab, new Vector3(_position.x, _position.y, BulletHitEffectPrefab.transform.position.z), Quaternion.identity, LevelLoader.Instance.GridHolder);

      if (!_isSlpash)
      {
        ProcessSingleShot();
      }
      else
      {
        ProcessSplashShot();
      }

      Destroy(gameObject);
    }
	}

  void ProcessSingleShot()
  {    
    // If enemy this bullet intended for was already killed, search if there is another one at the same spot
    if (_enemy == null)
    {
      // Check soldiers first
      foreach (var kvp in LevelLoader.Instance.SoldiersMap[_gridPos.X, _gridPos.Y])
      {
        if (kvp.Value != null)
        {
          _enemy = kvp.Value;
          break;
        }
      }          

      if (_enemy == null 
        && LevelLoader.Instance.ObjectsMap[_gridPos.X, _gridPos.Y] != null 
        && LevelLoader.Instance.ObjectsMap[_gridPos.X, _gridPos.Y].CellInstance != null
        && LevelLoader.Instance.ObjectsMap[_gridPos.X, _gridPos.Y].CellInstance.OwnerId != _bulletOwnerId)
      {
        _enemy = LevelLoader.Instance.ObjectsMap[_gridPos.X, _gridPos.Y];
      }
    }

    // For some reason there may be a null reference exception for CellInstance
    if (_enemy != null && _enemy.CellInstance != null)
    {
      // If during bullet flight target moved outside bullet "range", don't damage it
      float d = Vector3.Distance(transform.position, _enemy.transform.position);

      if (d < 0.3f)
      {
        _enemy.CellInstance.ReceiveDamage(1);

        if (_enemy.CellInstance.Hitpoints <= 0 
         && _enemy.CellInstance.Type != GlobalConstants.CellType.DRONE 
         && _enemy.CellInstance.Type != GlobalConstants.CellType.WALL 
         && !_enemy.IsDestroying)
        {
          LevelLoader.Instance.ScoreCountByOwner[_enemy.CellInstance.EnemyId] += GlobalConstants.DroneCostByType[_enemy.CellInstance.Type];
        }
      }
    }
  }

  void ProcessSplashShot()
  {
    int lx = _gridPos.X - Mathf.CeilToInt(_splashRadius);
    int ly = _gridPos.Y - Mathf.CeilToInt(_splashRadius);
    int hx = _gridPos.X + Mathf.CeilToInt(_splashRadius);
    int hy = _gridPos.Y + Mathf.CeilToInt(_splashRadius);

    float distance = 0.0f;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
          {
            if (kvp.Value != null)
            {
              distance = Vector3.Distance(transform.position, kvp.Value.transform.position);

              if (distance > _splashRadius)
              {
                continue;
              }

              int damageInflicted = _baseDamage - Mathf.RoundToInt(((distance / _splashRadius) * _baseDamage));

              if (damageInflicted > 0)
              {                
                kvp.Value.CellInstance.ReceiveDamage(damageInflicted);

                if (kvp.Value.CellInstance.Hitpoints <= 0 
                 && kvp.Value.CellInstance.Type != GlobalConstants.CellType.DRONE 
                 && kvp.Value.CellInstance.Type != GlobalConstants.CellType.WALL 
                 && !kvp.Value.IsDestroying)
                {
                  LevelLoader.Instance.ScoreCountByOwner[kvp.Value.CellInstance.EnemyId] += GlobalConstants.DroneCostByType[kvp.Value.CellInstance.Type];
                }
              }
            }
          }

          if (LevelLoader.Instance.ObjectsMap[x, y] != null && LevelLoader.Instance.ObjectsMap[x, y].CellInstance != null)
          {
            distance = Vector3.Distance(transform.position, LevelLoader.Instance.ObjectsMap[x, y].transform.position);

            if (distance > _splashRadius)
            {
              continue;
            }

            int damageInflicted = _baseDamage - Mathf.RoundToInt(((distance / _splashRadius) * _baseDamage));

            if (damageInflicted > 0)
            {
              LevelLoader.Instance.ObjectsMap[x, y].CellInstance.ReceiveDamage(damageInflicted);

              if (LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Hitpoints <= 0 
                && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type != GlobalConstants.CellType.DRONE 
                && LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type != GlobalConstants.CellType.WALL 
                && !LevelLoader.Instance.ObjectsMap[x, y].IsDestroying)
              {
                LevelLoader.Instance.ScoreCountByOwner[LevelLoader.Instance.ObjectsMap[x, y].CellInstance.EnemyId] += GlobalConstants.DroneCostByType[LevelLoader.Instance.ObjectsMap[x, y].CellInstance.Type];
              }
            }
          }
        }
      }
    }
  }
}
