using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Int2
{
  int _x = 0;
  int _y = 0;

  public Int2()
  {
    _x = 0;
    _y = 0;
  }

  public Int2(int x, int y)
  {
    _x = x;
    _y = y;
  }

  public Int2(float x, float y)
  {
    _x = (int)x;
    _y = (int)y;
  }

  public Int2(Vector2 v2)
  {
    _x = (int)v2.x;
    _y = (int)v2.y;
  }

  public int X
  {
    set { _x = value; }
    get { return _x; }
  }

  public int Y
  {
    set { _y = value; }
    get { return _y; }
  }

  public static Int2 Zero
  {
    get { return new Int2(); }
  }

  public override string ToString()
  {
    return string.Format("[Int2: X={0}, Y={1}]", X, Y);
  }
}
