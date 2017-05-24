using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Callback();
public delegate void CallbackO(object o);
public delegate void CallbackI2(Int2 coords);

public static class Utils
{
  public static int BlockDistance(Int2 point1, Int2 point2)
  {
    int cost = ( Mathf.Abs(point1.Y - point2.Y) + Mathf.Abs(point1.X - point2.X) );

    //Debug.Log(string.Format("Manhattan distance remaining from {0} to {1}: {2}", point.ToString(), end.ToString(), cost));

    return cost;
  }

  // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm

  public static List<Int2> BresenhamLine(Int2 p1, Int2 p2) 
  {
    List<Int2> res = new List<Int2>();

    int x = p1.X;
    int y = p1.Y;

    int w = p2.X - p1.X;
    int h = p2.Y - p1.Y;
    int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
    if (w < 0) dx1 = -1 ; else if (w > 0) dx1 = 1;
    if (h < 0) dy1 = -1 ; else if (h > 0) dy1 = 1;
    if (w < 0) dx2 = -1 ; else if (w > 0) dx2 = 1;
    int longest = Mathf.Abs(w);
    int shortest = Mathf.Abs(h);
    if (!(longest > shortest)) 
    {
      longest = Mathf.Abs(h);
      shortest = Mathf.Abs(w);
      if (h < 0)
      {
        dy2 = -1; 
      }
      else if (h > 0)
      {
        dy2 = 1;
      }

      dx2 = 0;            
    }

    int numerator = longest >> 1;

    for (int i = 0; i <= longest; i++) 
    {
      res.Add(new Int2(x, y));
      numerator += shortest;

      if (!(numerator<longest)) 
      {
        numerator -= longest;
        x += dx1;
        y += dy1;
      } 
      else 
      {
        x += dx2;
        y += dy2;
      }
    }

    return res;
  }
}
