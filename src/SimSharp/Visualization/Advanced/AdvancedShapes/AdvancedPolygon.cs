using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  public class AdvancedPolygon : AdvancedShape {
    public AnimationAttribute<int[]> Points { get; set; }

    public AdvancedPolygon(AnimationAttribute<int[]> points) {
      Points = points;
    }

    public override Dictionary<string, int[]> GetValueAttributes() {
      return new Dictionary<string, int[]> {
        { "points", Points.Value },
      };
    }

    public override Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "points", Points.CurrValue },
      };
    }

    public override void SetCurrValueAttributes(Dictionary<string, int[]> currValues) {
      currValues.TryGetValue("points", out int[] points);
      Points.CurrValue = points;
    }

    public override Dictionary<string, int[]> GetValueAttributesAt(int i) {
      return new Dictionary<string, int[]> {
        { "points", Points.GetValueAt(i) },
      };
    }

    public override bool CompareAttributeValues(int[] a, int[] b) {
      if (!a.Length.Equals(b.Length))
        return false;

      for (int i = 0; i < a.Length; i++) {
        if (a[i] != b[i])
          return false;
      }

      return true;
    }

    public override bool AllValues() {
      return Points.Function == null;
    }
  }
}
