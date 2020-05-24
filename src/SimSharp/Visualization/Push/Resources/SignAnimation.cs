using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push.Resources {
  public class SignAnimation {
    public string Name { get; }
    public Shape Shape { get; private set; }
    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }

    private AnimationBuilder animationBuilder;
    private Animation level;

    public SignAnimation(string name, Shape shape, string fill, string stroke, int strokeWidth, AnimationBuilder animationBuilder) {
      Name = name;
      Shape = shape;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;

      this.animationBuilder = animationBuilder;
      level = this.animationBuilder.Animate(Name, Shape, Shape, animationBuilder.Env.Now, animationBuilder.Env.Now, Fill, Stroke, StrokeWidth, true);
    }

    public void Update(string fill) {
      level.Update(Shape, Shape, animationBuilder.Env.Now, animationBuilder.Env.Now, fill, Stroke, StrokeWidth, true);
    }
  }
}