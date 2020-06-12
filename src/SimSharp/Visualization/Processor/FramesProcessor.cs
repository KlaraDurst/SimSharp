using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Processor{
  public interface FramesProcessor {
    void SendStart(AnimationConfig config);
    void SendFrame(string frame);
    void SendStop();
  }
}
