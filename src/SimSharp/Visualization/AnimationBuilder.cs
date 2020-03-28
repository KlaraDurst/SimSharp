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
      if (animations.Count > 0) {
        SortedList<DateTime, IEnumerator<string>> frames = new SortedList<DateTime, IEnumerator<string>>();

        foreach (Animation animation in animations) {
          List<AnimationUnit> units = animation.FramesFromTo(prior, now);

          if (units != null) {
            foreach(AnimationUnit unit in units) {
              frames.Add(unit.Start, unit.Frames.GetEnumerator());
            }
          }
        }

        List<IEnumerator<string>> framesEnums = new List<IEnumerator<string>>(frames.Count);
        DateTime until = frames.Keys[0];

        while (until <= now) {
          while (frames.Keys[0] == until) {
            framesEnums.Add(frames.Values[0]);
            frames.RemoveAt(0);
          }

          int frameNumber = Convert.ToInt32((now - until).TotalSeconds / Props.TimeStep);
          for (int i = 0; i <= frameNumber; i++) {
            writer.WriteStartObject();

            for (int j = framesEnums.Count - 1; j >= 0; j--) {
              bool first = true;
              string frame = framesEnums[j].Current;

              if (!first)
                writer.WriteRaw(",");
              writer.WriteRaw(frame);

              if (!framesEnums[j].MoveNext())
                framesEnums.RemoveAt(j);
            }
            writer.WriteEndObject();
          }
        }
      } 
      else {
        int frameNumber = Convert.ToInt32((now - prior).TotalSeconds / Props.TimeStep);
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
      // TODO
    }
  }
}