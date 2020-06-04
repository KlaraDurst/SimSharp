using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationUtil {
    public AnimationBuilder AnimationBuilder { get; }

    public AnimationUtil(AnimationBuilder animationBuilder) {
      AnimationBuilder = animationBuilder;
    }

    public Func<int, int> GetIntValueAt(DateTime start, DateTime stop, int startValue, int endValue) {
      int startFrame = Convert.ToInt32((start - AnimationBuilder.Env.StartDate).TotalSeconds * AnimationBuilder.FPS) + 1;
      int stopFrame = Convert.ToInt32((stop - AnimationBuilder.Env.StartDate).TotalSeconds * AnimationBuilder.FPS);
      int totalFrames = stopFrame - startFrame + 1;

      return t => {
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
      };
    }

    public Func<int, T> GetIfTimeBetween<T>(DateTime from, DateTime to, T val, T elseVal) {
      int startFrame = Convert.ToInt32((from - AnimationBuilder.Env.StartDate).TotalSeconds * AnimationBuilder.FPS) + 1;
      int stopFrame = Convert.ToInt32((to - AnimationBuilder.Env.StartDate).TotalSeconds * AnimationBuilder.FPS);

      return t => {
        if (t >= startFrame && t <= stopFrame)
          return val;
        else
          return elseVal;
      };
    }
  }
}
