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
    public Style Style { get; }
    public bool Keep { get; }

    public int Start { get; }
    public int Stop { get; }

    public bool Written { get; set; }

    public AnimationProps(Shape shape0, Shape shape1, DateTime time0, DateTime time1, Style style, bool keep, int start, int stop) { 
      Shape0 = shape0;
      Shape1 = shape1;
      Time0 = time0;
      Time1 = time1;
      Style = style;
      Keep = keep;
      Start = start;
      Stop = stop;
      Written = false;
    }
  }
}
