using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Push.Shapes {
  public class Rect : Shape {
    public int X { get; set; }
    public int Y { get; set; }

    public int Width { get; }
    public int Height { get; }

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

    public override void MoveRight(int space) {
      X += space;
    }

    public override Shape Copy() {
      return new Rect(X, Y, Width, Height);
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
