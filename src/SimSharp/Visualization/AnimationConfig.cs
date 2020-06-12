using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationConfig {
    public string Name { get; }
    public int FPS { get; }

    public int Width { get; }
    public int Height { get; }

    public int StartX { get; }
    public int StartY { get; }

    public bool SetCanvas { get; }

    public AnimationConfig (string name, int fps, int width, int height, int startX, int startY, bool setCanvas) {
      Name = name;
      FPS = fps;
      Width = width;
      Height = height;
      StartX = startX;
      StartY = startY;
      SetCanvas = setCanvas;
    }
  }
}
