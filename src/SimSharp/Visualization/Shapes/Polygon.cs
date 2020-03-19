using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Shapes {
  public class Polygon : Shape {
    public List<int> XList { get; set; }
    public List<int> YList { get; set; }

    public Polygon(string name, List<int> xList, List<int> yList) : base(name) {
      XList = xList;
      YList = yList;
    }
  }
}
