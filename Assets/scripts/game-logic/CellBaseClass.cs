using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for cells logic.
/// </summary>
public abstract class CellBaseClass
{
  public int Hitpoints = 3;
  public int OwnerId = -1;
  public int Priority = 1;
  public bool IsBeingAttacked = false;

  public Int2 Coordinates = Int2.Zero;
  public Vector3 WorldCoordinates = Vector3.zero;

  public GlobalConstants.CellType Type;

  // Reference to a behaviour for this cell (also used for translation)
  public CellBehaviour BehaviourRef;

  // Reference to a model for various effects (mainly rotation). 
  // Usually it's child of a game object. Because we use orthographic projection,
  // to deal with Z depth for all objects on a scene, we can adjust Z value of a child object in a prefab.
  public Transform ModelTransform;

  // How much cell is skewing (overriden in InitBehaviour of child class)
  protected float _animationSpeed = 0.0f;

  // To animate using scaling, we perform 4 steps:
  // (thus the magic number 4 in phase duration calculation of drone cell, for example)
  //
  // Starting from (1, 1, 1) scale:
  //
  // 1) Increasing X, decreasing Y
  // 2) Decreasing X, increasing Y (this returns to normal scale)
  // 3) Increasing Y, decreasing X
  // 4) Decreasing Y, increasing X (back to normal scale again)
  //
  // This is the duration of one such animation phase in seconds (overriden in InitBehaviour of child class)
  protected float _phaseDuration = 0.0f;

  protected float _zRotationSpeed = 10.0f;

  protected Vector3 _hitpointsBarSize = new Vector3(0.5f, 0.05f);
  Vector3 _hpBarNewSize = new Vector3(0.5f, 0.05f);
  Color _hitPointsBarColor = Color.green;
  int _halfHitpoints = 0;
  public void CalculateHitpointsBar(int maxHitpoints)
  {
    if (_halfHitpoints == 0)
    {
      _halfHitpoints = (maxHitpoints % 2 == 0) ? (maxHitpoints / 2) : (maxHitpoints + 1) / 2;
    }
    
    _hpBarNewSize.x = (_hitpointsBarSize.x / maxHitpoints) * Hitpoints;

    _hitPointsBarColor.r = ((maxHitpoints - Hitpoints) < _halfHitpoints) ? (((maxHitpoints - Hitpoints) % _halfHitpoints) * (1.0f / (float)_halfHitpoints)) : 1.0f;
    _hitPointsBarColor.g = ((maxHitpoints - Hitpoints) < _halfHitpoints) ? 1.0f : 1.0f - (((maxHitpoints - Hitpoints) % _halfHitpoints) * (1.0f / (float)_halfHitpoints));

    BehaviourRef.HitpointsBar.rectTransform.sizeDelta = _hpBarNewSize;
    BehaviourRef.HitpointsBar.color = _hitPointsBarColor;
  }

  float _hitpointsBarTimer = 0.0f;
  public virtual void Update()
  {    
    if (BehaviourRef != null)
    {
      WorldCoordinates.Set(BehaviourRef.transform.position.x, BehaviourRef.transform.position.y, 0.0f);

      if (_showHitpointsBarFlag)
      {
        _hitpointsBarTimer += Time.smoothDeltaTime;

        if (_hitpointsBarTimer > 3.0f)
        {
          _showHitpointsBarFlag = false;
          _hitpointsBarTimer = 0.0f;
          BehaviourRef.HitpointsBar.gameObject.SetActive(false);
        }
      }

      if (Type != GlobalConstants.CellType.DRONE)
      { 
        CalculateHitpointsBar(GlobalConstants.CellHitpointsByType[Type]);
      }

      if (Hitpoints <= 0)
      {
        BehaviourRef.DestroySelf();
      }
    }
  }

  bool _showHitpointsBarFlag = false;
  public void ShowHitpointsBar()
  {
    if (BehaviourRef.HitpointsBar != null)
    {
      BehaviourRef.HitpointsBar.gameObject.SetActive(true);
      _showHitpointsBarFlag = true;
      _hitpointsBarTimer = 0.0f;
    }
  }

  protected int _randomRotation = 0;
  protected int _enemyId = 0;
  public int EnemyId
  {
    get { return _enemyId; }
  }

  public virtual void InitBehaviour()
  { 
    if (BehaviourRef.HitpointsBar != null)
    {
      _hitpointsBarSize.x = BehaviourRef.HitpointsBar.rectTransform.sizeDelta.x;
      _hitpointsBarSize.y = BehaviourRef.HitpointsBar.rectTransform.sizeDelta.y;
    }

    int rnd = Random.Range(0, 2);
    _randomRotation = (rnd == 0) ? 1 : -1;

    WorldCoordinates.Set(Coordinates.X, Coordinates.Y, 0.0f);

     // Find enemy ID

    foreach (var b in LevelLoader.Instance.BaseCoordinatesByOwner)
    {
      if (OwnerId != b.Key)
      {
        _enemyId = b.Key;
        break;
      }
    }
  }

  public void ReceiveDamage(int amount)
  {
    ShowHitpointsBar();

    Hitpoints -= amount;
  }

  Int2 _emptyCellPos = Int2.Zero;
  protected Int2 TryToFindEmptyCell()
  {
    int lx = Coordinates.X - 1;
    int ly = Coordinates.Y - 1;
    int hx = Coordinates.X + 1;
    int hy = Coordinates.Y + 1;

    bool cellEmpty = true;

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {        
        if (x >= 0 && x < LevelLoader.Instance.MapSize
          && y >= 0 && y < LevelLoader.Instance.MapSize)
        {
          cellEmpty = false;

          // Check if cell is empty
          if (LevelLoader.Instance.ObjectsMap[x, y] == null && LevelLoader.Instance.LockMap[x, y] == 0)
          { 
            cellEmpty = true;

            // Now check if there are enemy attackers on it
            foreach (var kvp in LevelLoader.Instance.SoldiersMap[x, y])
            {
              if (kvp.Value != null && kvp.Value.CellInstance.OwnerId != OwnerId)
              {              
                cellEmpty = false;
                break;
              }
            }
          }

          if (cellEmpty)
          {
            _emptyCellPos.Set(x, y);
            goto exitLoop;
          }
        }
      }
    }

  exitLoop:

    return (cellEmpty ? _emptyCellPos : null);
  }

  float _timer = 0.0f;
  protected void TryToSpawnDrone()
  {
    var res = TryToFindEmptyCell();
    if (res != null)
    {
      _timer += Time.smoothDeltaTime;

      if (_timer > GlobalConstants.DroneSpawnTimeSeconds)
      {   
        LevelLoader.Instance.PlaceCell(res, GlobalConstants.CellType.DRONE, OwnerId);
        _timer = 0.0f;
      }
    }
    else
    {
      _timer = 0.0f;
    }
  }

  Vector3 _localScale = Vector3.one;
  float _scaleTimer = 0.0f;
  protected void PlayAnimation()
  {
    if ((_scaleTimer > 0.0f && _scaleTimer < _phaseDuration) || (_scaleTimer > _phaseDuration * 3.0f && _scaleTimer < _phaseDuration * 4.0f))
    {
      _localScale.x += Time.smoothDeltaTime * _animationSpeed;
      _localScale.y -= Time.smoothDeltaTime * _animationSpeed;
    }
    else if ((_scaleTimer > _phaseDuration && _scaleTimer < _phaseDuration * 2.0f) || (_scaleTimer > _phaseDuration * 2.0f && _scaleTimer < _phaseDuration * 3.0f))
    {
      _localScale.x -= Time.smoothDeltaTime * _animationSpeed;
      _localScale.y += Time.smoothDeltaTime * _animationSpeed;
    }
    else if (_scaleTimer > _phaseDuration * 4.0f)
    {
      _scaleTimer = 0.0f;
    }

    _scaleTimer += Time.smoothDeltaTime;

    _localScale.x = Mathf.Clamp(_localScale.x, 0.5f, 1.5f);
    _localScale.y = Mathf.Clamp(_localScale.y, 0.5f, 1.5f);

    ModelTransform.localScale = _localScale;

    ModelTransform.Rotate(Vector3.forward, Time.smoothDeltaTime * _zRotationSpeed * _randomRotation);
  }
}
