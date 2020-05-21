using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push {
  class AnimationProps {
    public Shape Shape0 { get; }
    public Shape Shape1 { get; }

    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }
    public DateTime Time0 { get; }
    public DateTime Time1 { get; }
    public bool Keep { get; }

    public bool Written { get; set; }

    public AnimationProps(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep)
      : this(fill, stroke, strokeWidth, time0, time1, keep) {
      Shape0 = shape0;
      Shape1 = shape1;
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
