using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    public AnimationProperties Props { get; }

    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<Animation> animations;

    public AnimationBuilder(AnimationProperties props) {
      Props = props;
      this.animations = new List<Animation>();
    }

    public void StartBuilding() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);

      writer.WriteStartObject();

      writer.WritePropertyName("name");
      writer.WriteValue(Props.Name);

      writer.WritePropertyName("timeStep");
      writer.WriteValue(Props.TimeStep);

      writer.WritePropertyName("frames");
      writer.WriteStartArray();
    }

    public void AddAnimation(Animation animation) {
      animations.Add(animation);
    }

    public void Step(DateTime prior, DateTime now) {
      int frameNumber = Convert.ToInt32((now - prior).TotalSeconds / Props.TimeStep);

      if (animations.Count > 0) {
        List<IEnumerator<string>> frames = new List<IEnumerator<string>>(animations.Count);

        foreach (Animation animation in animations) {
          IEnumerator<string> framesEnum = animation.FramesFromTo(prior, now);

          if (framesEnum != null)
            frames.Add(framesEnum);
        }

        for (int i = 0; i <= frameNumber; i++) {
          writer.WriteStartObject();

          foreach (IEnumerator<string> framesEnum in frames) {
            bool first = true;
            string frame = framesEnum.Current;
            framesEnum.MoveNext();

            if (frame != null) {
              if (!first)
                writer.WriteRaw(",");
              writer.WriteRaw(frame);
              first = false;
            }
          }

          writer.WriteEndObject();
        }
      }
      else {
        for (int i = 0; i <= frameNumber; i++) {
          writer.WriteStartObject();
          writer.WriteEndObject();
        }
      }
    }

    private void StopBuilding() {
      writer.WriteEndArray();
      writer.WriteEndObject();
    }

    public void WriteJson() {
      throw new NotImplementedException();
    }
  }
}