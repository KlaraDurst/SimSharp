using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Ellipse {
    public int X { get; }
    public int Y { get; }

    public int Radius1 { get; }
    public int Radius2 { get; }

    public Ellipse(int x, int y, int radius1, int radius2) {
      X = x;
      Y = y;
      Radius1 = radius1;
      Radius2 = radius2;
    }
  }
}
