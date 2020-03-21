using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  class RectangleAnimation : Animation {

    public RectangleAnimation(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep)
      : base(rectangle0, rectangle1, time0, time1, fillColor, lineColor, lineWidth, keep) {}

    public override void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      throw new NotImplementedException();
    }

    protected override void Animate() {
      throw new NotImplementedException();
    }
  }
}
