using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Ellipse : Shape {
    public int X { get; set; }
    public int Y { get; set; }

    public int Radius1 { get; set; }
    public int Radius2 { get; set; }

    public Ellipse(string name, int x, int y, int radius1, int radius2) : base(name) {
      X = x;
      Y = y;
      Radius1 = radius1;
      Radius2 = radius2;
    }
  }
}
