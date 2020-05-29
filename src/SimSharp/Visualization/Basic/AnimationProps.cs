using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Basic.Shapes;

namespace SimSharp.Visualization.Basic {
  class AnimationProps {
    public Shape Shape0 { get; }
    public Shape Shape1 { get; }
    public DateTime Time0 { get; }
    public DateTime Time1 { get; }
    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }
    public bool Keep { get; }

    public int Start { get; }
    public int Stop { get; }

    public bool Written { get; set; }

    public AnimationProps(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep, int start, int stop) { 
      Shape0 = shape0;
      Shape1 = shape1;
      Time0 = time0;
      Time1 = time1;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Keep = keep;
      Start = start;
      Stop = stop;
      Written = false;
    }
  }
}
