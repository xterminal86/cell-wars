using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
  public GameObject GridCellFullPrefab;
  public GameObject GridCellTPrefab;
  public GameObject GridCellCornerPrefab;

  public Transform GridHolder;

  const int MapSize = 20;

  void Start()
  {
    for (int x = 0; x < MapSize; x++)
    {
      for (int y = 0; y < MapSize; y++)
      {
        InstantiateCellPrefab(x, y);
      }
    }
  }

  void InstantiateCellPrefab(int x, int y)
  {
    int lx = x - 1;
    int ly = y - 1;
    int hx = x + 1;
    int hy = y + 1;

    Vector3 eulerAngles = Vector3.zero;

    GameObject prefabToInstantiate;

    // Corner, T shaped and full cells

    if ((lx < 0 && ly < 0)
     || (hx > MapSize - 1 && ly < 0)
     || (hx > MapSize - 1 && hy > MapSize - 1)
     || (lx < 0 && hy > MapSize - 1))
    {
      prefabToInstantiate = GridCellCornerPrefab;

      if (lx < 0 && ly < 0)
      {
        eulerAngles.z = 0.0f;
      }
      else if (hx > MapSize - 1 && ly < 0)
      {
        eulerAngles.z = 90.0f;
      }
      else if (hx > MapSize - 1 && hy > MapSize - 1)
      {
        eulerAngles.z = 180.0f;
      }
      else if (lx < 0 && hy > MapSize - 1)
      {
        eulerAngles.z = 270.0f;
      }
    }
    else if (ly < 0 || hx > MapSize - 1 || hy > MapSize - 1 || lx < 0)
    {
      prefabToInstantiate = GridCellTPrefab;

      if (ly < 0)
      {
        eulerAngles.z = 0.0f;
      }
      else if (hx > MapSize - 1)
      {
        eulerAngles.z = 90.0f;
      }
      else if (hy > MapSize - 1)
      {
        eulerAngles.z = 180.0f;
      }
      else if (lx < 0)
      {
        eulerAngles.z = 270.0f;
      }
    }
    else
    {
      prefabToInstantiate = GridCellFullPrefab;
    }

    Quaternion rotation = Quaternion.Euler(eulerAngles);

    Instantiate(prefabToInstantiate, new Vector3(x, y, 0.0f), rotation, GridHolder);
  }
}
