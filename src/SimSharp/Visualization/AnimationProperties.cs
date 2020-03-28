using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimSharp.Visualization {
  public class AnimationProperties {
    public string Name { get; }
    public double TimeStep { get; }
    public string Target { get; }

    public AnimationProperties() : this("Visualization") { }
    public AnimationProperties(string name) : this(name, 0.25) { }
    public AnimationProperties(string name, string target) : this(name, 0.25, target) { }
    public AnimationProperties(string name, double timeStep) : this(name, timeStep, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName) { }
    public AnimationProperties(string name, double timeStep, string target) {
      if (timeStep > 1)
        throw new ArgumentException("The time lapsed between each pair of consecutive frames can not be more than 1 second.");
      if (Math.Floor(1 / timeStep) != (1 / timeStep))
        throw new ArgumentException("The time lapsed between each pair of consecutive frames has to devide 1 second without rest.");

      Name = name;
      TimeStep = timeStep;
      Target = target;
    }
  }
}
