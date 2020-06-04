using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  public class AdvancedGroup : AdvancedShape {
    public AnimationAttribute<int> X { get; set; }
    public AnimationAttribute<int> Y { get; set; }
    public AnimationAttribute<int> Width { get; set; }
    public AnimationAttribute<int> Height { get; set; }

    public AdvancedGroup(AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public override void WriteValueJson(JsonTextWriter writer, AdvancedShape compare) {
      if (compare == null) {
        writer.WritePropertyName("x");
        writer.WriteValue(X.Value);

        writer.WritePropertyName("y");
        writer.WriteValue(Y.Value);

        writer.WritePropertyName("width");
        writer.WriteValue(Width.Value);

        writer.WritePropertyName("height");
        writer.WriteValue(Height.Value);
      } else {
        AdvancedGroup r = (AdvancedGroup)compare;

        if (r.X.CurrValue != X.Value) {
          writer.WritePropertyName("x");
          writer.WriteValue(X.Value);
        }

        if (r.Y.CurrValue != Y.Value) {
          writer.WritePropertyName("y");
          writer.WriteValue(Y.Value);
        }

        if (r.Width.CurrValue != Width.Value) {
          writer.WritePropertyName("width");
          writer.WriteValue(Width.Value);
        }

        if (r.Height.CurrValue != Height.Value) {
          writer.WritePropertyName("height");
          writer.WriteValue(Height.Value);
        }
      }
    }

    public override void WriteValueAtJson(int i, JsonTextWriter writer, Dictionary<string, int[]> compare) {
      if (compare == null) {
        int x = X.GetValueAt(i);
        writer.WritePropertyName("x");
        writer.WriteValue(x);
        X.CurrValue = x;

        int y = Y.GetValueAt(i);
        writer.WritePropertyName("y");
        writer.WriteValue(y);
        Y.CurrValue = y;

        int width = Width.GetValueAt(i);
        writer.WritePropertyName("width");
        writer.WriteValue(width);
        Width.CurrValue = width;

        int height = Height.GetValueAt(i);
        writer.WritePropertyName("height");
        writer.WriteValue(height);
        Height.CurrValue = height;
      } else {
        compare.TryGetValue("x", out int[] prevX);
        compare.TryGetValue("y", out int[] prevY);
        compare.TryGetValue("width", out int[] prevWidht);
        compare.TryGetValue("height", out int[] prevHeight);

        int x = X.GetValueAt(i);
        if (prevX[0] != x) {
          writer.WritePropertyName("x");
          writer.WriteValue(x);
        }
        X.CurrValue = x;

        int y = Y.GetValueAt(i);
        if (prevY[0] != y) {
          writer.WritePropertyName("y");
          writer.WriteValue(y);
        }
        Y.CurrValue = y;

        int width = Width.GetValueAt(i);
        if (prevWidht[0] != width) {
          writer.WritePropertyName("width");
          writer.WriteValue(width);
        }
        Width.CurrValue = width;

        int height = Height.GetValueAt(i);
        if (prevHeight[0] != height) {
          writer.WritePropertyName("height");
          writer.WriteValue(height);
        }
        Height.CurrValue = height;
      }
    }

    public override Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X.CurrValue } },
        { "y", new int[] { Y.CurrValue } },
        { "width", new int[] { Width.CurrValue } },
        { "height", new int[] { Height.CurrValue } }
      };
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
