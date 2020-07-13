using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  public class AdvancedPolygon : AdvancedShape {
    public AnimationAttribute<int[]> Points { get; set; }

    public AdvancedPolygon(AnimationAttribute<int[]> points) {
      Points = points;
    }

    public void WriteValueJson(JsonTextWriter writer, AdvancedShape compare) {
      if (compare == null) {
        writer.WritePropertyName("points");
        writer.WriteStartArray();
        foreach (int p in Points.Value) {
          writer.WriteValue(p);
        }
        writer.WriteEndArray();
      } else {
        AdvancedPolygon p = (AdvancedPolygon)compare;

        if (!CompareAttributeValues(Points.Value, p.Points.CurrValue)) {
          writer.WritePropertyName("points");
          writer.WriteStartArray();
          foreach (int val in Points.Value) {
            writer.WriteValue(val);
          }
          writer.WriteEndArray();
        }
      }
    }

    public void WriteValueAtJson(int i, JsonTextWriter writer, Dictionary<string, int[]> compare) {
      if (compare == null) {
        int[] points = Points.GetValueAt(i);
        writer.WritePropertyName("points");
        writer.WriteValue(points);
        Points.CurrValue = points;
      } else {
        compare.TryGetValue("points", out int[] prevPoints);

        int[] points = Points.GetValueAt(i);
        if (prevPoints != points) {
          writer.WritePropertyName("points");
          writer.WriteStartArray();
          foreach (int val in points) {
            writer.WriteValue(val);
          }
          writer.WriteEndArray();
        }
        Points.CurrValue = points;
      }
    }

    public Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "points", Points.CurrValue },
      };
    }

    private bool CompareAttributeValues(int[] a, int[] b) {
      if (!a.Length.Equals(b.Length))
        return false;

      for (int i = 0; i < a.Length; i++) {
        if (a[i] != b[i])
          return false;
      }

      return true;
    }

    public bool AllValues() {
      return Points.Function == null;
    }
  }
}
