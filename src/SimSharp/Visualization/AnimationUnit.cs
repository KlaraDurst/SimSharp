using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationUnit {
    public int Start { get; set;  }
    public int Stop { get; set;  }
    public List<string> Frames { get; set; }

    public AnimationUnit(int start, int stop, int frameNumber) {
      Start = start;
      Stop = stop;
      Frames = new List<string>(frameNumber);
    }

    public void AddFrame(string frame) {
      Frames.Add(frame);
    }

    public void AddFrameRange(IEnumerable<string> frames) {
      Frames.AddRange(frames);
    }
  }
}