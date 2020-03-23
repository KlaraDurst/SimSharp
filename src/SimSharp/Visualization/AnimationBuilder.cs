using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    public string Name { get; }
    public double TimeStep { get; }

    private string target;

    private StringWriter stringWriter;
    private JsonTextWriter writer;

    private List<Animation> animations;

    public AnimationBuilder() : this("Visualization") { }
    public AnimationBuilder(string name) : this(name, 0.25) { }
    public AnimationBuilder(string name, string target) : this(name, 0.25, target) { }
    public AnimationBuilder(string name, double timeStep) : this(name, timeStep, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName) { }
    public AnimationBuilder(string name, double timeStep, string target) {
      Name = name;
      TimeStep = timeStep;
      this.target = target;
    }

    public void OpenJson() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);

      writer.WriteStartObject();

      writer.WritePropertyName("name");
      writer.WriteValue(Name);

      writer.WritePropertyName("timeStep");
      writer.WriteValue(TimeStep);

      writer.WritePropertyName("frames");
      writer.WriteStartArray();
    }

    public void AddAnimation(Animation animation) {
      animations.Add(animation);
    }

    public void Step(DateTime now) {

    }

    private void CloseJson() {
      writer.WriteEndArray();
      writer.WriteEndObject();
    }

    public void WriteJson() {
      throw new NotImplementedException();
    }
  }
}