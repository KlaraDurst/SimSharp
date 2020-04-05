using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationUnit {
    public DateTime Time0 { get; set;  }
    public DateTime Time1 { get; set;  }
    public List<string> Frames { get; set; }

    public AnimationUnit(DateTime time0, DateTime time1, int frameNumber) {
      Time0 = time0;
      Time1 = time1;
      Frames = new List<string>(frameNumber);
    }

    public void AddFrame(string frame) {
      Frames.Add(frame);
    }
  }
}