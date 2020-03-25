using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Frames {
  public class FrameProperties {
    public enum Shape { Rectangle, Ellipse, Polygon }

    public Shape Type { get; }
    public string FillColor { get; }
    public string LineColor { get; }
    public int LineWidth { get; }

    public FrameProperties(Shape type, string fillColor, string lineColor, int lineWidth) {
      Type = type;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
    }
  }
}
