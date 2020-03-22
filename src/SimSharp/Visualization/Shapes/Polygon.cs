using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Polygon {
    public List<int> XList { get; }
    public List<int> YList { get; }
    public List<int> XYList { get; }

    public Polygon(params int[] xy) {
      if (xy.Length % 2 != 0) {
        throw new ArgumentException("A polygon needs the same number of y than x coordinates");
      }
      else {
        for (int i = 0; i < xy.Length; i+=2) {
          XList.Add(xy[i]);
          YList.Add(xy[i + 1]);
        }

        XYList.AddRange(xy);
      }
    } 
  }
}
