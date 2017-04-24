using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour 
{
  public GameObject BulletHitEffectPrefab;

  int _enemyID = -1;

  Vector3 _targetPos = Vector3.zero;
  public void SetTarget(Vector3 targetPos, int enemyID)
  {
    _targetPos = targetPos;

    _gridX = Mathf.Round(_targetPos.x);
    _gridY = Mathf.Round(_targetPos.y);

    _gridPos.Set(_gridX, _gridY);

    _enemyID = enemyID;
  }

  Vector3 _position = Vector3.zero;
  Vector3 _heading = Vector3.zero;
  Vector3 _dir = Vector3.zero;
  float _magnitude = 0.0f, _gridX = 0.0f, _gridY = 0.0f;
  Int2 _gridPos = Int2.Zero;
	void Update () 
	{
    _position = transform.position;

    _heading = (_targetPos - _position);
    _magnitude = _heading.magnitude;
    _dir = _heading / _magnitude;

    if (_magnitude > 0.1f)
    {
      _position += (_dir * Time.smoothDeltaTime * GlobalConstants.BulletSpeed);

      transform.position = _position;
    }
    else
    {
      var hitEffect = (GameObject)Instantiate(BulletHitEffectPrefab, new Vector3(_position.x, _position.y, BulletHitEffectPrefab.transform.position.z), Quaternion.identity, LevelLoader.Instance.GridHolder);

      var cell = LevelLoader.Instance.Map[_gridPos.X, _gridPos.Y].CellHere;
      CellBaseClass soldier = null;

      if (LevelLoader.Instance.Map[_gridPos.X, _gridPos.Y].SoldiersByOwnerHere[_enemyID].Count != 0)
      {
        soldier = LevelLoader.Instance.Map[_gridPos.X, _gridPos.Y].SoldiersByOwnerHere[_enemyID].Peek();
      }

      if (cell != null)
      {
        cell.ReceiveDamage(1);
      }
      else if (soldier != null)
      {
        soldier.ReceiveDamage(1);
      }

      Destroy(gameObject);
    }
	}
}
