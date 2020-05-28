using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationUtil {
    public AnimationBuilder AnimationBuilder { get; }

    public AnimationUtil(AnimationBuilder animationBuilder) {
      AnimationBuilder = animationBuilder;
    }

    public int GetIntValueAt(int t, DateTime start, DateTime stop, int startValue, int endValue) {
      int startFrame = Convert.ToInt32((start - AnimationBuilder.Env.StartDate).TotalSeconds * AnimationBuilder.FPS) + 1;
      int pauseFrame = Convert.ToInt32((stop - AnimationBuilder.Env.StartDate).TotalSeconds * AnimationBuilder.FPS);
      int totalFrames = pauseFrame - startFrame + 1;

      int currFrame = t - startFrame;
      if (totalFrames < 2) {
        if (currFrame <= 0)
          return startValue;
        else
          return endValue;
      } else {
        if (currFrame >= 0 && currFrame <= totalFrames) {
          double i = 1 / Convert.ToDouble(totalFrames - 1) * currFrame;
          return Convert.ToInt32((1 - i) * startValue + i * endValue);
        } else if (currFrame < 0)
          return startValue;
        else
          return endValue;
      }
    }
  }
}
