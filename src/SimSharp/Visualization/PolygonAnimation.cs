using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  class PolygonAnimation : Animation {

    public PolygonAnimation(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep)
      : base(polygon0, polygon1, time0, time1, fillColor, lineColor, lineWidth, keep) { }

    public override void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      throw new NotImplementedException();
    }

    protected override void Animate() {
      throw new NotImplementedException();
    }
  }
}
