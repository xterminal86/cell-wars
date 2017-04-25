using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
  public Int2 Coordinates = Int2.Zero;

  public CellBaseClass CellHere;

  public Dictionary<int, Queue<CellBaseClass>> SoldiersByOwnerHere = new Dictionary<int, Queue<CellBaseClass>>();

  public GridCell()
  {
    // FIXME: hardcoded owner IDs

    SoldiersByOwnerHere[0] = new Queue<CellBaseClass>();
    SoldiersByOwnerHere[1] = new Queue<CellBaseClass>();
  }
}
