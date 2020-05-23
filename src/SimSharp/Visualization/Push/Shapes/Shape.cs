using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Push.Shapes {
  abstract public class Shape {
    abstract public Dictionary<string, int[]> GetAttributes();
    abstract public bool CompareAttributeValues(int[] a, int[] b);
  }
}
