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

  Vector3 _targetPos = Vector3.zero;
  Vector3 _startingPos = Vector3.zero;
  public void SetTarget(Vector3 targetPos, CellBehaviour enemy, float bulletSpeed)
  {
    _targetPos = targetPos;
    _startingPos = transform.position;

    _gridX = Mathf.Round(_targetPos.x);
    _gridY = Mathf.Round(_targetPos.y);

    _gridPos.Set(_gridX, _gridY);

    _enemy = enemy;
    _bulletSpeed = bulletSpeed;

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

      if (_enemy != null)
      {
        _enemy.CellInstance.ReceiveDamage(1);

        if (_enemy.CellInstance.Hitpoints <= 0 && _enemy.CellInstance.Type != GlobalConstants.CellType.DRONE && !_enemy.IsDestroying)
        {
          LevelLoader.Instance.ScoreCountByOwner[_enemy.CellInstance.EnemyId] += GlobalConstants.CellHitpointsByType[_enemy.CellInstance.Type];
        }
      }

      Destroy(gameObject);
    }
	}
}
