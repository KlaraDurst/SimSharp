using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedStyles {
  public class AdvancedStyle {
    public class State {
      public string Fill { get; private set;}
      public string Stroke { get; private set; }
      public int StrokeWidth { get; private set; }

      public State() {}

      public State(AdvancedStyle style) {
        Fill = style.Fill.CurrValue;
        Stroke = style.Stroke.CurrValue;
        StrokeWidth = style.StrokeWidth.CurrValue;
      }

      public void SetState(AdvancedStyle style) {
        Fill = style.Fill.CurrValue;
        Stroke = style.Stroke.CurrValue;
        StrokeWidth = style.StrokeWidth.CurrValue;
      }
    }

    public AnimationAttribute<string> Fill { get; set; }
    public AnimationAttribute<string> Stroke { get; set; }
    public AnimationAttribute<int> StrokeWidth { get; set; }

    public AdvancedStyle(AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth) {
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
    }

    public void WriteValueJson(JsonTextWriter writer, AdvancedStyle compare) {
      if (compare == null) {
        writer.WritePropertyName("fill");
        writer.WriteValue(Fill.Value);

        writer.WritePropertyName("stroke");
        writer.WriteValue(Stroke.Value);

        writer.WritePropertyName("stroke-width");
        writer.WriteValue(StrokeWidth.Value);
      } else {
        if (compare.Fill.CurrValue != Fill.Value) {
          writer.WritePropertyName("fill");
          writer.WriteValue(Fill.Value);
        }

        if (compare.Stroke.CurrValue != Stroke.Value) {
          writer.WritePropertyName("stroke");
          writer.WriteValue(Stroke.Value);
        }

        if (compare.StrokeWidth.CurrValue != StrokeWidth.Value) {
          writer.WritePropertyName("stroke-width");
          writer.WriteValue(StrokeWidth.Value);
        }
      }
    }

    public void WriteValueAtJson(int i, JsonTextWriter writer, State compare) {
      if (compare == null) {
        string fill = Fill.GetValueAt(i);
        writer.WritePropertyName("fill");
        writer.WriteValue(fill);
        Fill.CurrValue = fill;

        string stroke = Stroke.GetValueAt(i);
        writer.WritePropertyName("stroke");
        writer.WriteValue(stroke);
        Stroke.CurrValue = stroke;

        int strokeWidth = StrokeWidth.GetValueAt(i);
        writer.WritePropertyName("stroke-width");
        writer.WriteValue(strokeWidth);
        StrokeWidth.CurrValue = strokeWidth;
      } else {
        string fill = Fill.GetValueAt(i);
        if (compare.Fill != fill) {
          writer.WritePropertyName("fill");
          writer.WriteValue(fill);
        }
        Fill.CurrValue = fill;

        string stroke = Stroke.GetValueAt(i);
        if (compare.Stroke != stroke) {
          writer.WritePropertyName("stroke");
          writer.WriteValue(stroke);
        }
        Stroke.CurrValue = stroke;

        int strokeWidth = StrokeWidth.GetValueAt(i);
        if (compare.StrokeWidth != strokeWidth) {
          writer.WritePropertyName("stroke-width");
          writer.WriteValue(strokeWidth);
        }
        StrokeWidth.CurrValue = strokeWidth;
      }
    }

    public bool AllValues() {
      if (Fill.Function != null)
        return false;
      if (Stroke.Function != null)
        return false;
      if (StrokeWidth.Function != null)
        return false;
      return true;
    }
  }
}
