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

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      }
      else {
        Ellipse e = (Ellipse)obj;
        return (X == e.X) && (Y == e.Y) && (Radius1 == e.Radius1) && (Radius2 == e.Radius2);
      }
    }
  }
}
