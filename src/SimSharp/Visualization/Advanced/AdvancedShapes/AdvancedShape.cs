using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  abstract public class AdvancedShape {
    public abstract Dictionary<string, int[]> GetValueAttributes();
    public abstract Dictionary<string, int[]> GetCurrValueAttributes();
    public abstract void SetCurrValueAttributes(Dictionary<string, int[]> currValues);
    public abstract Dictionary<string, int[]> GetValueAttributesAt(int i);
    public abstract bool CompareAttributeValues(int[] a, int[] b);
    public abstract bool AllValues();
  }
}
