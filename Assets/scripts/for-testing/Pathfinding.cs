using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pathfinding : MonoBehaviour 
{
  public Transform Holder;

  public GameObject PathfindingCellPrefab;
  public GameObject Actor;

  const int MapSize = 16;

  MapObject[,] _map = new MapObject[MapSize, MapSize];

  PathfindingCell[,] _pathfindingMap = new PathfindingCell[MapSize, MapSize];

  void Start()
  {
    for (int x = 0; x < MapSize; x++)
    { 
      for (int y = 0; y < MapSize; y++)
      {
        _map[x, y] = new MapObject();

        GameObject go = Instantiate(PathfindingCellPrefab, new Vector3(x, y, 0.0f), Quaternion.identity, Holder);
        _pathfindingMap[x, y] = go.GetComponent<PathfindingCell>();

        _pathfindingMap[x, y].SetMapOverlay(".");
      }
    }

    PlaceObstacle(1, 4);
    PlaceObstacle(2, 4);
    PlaceObstacle(3, 4);
    PlaceObstacle(4, 4);
    PlaceObstacle(4, 5);
    PlaceObstacle(4, 6);
    PlaceObstacle(4, 7);
    PlaceObstacle(3, 7);
    PlaceObstacle(2, 7);
    PlaceObstacle(1, 7);

    PlaceTarget(MapSize - 2, MapSize - 2);

    RefreshMap();

    PlaceActor(0, 3);
  }

  void PlaceActor(int x, int y)
  {
    Vector3 pos = new Vector3(x, y, 0.0f);
    Actor.transform.position = pos;

    StartCoroutine(FindPathRoutine());
  }

  IEnumerator FindPathRoutine()
  {
    while ((int)Actor.transform.position.x != _targetPos.X
        || (int)Actor.transform.position.y != _targetPos.Y)
    {
      Int2 currentPos = new Int2(Actor.transform.position.x, Actor.transform.position.y);

      int currentWeight = _map[currentPos.X, currentPos.Y].Weight;

      Int2 newPos = FindNextCell(currentPos, currentWeight);
      if (newPos != null)
      {  
        Vector3 pos = new Vector3(newPos.X, newPos.Y, 0.0f);
        Actor.transform.position = pos;

        float timer = 0.0f;
        while (timer < 0.5f)
        {
          timer += Time.smoothDeltaTime;

          yield return null;
        }
      }
      else
      {
        yield break;
      }
    }

    yield return null;
  }

  Int2 FindNextCell(Int2 pos, int weight)
  {
    int lx = pos.X - 1;
    int ly = pos.Y - 1;
    int hx = pos.X + 1;
    int hy = pos.Y + 1;

    int w = weight;
    Int2 newPos = new Int2(pos);

    for (int x = lx; x <= hx; x++)
    {
      for (int y = ly; y <= hy; y++)
      {
        if (x < 0 || x > MapSize - 1 || y < 0 || y > MapSize - 1)
        {
          continue;
        }

        if (x == pos.X && y == pos.Y)
        {
          continue;
        }

        if (_map[x, y].Weight < w)
        {        
          w = _map[x, y].Weight;
          newPos.Set(x, y);
        }
      }
    }

    if (w != weight)
    {
      return newPos;
    }

    return null;
  }

  void RefreshMap()
  {
    for (int x = 0; x < MapSize; x++)
    { 
      for (int y = 0; y < MapSize; y++)
      {
        _pathfindingMap[x, y].SetPathOverlay(_map[x, y].Weight.ToString());
      }
    }
  }

  void PlaceObstacle(int x, int y)
  {
    _map[x, y].IsObstacle = true;
    _pathfindingMap[x, y].SetMapOverlay("#");
  }

  Int2 _targetPos = Int2.Zero;
  void PlaceTarget(int x, int y)
  {
    _targetPos.Set(x, y);

    _map[x, y].IsTarget = true;

    _pathfindingMap[x, y].SetMapOverlay("X");

    GenerateField(new Int2(x, y));
  }

  void GenerateField(Int2 pos)
  {
    for (int x = 0; x < MapSize; x++)
    { 
      for (int y = 0; y < MapSize; y++)
      {        
        if (_map[x, y].IsObstacle)
        {
          _map[x, y].Weight = Utils.BlockDistance(pos, new Int2(x, y)) + 500;
        }
        else
        {
          _map[x, y].Weight = Utils.BlockDistance(pos, new Int2(x, y));
        }
      }
    }
  }

  class MapObject
  {
    public int Weight = 0;
    public bool IsObstacle = false;
    public bool IsTarget = false;
  }
}
