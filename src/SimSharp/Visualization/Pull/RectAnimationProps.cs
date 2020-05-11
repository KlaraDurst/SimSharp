using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull {
  class RectAnimationProps {
    public AnimationAttribute<int> X { get; set; }
    public AnimationAttribute<int> Y { get; set; }
    public AnimationAttribute<int> Width { get; set; }
    public AnimationAttribute<int> Height { get; set; }
    public AnimationAttribute<string> Fill { get; set; }
    public AnimationAttribute<string> Stroke { get; set; }
    public AnimationAttribute<int> StrokeWidth { get; set; }
    public AnimationAttribute<bool> Visible { get; set; }

    public bool Written { get; set; }

    public RectAnimationProps(RectAnimationProps other) 
      : this(other.X, other.Y, other.Height, other.Width, other.Fill, other.Stroke, other.StrokeWidth, other.Visible) { }

    public RectAnimationProps(AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visible) {
      X = x;
      Y = y;
      Width = width;
      Height = height;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Visible = visible;
      Written = false;
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
      if (Fill.Function != null)
        return false;
      if (Stroke.Function != null)
        return false;
      if (StrokeWidth.Function != null)
        return false;
      if (Visible.Function != null)
        return false;
      return true;
    }
  }
}
