using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationUnit {
    public DateTime Start { get; }
    public DateTime Stop { get; }
    public List<string> Frames { get; }

    public AnimationUnit(DateTime start, DateTime stop, int frameNumber) {
      Start = start;
      Stop = stop;
      Frames = new List<string>(frameNumber);
    }

    public void AddFrame(string frame) {
      Frames.Add(frame);
    }

    public void AddFrames(IEnumerable<string> frames) {
      Frames.AddRange(frames);
    }
  }
}