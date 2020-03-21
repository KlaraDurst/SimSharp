using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  public abstract class Animation {
    public Shape Shape0 { get; }
    public Shape Shape1 { get; }

    public DateTime Time0 { get; }
    public DateTime Time1 { get; }

    public string FillColor { get; }
    public string LineColor { get; }
    public int LineWidth { get; }

    public bool Keep { get; }

    protected Animation(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      Shape0 = shape0;
      Shape1 = shape1;
      Time0 = time0;
      Time1 = time1;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
      Keep = keep;

      Animate();
    }

    protected abstract void Animate();
    public abstract void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep);
  }
}