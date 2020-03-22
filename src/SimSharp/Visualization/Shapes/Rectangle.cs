using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Rectangle {
    public int X { get; }
    public int Y { get; }

    public int Width { get; }
    public int Height { get; }

    public Rectangle (int x, int y, int width, int height) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }
  }
}
