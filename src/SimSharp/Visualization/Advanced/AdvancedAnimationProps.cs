using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Advanced.AdvancedShapes;

namespace SimSharp.Visualization.Advanced {
  public class AdvancedAnimationProps {
    public class State {
      public string Fill { get; set; }
      public string Stroke { get; set; }
      public int StrokeWidth { get; set; }

      public State(string fill, string stroke, int strokeWidth) {
        Fill = fill;
        Stroke = stroke;
        StrokeWidth = strokeWidth;
      }
    }

    public AdvancedShape Shape { get; set; }
    public AnimationAttribute<string> Fill { get; set; }
    public AnimationAttribute<string> Stroke { get; set; }
    public AnimationAttribute<int> StrokeWidth { get; set; }
    public AnimationAttribute<bool> Visibility { get; set; }

    public bool Written { get; set; }

    public AdvancedAnimationProps(AdvancedAnimationProps other) 
      : this(other.Shape, other.Fill, other.Stroke, other.StrokeWidth, other.Visibility) { }

    public AdvancedAnimationProps(AdvancedShape shape, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visibility) {
      Shape = shape;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Visibility = visibility;
      Written = false;
    }
    
    public void WriteValueJson(JsonTextWriter writer, bool currVisible, AdvancedAnimationProps compare) {
      if (compare == null) {
        writer.WritePropertyName("fill");
        writer.WriteValue(Fill.Value);

        writer.WritePropertyName("stroke");
        writer.WriteValue(Stroke.Value);

        writer.WritePropertyName("stroke-width");
        writer.WriteValue(StrokeWidth.Value);

        writer.WritePropertyName("visibility");
        writer.WriteValue(Visibility.Value);

        Shape.WriteValueJson(writer, null);
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

        if (!currVisible) {
          writer.WritePropertyName("visibility");
          writer.WriteValue(Visibility.Value);
        }

        Shape.WriteValueJson(writer, compare.Shape);
      }
      Written = true;
    }

    public void WriteValueAtJson(int i, JsonTextWriter writer, bool currVisible, State compare) {
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

        writer.WritePropertyName("visibility");
        writer.WriteValue(true);
        Visibility.CurrValue = true;
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

        if (!currVisible) {
          writer.WritePropertyName("visibility");
          writer.WriteValue(true);
        }
        Visibility.CurrValue = true;
      }
      Written = true;
    }

    public bool AllValues() {
      if (!Shape.AllValues())
        return false;
      if (Fill.Function != null)
        return false;
      if (Stroke.Function != null)
        return false;
      if (StrokeWidth.Function != null)
        return false;
      if (Visibility.Function != null)
        return false;
      return true;
    }
  }
}
