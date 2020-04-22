using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Pull.Attributes;

namespace SimSharp.Visualization.Pull {
  class RectangleAnimationProps {
    public IntAttribute X { get; set; }
    public IntAttribute Y { get; set; }
    public IntAttribute Width { get; set; }
    public IntAttribute Height { get; set; }
    public StringAttribute FillColor { get; set; }
    public StringAttribute LineColor { get; set; }
    public IntAttribute LineWidth { get; set; }
    public BoolAttribute Visible { get; set; }

    public RectangleAnimationProps(RectangleAnimationProps other) 
      : this(other.X, other.Y, other.Height, other.Width, other.FillColor, other.LineColor, other.LineWidth, other.Visible) { }

    public RectangleAnimationProps(IntAttribute x, IntAttribute y, IntAttribute width, IntAttribute height, StringAttribute fillColor, StringAttribute lineColor, IntAttribute lineWidth, BoolAttribute visible) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
      Visible = visible;
    }
  }
}
