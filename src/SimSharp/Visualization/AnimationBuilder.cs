using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    public AnimationBuilderProps Props { get; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<FramesProvider> providers;
    private int frameCount;

    public AnimationBuilder(AnimationBuilderProps props, Simulation env) {
      Props = props;
      this.env = env;
      this.providers = new List<FramesProvider>();
      this.frameCount = 1;
    }

    public void StartBuilding() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);

      writer.WriteStartObject();

      writer.WritePropertyName("name");
      writer.WriteValue(Props.Name);

      writer.WritePropertyName("timeStep");
      writer.WriteValue(Props.TimeStep);

      if (Props.SetCanvas) {
        writer.WritePropertyName("width");
        writer.WriteValue(Props.Width);

        writer.WritePropertyName("height");
        writer.WriteValue(Props.Height);

        writer.WritePropertyName("startX");
        writer.WriteValue(Props.StartX);

        writer.WritePropertyName("startY");
        writer.WriteValue(Props.StartY);
      }

      writer.WritePropertyName("frames");
      writer.WriteStartArray();
    }

    public void AddProvider(FramesProvider provider) {
      providers.Add(provider);
    }

    public void Step(DateTime prior, DateTime now) {
      int startFrameNumber = Convert.ToInt32((prior - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep) + 1;
      int stopFrameNumber = Convert.ToInt32((now - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
      int totalFrameNumber = Convert.ToInt32((now - prior).TotalSeconds / Props.TimeStep);

      if (totalFrameNumber > 0) {
        if (providers.Count > 0) {
          SortedList<int, List<IEnumerator<string>>> frames = new SortedList<int, List<IEnumerator<string>>>();

          foreach (FramesProvider provider in providers) {
            List<AnimationUnit> units = provider.FramesFromTo(startFrameNumber, stopFrameNumber);

            if (units.Count > 0) {
              foreach (AnimationUnit unit in units) {
                if (frames.ContainsKey(unit.Start)) {
                  frames.TryGetValue(unit.Start, out List<IEnumerator<string>> l);
                  l.Add(unit.Frames.GetEnumerator());
                } else {
                  frames.Add(unit.Start, new List<IEnumerator<string>>() { unit.Frames.GetEnumerator() });
                }
              }
            }
          }

          if (frames.Count > 0) {
            List<IEnumerator<string>> framesEnums = new List<IEnumerator<string>>(frames.Count);
            int start = frames.Keys[0];
            int stop = frames.Count > 1 ? frames.Keys[1] : stopFrameNumber + 1;

            int precedingFramesNumber = start - startFrameNumber;
            WriteEmptyObjects(precedingFramesNumber);

            while (start <= stopFrameNumber) {
              framesEnums.AddRange(frames.Values[0]);
              frames.RemoveAt(0);

              int frameNumber = stop - start;
              for (int i = 0; i < frameNumber; i++) {
                writer.WriteStartObject();
                WriteFrameNumber();
                bool first = true;

                for (int j = framesEnums.Count - 1; j >= 0; j--) {
                  if (!framesEnums[j].MoveNext()) {
                    framesEnums.RemoveAt(j);
                    continue;
                  }
                  string frame = framesEnums[j].Current;

                  if (env.AddFrameNumbers || !first)
                    writer.WriteRaw(",");
                  writer.WriteRaw(frame);

                  first = false;
                }
                writer.WriteEndObject();
              }
              start = stop;
              stop = frames.Count > 1 ? frames.Keys[1] : stopFrameNumber + 1;
            }
          } else {
            WriteEmptyObjects(totalFrameNumber);
          }
        } else {
          WriteEmptyObjects(totalFrameNumber);
        }
      }
    }

    private void WriteEmptyObjects(int frameNumber) {
      for (int i = 0; i < frameNumber; i++) {
        writer.WriteStartObject();
        WriteFrameNumber();
        writer.WriteEndObject();
      }
    }

    public void StopBuilding() {
      writer.WriteEndArray();
      writer.WriteEndObject();

      WriteJson();
    }

    private void WriteJson() {
      File.WriteAllText(Props.Target, stringWriter.ToString());
    }

    private void WriteFrameNumber() {
      if (env.AddFrameNumbers) {
        writer.WritePropertyName("frame");
        writer.WriteValue(frameCount);
        frameCount++;
      }
    }
  }
}