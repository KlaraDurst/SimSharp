using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Basic.Shapes;
using SimSharp.Visualization.Basic.Styles;

namespace SimSharp.Visualization.Basic.Resources {
  public class LevelAnimation {
    public string Name { get; }
    public Rect Rect { get; }
    public Style Style { get; set; }

    private AnimationBuilder animationBuilder;
    private Animation animation;

    public LevelAnimation(string name, Rect rect, Style style, AnimationBuilder animationBuilder) {
      Name = name;
      Rect = rect;
      Style = style;

      this.animationBuilder = animationBuilder;
      animation = this.animationBuilder.Animate(Name, Rect, style, true);
    }

    public void Update(double level) {
      Rect newRect = new Rect(Rect.X, Convert.ToInt32(Rect.Y + Rect.Height - level), Rect.Width, Convert.ToInt32(level));

      animation.Update(newRect, Style, true);
    }
  }
}
