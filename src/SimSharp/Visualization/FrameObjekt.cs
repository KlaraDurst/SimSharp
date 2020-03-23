using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class FrameObjekt {
    public enum Shape { Rectangle, Ellipse, Polygon }

    public string Name { get; }
    public Shape Type { get; }
    public string FillColor { get; }
    public string LineColor { get; }
    public int LineWidth { get; }
    public bool Visible { get; }
    public List<int> Props { get; }

    public FrameObjekt(string name) {
      Name = name;
    }

    public FrameObjekt(string name, Shape type, string fillColor, string lineColor, int lineWidth, bool visible, List<int> props) {
      Name = name;
      Type = type;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
      Visible = visible;
      Props = props;
    }
  }
}