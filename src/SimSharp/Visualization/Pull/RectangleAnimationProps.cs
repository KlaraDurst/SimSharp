using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull {
  class RectangleAnimationProps {
    public AnimationAttribute<int> X { get; set; }
    public AnimationAttribute<int> Y { get; set; }
    public AnimationAttribute<int> Width { get; set; }
    public AnimationAttribute<int> Height { get; set; }
    public AnimationAttribute<string> FillColor { get; set; }
    public AnimationAttribute<string> LineColor { get; set; }
    public AnimationAttribute<int> LineWidth { get; set; }
    public AnimationAttribute<bool> Visible { get; set; }

    public RectangleAnimationProps(RectangleAnimationProps other) 
      : this(other.X, other.Y, other.Height, other.Width, other.FillColor, other.LineColor, other.LineWidth, other.Visible) { }

    public RectangleAnimationProps(AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fillColor, AnimationAttribute<string> lineColor, AnimationAttribute<int> lineWidth, AnimationAttribute<bool> visible) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
      Visible = visible;
    }

    public bool AllValues() {
      if (X.Function != null)
        return false;
      if (Y.Function != null)
        return false;
      if (Width.Function != null)
        return false;
      if (Height.Function != null)
        return false;
      if (FillColor.Function != null)
        return false;
      if (LineColor.Function != null)
        return false;
      if (LineWidth.Function != null)
        return false;
      if (Visible.Function != null)
        return false;
      return true;
    }
  }
}
