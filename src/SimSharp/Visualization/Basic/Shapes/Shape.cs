using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
  abstract public class Shape {
    abstract public void WriteJson(JsonTextWriter writer, Shape compare);
    abstract public Dictionary<string, int[]> GetAttributes();
    abstract public bool CompareAttributeValues(int[] a, int[] b);
    abstract public bool CompareAttributeValues(List<int> a, int[] b);
    abstract public void MoveUp(int space);
    abstract public void MoveRight(int space);
    abstract public void MoveDown(int space);
    abstract public void MoveLeft(int space);
    abstract public Shape Copy();
    abstract public Shape CopyAndSet(Dictionary<string, int[]> attributes);
  }
}
