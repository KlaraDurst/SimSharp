using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Advanced.AdvancedShapes;

namespace SimSharp.Visualization.Advanced {
  class AdvancedAnimationProps {
    public AdvancedShape Shape { get; set; }
    public AnimationAttribute<string> Fill { get; set; }
    public AnimationAttribute<string> Stroke { get; set; }
    public AnimationAttribute<int> StrokeWidth { get; set; }
    public AnimationAttribute<bool> Visibility { get; set; }

    public bool Written { get; set; }

    public AdvancedAnimationProps(AdvancedAnimationProps other) 
      : this(other.Shape, other.Fill, other.Stroke, other.StrokeWidth, other.Visibility) { }

    public AdvancedAnimationProps(AdvancedShape shape, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visibility) {
      Shape = shape;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Visibility = visibility;
      Written = false;
    }

    public bool AllValues() {
      if (!Shape.AllValues())
        return false;
      if (Fill.Function != null)
        return false;
      if (Stroke.Function != null)
        return false;
      if (StrokeWidth.Function != null)
        return false;
      if (Visibility.Function != null)
        return false;
      return true;
    }
  }
}
