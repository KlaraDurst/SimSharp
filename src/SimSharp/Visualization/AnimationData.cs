using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization {
  public class AnimationData {
    public string Name { get; }
    public double TimeStep { get; }

    private string target;

    private StringWriter stringWriter;
    private JsonTextWriter writer;

    private List<Animation> animations;

    public AnimationData(string name, double timeStep, string target) {
      Name = name;
      TimeStep = timeStep;
      this.target = target;

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

    private void CloseJson() {
      writer.WriteEndArray();
      writer.WriteEndObject();
    }

    public void WriteJson() {
      throw new NotImplementedException();
    }
  }
}