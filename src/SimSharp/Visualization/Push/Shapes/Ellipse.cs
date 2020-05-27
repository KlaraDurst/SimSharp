using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Push.Shapes {
  public class Ellipse : Shape {
    public int Cx { get; private set; }
    public int Cy { get; private set; }

    public int Rx { get; }
    public int Ry { get; }

    public Ellipse(int cx, int cy, int rx, int ry) {
      Cx = cx;
      Cy = cy;
      Rx = rx;
      Ry = ry;
    }

    public override Dictionary<string, int[]> GetAttributes() {
      return new Dictionary<string, int[]> {
        { "cx", new int[] { Cx } },
        { "cy", new int[] { Cy } },
        { "rx", new int[] { Rx } },
        { "ry", new int[] { Ry } }
      };
    }

    public override bool CompareAttributeValues(int[] a, int[] b) {
      return a[0] == b[0];
    }

    public override bool CompareAttributeValues(List<int> a, int[] b) {
      return a[0] == b[0];
    }

    public override void MoveRight(int space) {
      Cx += space;
    }

    public override Shape Copy() {
      return new Ellipse(Cx, Cy, Rx, Ry);
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      }
      else {
        Ellipse e = (Ellipse)obj;
        return (Cx == e.Cx) && (Cy == e.Cy) && (Rx == e.Rx) && (Ry == e.Ry);
      }
    }

    public override int GetHashCode() {
      return (Cx, Cy, Rx, Ry).GetHashCode();
    }
  }
}
