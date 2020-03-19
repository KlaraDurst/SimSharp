using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Rectangle : Shape {
    public int X { get; set; }
    public int Y { get; set; }
    
    public int Height { get; set; }
    public int Width { get; set; }

    public Rectangle (string name, int x, int y, int height, int width) : base (name) {
      X = x;
      Y = y;
      Height = height;
      Width = width;
    }
  }
}
