using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
  public interface Shape {
    void WriteJson(JsonTextWriter writer, Shape compare);
    Dictionary<string, int[]> GetAttributes();
    bool CompareAttributeValues(int[] a, int[] b);
    bool CompareAttributeValues(List<int> a, int[] b);
    void MoveUp(int space);
    void MoveRight(int space);
   void MoveDown(int space);
    void MoveLeft(int space);
    Shape Copy();
    Shape CopyAndSet(Dictionary<string, int[]> attributes);
  }
}
