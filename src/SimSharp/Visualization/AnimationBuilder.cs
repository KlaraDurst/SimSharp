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
      int totalFrameNumber = Convert.ToInt32((now - prior).TotalSeconds / Props.TimeStep);

      if (animations.Count > 0) {
        SortedList<DateTime, List<IEnumerator<string>>> frames = new SortedList<DateTime, List<IEnumerator<string>>>();

        foreach (Animation animation in animations) {
          List<AnimationUnit> units = animation.FramesFromTo(prior, now);

          if (units != null) {
            foreach(AnimationUnit unit in units) {
              if (frames.ContainsKey(unit.Start)) {
                List<IEnumerator<string>> l;
                frames.TryGetValue(unit.Start, out l);
                l.Add(unit.Frames.GetEnumerator());
              }
              else {
                frames.Add(unit.Start, new List<IEnumerator<string>>() { unit.Frames.GetEnumerator() });
              }
            }
          }
        }

        if (frames.Count > 0) {
          List<IEnumerator<string>> framesEnums = new List<IEnumerator<string>>(frames.Count);
          DateTime start = frames.Keys[0];
          DateTime stop = frames.Count > 1 ? frames.Keys[1] : now;

          int precedingFramesNumber = Convert.ToInt32((start - prior).TotalSeconds / Props.TimeStep);
          writeEmptyObjects(precedingFramesNumber);

          while (start != now) {
            framesEnums.AddRange(frames.Values[0]);
            frames.RemoveAt(0);

            int frameNumber = Convert.ToInt32((stop - start).TotalSeconds / Props.TimeStep);
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
            start = stop;
            stop = frames.Count > 1 ? frames.Keys[1] : now;
          }
        }
        else {
          writeEmptyObjects(totalFrameNumber);
        }
      } 
      else {
        writeEmptyObjects(totalFrameNumber);
      }
    }

    private void writeEmptyObjects(int frameNumber) {
      for (int i = 0; i <= frameNumber; i++) {
        writer.WriteStartObject();
        writer.WriteEndObject();
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