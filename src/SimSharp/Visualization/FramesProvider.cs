using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public interface FramesProvider {

    // incl. start and stop
    List<AnimationUnit> FramesFromTo(int start, int stop);
  }
}
