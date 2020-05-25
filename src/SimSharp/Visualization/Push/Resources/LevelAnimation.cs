﻿using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push.Resources {
  public class LevelAnimation {
    public string Name { get; }
    public Rect Rect { get; }
    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }

    private AnimationBuilder animationBuilder;
    private Animation animation;

    public LevelAnimation(string name, Rect rect, string fill, string stroke, int strokeWidth, AnimationBuilder animationBuilder) {
      Name = name;
      Rect = rect;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;

      this.animationBuilder = animationBuilder;
      animation = this.animationBuilder.Animate(Name, Rect, Rect, animationBuilder.Env.Now, animationBuilder.Env.Now, Fill, Stroke, StrokeWidth, true);
    }

    public void Update(double level) {
      Rect newRect = new Rect(Rect.X, Convert.ToInt32(Rect.Y + Rect.Height - level), Rect.Width, Convert.ToInt32(level));

      animation.Update(newRect, newRect, animationBuilder.Env.Now, animationBuilder.Env.Now, Fill, Stroke, StrokeWidth, true);
    }
  }
}
