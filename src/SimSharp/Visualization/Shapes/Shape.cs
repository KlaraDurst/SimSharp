using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public abstract class Shape {
    public string Name { get; }

    protected Shape(string name) {
      Name = name;
    }
  }
}
