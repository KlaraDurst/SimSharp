using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Processor {
  public class JsonWriter : FramesProcessor {
    protected string target = Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName;
    protected StringWriter stringWriter;
    protected JsonTextWriter writer;
    protected string name;
    protected bool first = true;

    public JsonWriter() {
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
    }

    public void SendStart(AnimationConfig config) {
      this.name = config.Name;

      writer.WriteStartObject();

      writer.WritePropertyName("name");
      writer.WriteValue(config.Name);

      writer.WritePropertyName("fps");
      writer.WriteValue(config.FPS);

      if (config.SetCanvas) {
        writer.WritePropertyName("width");
        writer.WriteValue(config.Width);

        writer.WritePropertyName("height");
        writer.WriteValue(config.Height);

        writer.WritePropertyName("startX");
        writer.WriteValue(config.StartX);

        writer.WritePropertyName("startY");
        writer.WriteValue(config.StartY);
      }

      writer.WritePropertyName("frames");
      writer.WriteStartArray();
    }

    public void SendFrame(string frame) {
      if (!first)
        writer.WriteRaw(",");

      writer.WriteRaw(frame);
      first = false;
    }

    public virtual void SendStop() {
      writer.WriteEndArray();
      writer.WriteEndObject();

      Write();
    }

    private void Write() {
      File.WriteAllText(Path.Combine(target, Regex.Replace(name, @"\s+", "") + ".json"), stringWriter.ToString());
    }
  }
}
