using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Callback();
public delegate void CallbackO(object o);

public static class Utils
{
  public static int BlockDistance(Int2 point1, Int2 point2)
  {
    int cost = ( Mathf.Abs(point1.Y - point2.Y) + Mathf.Abs(point1.X - point2.X) );

    //Debug.Log(string.Format("Manhattan distance remaining from {0} to {1}: {2}", point.ToString(), end.ToString(), cost));

    return cost;
  }
}
