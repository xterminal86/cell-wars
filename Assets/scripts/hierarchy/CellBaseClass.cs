using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBaseClass
{
  public int OwnerId = -1;

  public Int2 Coordinates = Int2.Zero;

  public GlobalConstants.CellType Type;

  public virtual void Update()
  {
  }
}
