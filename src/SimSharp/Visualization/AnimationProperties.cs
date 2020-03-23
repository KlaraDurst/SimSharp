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
      Name = name;
      TimeStep = timeStep;
      Target = target;
    }
  }
}
