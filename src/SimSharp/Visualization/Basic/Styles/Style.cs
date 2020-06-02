using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Styles {
  public class Style {
    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }

    public Style(string fill, string stroke, int strokeWidth) {
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
    }

    public virtual void WriteJson(string name, JsonTextWriter writer, Style compare) {
      if (compare == null) {
        writer.WritePropertyName("fill");
        writer.WriteValue(Fill);

        writer.WritePropertyName("stroke");
        writer.WriteValue(Stroke);

        writer.WritePropertyName("stroke-width");
        writer.WriteValue(StrokeWidth);
      } else { 
        if (compare.Fill != Fill) {
          writer.WritePropertyName("fill");
          writer.WriteValue(Fill);
        }

        if (compare.Stroke != Stroke) {
          writer.WritePropertyName("stroke");
          writer.WriteValue(Stroke);
        }

        if (compare.StrokeWidth != StrokeWidth) {
          writer.WritePropertyName("stroke-width");
          writer.WriteValue(StrokeWidth);
        }
      }
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Style s = (Style)obj;
        return (Fill.Equals(s.Fill)) && (Stroke.Equals(s.Stroke)) && (StrokeWidth == s.StrokeWidth);
      }
    }

    public override int GetHashCode() {
      return (Fill, Stroke, StrokeWidth).GetHashCode();
    }
  }
}
