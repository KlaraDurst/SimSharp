using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Frames {
  public class Frame {

    public string Name { get; }
    public bool Visible { get; }
    public FrameProperties Props { get; }
    public List<int> Transformation { get; }

    public Frame(string name, bool visible) {
      Name = name;
      Visible = visible;
    }

    public Frame(string name, List<int> transformation) {
      Name = name;
      Transformation = transformation;
    }

    public Frame(string name, bool visible, FrameProperties.Shape type, string fillColor, string lineColor, int lineWidth, List<int> transformation) {
      Name = name;
      Visible = visible;
      Props = new FrameProperties(type, fillColor, lineColor, lineWidth);
      Transformation = transformation;
    }
  }
}