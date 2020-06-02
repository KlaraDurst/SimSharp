using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
  public class Text : Shape {
    public int X { get; set; }
    public int Y { get; set; }

    public int TextLength { get; set; }
    public int FontSize { get; set; }

    public Text(int x, int y, int textLength = default, int fontSize = default) {
      X = x;
      Y = y;
      TextLength = textLength;
      FontSize = fontSize;
    }

    public override void WriteJson(JsonTextWriter writer, Shape compare) {
      if (compare == null) {
        writer.WritePropertyName("x");
        writer.WriteValue(X);

        writer.WritePropertyName("y");
        writer.WriteValue(Y);

        writer.WritePropertyName("textLength");
        writer.WriteValue(TextLength);

        writer.WritePropertyName("font-size");
        writer.WriteValue(FontSize);
      } else {
        Text t = (Text)compare;

        if (t.X != X) {
          writer.WritePropertyName("x");
          writer.WriteValue(X);
        }

        if (t.Y != Y) {
          writer.WritePropertyName("y");
          writer.WriteValue(Y);
        }

        if (t.TextLength != TextLength) {
          writer.WritePropertyName("textLength");
          writer.WriteValue(TextLength);
        }

        if (t.FontSize != FontSize) {
          writer.WritePropertyName("font-size");
          writer.WriteValue(FontSize);
        }
      }
    }

    public override Dictionary<string, int[]> GetAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X } },
        { "y", new int[] { Y } },
        { "textLength", new int[] { TextLength } },
        { "font-size", new int[] { FontSize } }
      };
    }

    public override bool CompareAttributeValues(int[] a, int[] b) {
      return a[0] == b[0];
    }

    public override bool CompareAttributeValues(List<int> a, int[] b) {
      return a[0] == b[0];
    }

    public override void MoveUp(int space) {
      Y -= space;
    }

    public override void MoveRight(int space) {
      X += space;
    }

    public override void MoveDown(int space) {
      Y += space;
    }

    public override void MoveLeft(int space) {
      X -= space;
    }

    public override Shape Copy() {
      return new Text(X, Y, TextLength, FontSize);
    }

    public override Shape CopyAndSet(Dictionary<string, int[]> attributes) {
      attributes.TryGetValue("x", out int[] x);
      attributes.TryGetValue("y", out int[] y);
      attributes.TryGetValue("textLength", out int[] textLength);
      attributes.TryGetValue("font-size", out int[] fontSize);
      return new Text(x[0], y[0], textLength[0], fontSize[0]);
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Text r = (Text)obj;
        return (X == r.X) && (Y == r.Y) && (TextLength == r.TextLength) && (FontSize == r.FontSize);
      }
    }

    public override int GetHashCode() {
      return (X, Y, TextLength, FontSize).GetHashCode();
    }
  }
}
