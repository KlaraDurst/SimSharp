using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SimSharp.Visualization {
  public class AnimationBuilderProps {
    public string Name { get; }
    public double TimeStep { get; }
    public string Target { get; }

    public int Width { get; }
    public int Height { get; }

    public int StartX { get; }
    public int StartY { get; }

    public bool SetCanvas { get; }

    public AnimationBuilderProps() : this(0,0,0,0,"Visualization", 0.25, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName + @"\" + Regex.Replace("Visualization", @"\s+", "") + ".json", false) { }
    public AnimationBuilderProps(int width, int height) : this(width, height, "Visualization") { }
    public AnimationBuilderProps(int width, int height, string name) : this(width, height, name, 0.25) { }
    public AnimationBuilderProps(int width, int height, double timeStep) : this(width, height, "Visualization", timeStep) { }
    public AnimationBuilderProps(int width, int height, string name, string target) : this(width, height, 0, 0, name, target) { }
    public AnimationBuilderProps(int width, int height, string name, double timeStep) : this(width, height, 0, 0, name, timeStep) { }
    public AnimationBuilderProps(int width, int height, int startX, int startY) : this(width, height, startX, startY, "Visualization") { }
    public AnimationBuilderProps(int width, int height, int startX, int startY, double timeStep) : this(width, height, startX, startY, "Visualization", timeStep) { }
    public AnimationBuilderProps(int width, int height, int startX, int startY, string name) : this(width, height, startX, startY, name, 0.25) { }
    public AnimationBuilderProps(int width, int height, int startX, int startY, string name, string target) : this(width, height, startX, startY, name, 0.25, target, true) { }
    public AnimationBuilderProps(int width, int height, int startX, int startY, string name, double timeStep) : this(width, height, startX, startY, name, timeStep, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName + @"\" + Regex.Replace(name, @"\s+", "") + ".json", true) { }
    public AnimationBuilderProps(int width, int height, int startX, int startY, string name, double timeStep, string target) : this(width, height, startX, startY, name, timeStep, target, true) { }

    private AnimationBuilderProps(int width, int height, int startX, int startY, string name, double timeStep, string target, bool setCanvas) {
      if (timeStep > 1)
        throw new ArgumentException("The time lapsed between each pair of consecutive frames can not be more than 1 second.");
      if (Math.Floor(1 / timeStep) != (1 / timeStep))
        throw new ArgumentException("The time lapsed between each pair of consecutive frames has to devide 1 second without rest.");

      Name = name;
      TimeStep = timeStep;
      Target = target;
      Width = width;
      Height = height;
      StartX = startX;
      StartY = startY;
      SetCanvas = setCanvas;
    }
  }
}
