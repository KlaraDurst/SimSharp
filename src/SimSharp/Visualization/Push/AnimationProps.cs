using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push {
  class AnimationProps {
    public Rectangle Rectangle0 { get; }
    public Rectangle Rectangle1 { get; }

    public Ellipse Ellipse0 { get; }
    public Ellipse Ellipse1 { get; }

    public Polygon Polygon0 { get; }
    public Polygon Polygon1 { get; }

    public string FillColor { get; }
    public string LineColor { get; }
    public int LineWidth { get; }
    public DateTime Time0 { get; }
    public DateTime Time1 { get; }
    public bool Keep { get; }

    public AnimationProps(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep)
      : this(fillColor, lineColor, lineWidth, time0, time1, keep) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;
    }

    public AnimationProps(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep)
      : this(fillColor, lineColor, lineWidth, time0, time1, keep) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;
    }

    public AnimationProps(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep)
      : this(fillColor, lineColor, lineWidth, time0, time1, keep) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;
    }

    private AnimationProps(string fillColor, string lineColor, int lineWidth, DateTime time0, DateTime time1, bool keep) {
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
      Time0 = time0;
      Time1 = time1;
      Keep = keep;
    }
  }
}
