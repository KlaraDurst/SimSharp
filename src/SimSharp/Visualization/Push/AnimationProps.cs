using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push {
  class AnimationProps {
    public Rect Rect0 { get; }
    public Rect Rect1 { get; }

    public Ellipse Ellipse0 { get; }
    public Ellipse Ellipse1 { get; }

    public Polygon Polygon0 { get; }
    public Polygon Polygon1 { get; }

    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }
    public DateTime Time0 { get; }
    public DateTime Time1 { get; }
    public bool Keep { get; }

    public bool Written { get; set; }

    public AnimationProps(Rect rect0, Rect rect1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep)
      : this(fill, stroke, strokeWidth, time0, time1, keep) {
      Rect0 = rect0;
      Rect1 = rect1;
    }

    public AnimationProps(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep)
      : this(fill, stroke, strokeWidth, time0, time1, keep) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;
    }

    public AnimationProps(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep)
      : this(fill, stroke, strokeWidth, time0, time1, keep) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;
    }

    private AnimationProps(string fill, string stroke, int strokeWidth, DateTime time0, DateTime time1, bool keep) {
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Time0 = time0;
      Time1 = time1;
      Keep = keep;
      Written = false;
    }
  }
}
