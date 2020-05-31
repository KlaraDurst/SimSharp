using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
  public class Rect : Shape {
    public int X { get; set; }
    public int Y { get; set; }

    public int Width { get; }
    public int Height { get; }

    public override void WriteJson(JsonTextWriter writer, Shape compare) {
      if (compare == null) {
        writer.WritePropertyName("x");
        writer.WriteValue(X);

        writer.WritePropertyName("y");
        writer.WriteValue(Y);

        writer.WritePropertyName("width");
        writer.WriteValue(Width);

        writer.WritePropertyName("height");
        writer.WriteValue(Height);
      } else {
        Rect r = (Rect)compare;

        if (r.X != X) {
          writer.WritePropertyName("x");
          writer.WriteValue(X);
        }

        if (r.Y != Y) {
          writer.WritePropertyName("y");
          writer.WriteValue(Y);
        }

        if (r.Width != Width) {
          writer.WritePropertyName("width");
          writer.WriteValue(Width);
        }

        if (r.Height != Height) {
          writer.WritePropertyName("height");
          writer.WriteValue(Height);
        }
      }
    }

    public Rect (int x, int y, int width, int height) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public override Dictionary<string, int[]> GetAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X } },
        { "y", new int[] { Y } },
        { "width", new int[] { Width } },
        { "height", new int[] { Height } }
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
      return new Rect(X, Y, Width, Height);
    }

    public override Shape CopyAndSet(Dictionary<string, int[]> attributes) {
      attributes.TryGetValue("x", out int[] x);
      attributes.TryGetValue("y", out int[] y);
      attributes.TryGetValue("width", out int[] width);
      attributes.TryGetValue("height", out int[] height);
      return new Rect(x[0], y[0], width[0], height[0]);
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Rect r = (Rect)obj;
        return (X == r.X) && (Y == r.Y) && (Width == r.Width) && (Height == r.Height);
      }
    }

    public override int GetHashCode() {
      return (X, Y, Width, Height).GetHashCode();
    }
  }
}
