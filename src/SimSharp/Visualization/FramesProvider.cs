using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public interface FramesProvider {

    // incl. start, excl. stop
    List<AnimationUnit> FramesFromTo(DateTime start, DateTime stop);
  }
}
