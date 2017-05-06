using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drone - occupies grid cell, does nothing. 
/// Used in construction and spawning of other cells.
/// </summary>
public class CellDrone : CellBaseClass 
{
  public CellDrone()
  {
    Type = GlobalConstants.CellType.DRONE;
    Hitpoints = GlobalConstants.CellDroneHitpoints;
  }

  Color _color = Color.white;
  Material _m;
  public override void InitBehaviour()
  {
    _phaseDuration = 3.0f / 4.0f;
    _animationSpeed = 0.2f / _phaseDuration;

    _m = BehaviourRef.GetComponentInChildren<Renderer>().material;

    _color.r = _m.color.r;
    _color.g = _m.color.g;
    _color.b = _m.color.b;
    _color.a = _m.color.a;

    _colorDecreaseFactor = 1.0f / GlobalConstants.AbandonedDroneLifetimeSeconds;
  }

  float _lifeTimer = 0.0f;
  float _colorDecreaseFactor = 0.0f;
  public override void Update()
  {
    base.Update();

    PlayAnimation();

    if (!FindBaseOrColonyAround())
    {
      _lifeTimer += Time.smoothDeltaTime;

      _color.a -= Time.smoothDeltaTime * _colorDecreaseFactor;

      _color.a = Mathf.Clamp(_color.a, 0.0f, 1.0f);

      if (_lifeTimer > GlobalConstants.AbandonedDroneLifetimeSeconds)
      {
        BehaviourRef.DestroySelf();
      }
    }
    else
    {
      _color.a = 1.0f;
      _lifeTimer = 0.0f;
    }
      
    _m.color = _color;
  }

  bool FindBaseOrColonyAround()
  {    
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x >= 0 && x < LevelLoader.Instance.MapSize
         && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          if (LevelLoader.Instance.Map[x, y].CellHere != null
            && (LevelLoader.Instance.Map[x, y].CellHere.Type == GlobalConstants.CellType.COLONY 
             || LevelLoader.Instance.Map[x, y].CellHere.Type == GlobalConstants.CellType.BASE)
            && LevelLoader.Instance.Map[x, y].CellHere.OwnerId == OwnerId)
          {
            return true;
          }
        }
      }
    }

    return false;
  }
}


