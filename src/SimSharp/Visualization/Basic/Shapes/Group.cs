﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
  public class Group : Shape {
    public int X { get; private set; }
    public int Y { get; private set; }

    public int Width { get; }
    public int Height { get; }

    public Group(int x, int y, int width, int height) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public void WriteJson(JsonTextWriter writer, Shape compare) {
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
        Group g = (Group)compare;

        if (g.X != X) {
          writer.WritePropertyName("x");
          writer.WriteValue(X);
        }

        if (g.Y != Y) {
          writer.WritePropertyName("y");
          writer.WriteValue(Y);
        }

        if (g.Width != Width) {
          writer.WritePropertyName("width");
          writer.WriteValue(Width);
        }

        if (g.Height != Height) {
          writer.WritePropertyName("height");
          writer.WriteValue(Height);
        }
      }
    }

    public Dictionary<string, int[]> GetAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X } },
        { "y", new int[] { Y } },
        { "width", new int[] { Width } },
        { "height", new int[] { Height } }
      };
    }

    public bool CompareAttributeValues(int[] a, int[] b) {
      return a[0] == b[0];
    }

    public bool CompareAttributeValues(List<int> a, int[] b) {
      return a[0] == b[0];
    }

    public void MoveUp(int space) {
      Y -= space;
    }

    public void MoveRight(int space) {
      X += space;
    }

    public void MoveDown(int space) {
      Y += space;
    }

    public void MoveLeft(int space) {
      X -= space;
    }

    public Shape Copy() {
      return new Group(X, Y, Width, Height);
    }

    public Shape CopyAndSet(Dictionary<string, int[]> attributes) {
      attributes.TryGetValue("x", out int[] x);
      attributes.TryGetValue("y", out int[] y);
      attributes.TryGetValue("width", out int[] width);
      attributes.TryGetValue("height", out int[] height);
      return new Group(x[0], y[0], width[0], height[0]);
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Group r = (Group)obj;
        return (X == r.X) && (Y == r.Y) && (Width == r.Width) && (Height == r.Height);
      }
    }

    public override int GetHashCode() {
      return (X, Y, Width, Height).GetHashCode();
    }
  }
}
