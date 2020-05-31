using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
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

    public override void WriteJson(JsonTextWriter writer, Shape compare) {
      if (compare == null) {
        writer.WritePropertyName("cx");
        writer.WriteValue(Cx);

        writer.WritePropertyName("cy");
        writer.WriteValue(Cy);

        writer.WritePropertyName("rx");
        writer.WriteValue(Rx);

        writer.WritePropertyName("ry");
        writer.WriteValue(Ry);
      } else {
        Ellipse e = (Ellipse)compare;

        if (e.Cx != Cx) {
          writer.WritePropertyName("cx");
          writer.WriteValue(Cx);
        }

        if (e.Cy != Cy) {
          writer.WritePropertyName("cy");
          writer.WriteValue(Cy);
        }

        if (e.Rx != Rx) {
          writer.WritePropertyName("rx");
          writer.WriteValue(Rx);
        }

        if (e.Ry != Ry) {
          writer.WritePropertyName("ry");
          writer.WriteValue(Ry);
        }
      }
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

    public override void MoveUp(int space) {
      Cy -= space;
    }

    public override void MoveRight(int space) {
      Cx += space;
    }

    public override void MoveDown(int space) {
      Cy += space;
    }

    public override void MoveLeft(int space) {
      Cx -= space;
    }

    public override Shape Copy() {
      return new Ellipse(Cx, Cy, Rx, Ry);
    }

    public override Shape CopyAndSet(Dictionary<string, int[]> attributes) {
      attributes.TryGetValue("cx", out int[] cx);
      attributes.TryGetValue("cy", out int[] cy);
      attributes.TryGetValue("rx", out int[] rx);
      attributes.TryGetValue("ry", out int[] ry);
      return new Ellipse(cx[0], cy[0], rx[0], ry[0]);
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
