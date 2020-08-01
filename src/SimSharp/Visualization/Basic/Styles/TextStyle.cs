using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Styles {
  public class TextStyle : Style {
    public string Text { get; }

    public TextStyle(string fill = "", string stroke = "", int strokeWidth = -1, string text = "text") : base (fill, stroke, strokeWidth) {
      Text = text;
    }

    public override void WriteJson(string name, JsonTextWriter writer, Style compare) {
      base.WriteJson(name, writer, compare);

      if (compare == null) {
        writer.WritePropertyName("text");
        writer.WriteValue(Text);
      } else {
        TextStyle textCompare = (TextStyle)compare;
        if (!Text.Equals(textCompare.Text)) {
          writer.WritePropertyName("text");
          writer.WriteValue(Text);
        }
      }
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        TextStyle ts = (TextStyle)obj;
        return base.Equals(obj) && Text.Equals(ts.Text);
      }
    }

    public override int GetHashCode() {
      return (Fill, Stroke, StrokeWidth, Text).GetHashCode();
    }
  }
}
