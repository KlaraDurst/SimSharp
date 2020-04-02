using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Polygon {
    public List<int> XList { get; }
    public List<int> YList { get; }
    public int[] XYList { get; }

    public Polygon(params int[] xy) {
      if (xy.Length % 2 != 0) {
        throw new ArgumentException("A polygon needs the same number of y and x coordinates");
      }
      else {
        for (int i = 0; i < xy.Length; i+=2) {
          XList.Add(xy[i]);
          YList.Add(xy[i + 1]);
        }

        XYList = xy;
      }
    }

    public int[] GetTransformation() {
      return XYList;
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Polygon p = (Polygon)obj;
        return XList.SequenceEqual(p.XList) && YList.SequenceEqual(p.YList);
      }
    }

    public override int GetHashCode() {
      return (XList, YList).GetHashCode();
    }
  }
}
