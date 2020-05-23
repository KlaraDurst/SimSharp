using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull.AdvancedShapes {
  public class AdvancedRect : AdvancedShape{
    public AnimationAttribute<int> X { get; set; }
    public AnimationAttribute<int> Y { get; set; }
    public AnimationAttribute<int> Width { get; set; }
    public AnimationAttribute<int> Height { get; set; }

    public AdvancedRect(AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public override Dictionary<string, int[]> GetValueAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X.Value } },
        { "y", new int[] { Y.Value } },
        { "width", new int[] { Width.Value } },
        { "height", new int[] { Height.Value } }
      };
    }

    public override Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X.CurrValue } },
        { "y", new int[] { Y.CurrValue } },
        { "width", new int[] { Width.CurrValue } },
        { "height", new int[] { Height.CurrValue } }
      };
    }

    public override void SetCurrValueAttributes(Dictionary<string, int[]> currValues) {
      currValues.TryGetValue("x", out int[] x);
      X.CurrValue = x[0];
      currValues.TryGetValue("y", out int[] y);
      Y.CurrValue = y[0];
      currValues.TryGetValue("width", out int[] width);
      Width.CurrValue = width[0];
      currValues.TryGetValue("height", out int[] height);
      Height.CurrValue = height[0];
    }

    public override Dictionary<string, int[]> GetValueAttributesAt(int i) {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X.GetValueAt(i) } },
        { "y", new int[] { Y.GetValueAt(i) } },
        { "width", new int[] { Width.GetValueAt(i) } },
        { "height", new int[] { Height.GetValueAt(i) } }
      };
    }

    public override bool CompareAttributeValues(int[] a, int[] b) {
      return a[0] == b[0];
    }

    public override bool AllValues() {
      if (X.Function != null)
        return false;
      if (Y.Function != null)
        return false;
      if (Width.Function != null)
        return false;
      if (Height.Function != null)
        return false;
      return true;
    }
  }
}
