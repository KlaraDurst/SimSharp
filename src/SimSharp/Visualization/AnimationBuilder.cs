using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Pull;
using SimSharp.Visualization.Pull.AdvancedShapes;
using SimSharp.Visualization.Push;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    public string Name { get; }
    public int FPS { get; }
    public string Target { get; }

    public int Width { get; }
    public int Height { get; }

    public int StartX { get; }
    public int StartY { get; }

    public IPlayer Player { get; set; }

    public Simulation Env { 
      get { return env; } 
      set {
        env = value;
        if (prior.Equals(DateTime.MinValue))
          prior = env.StartDate;
      }
    }

    public bool DebugAnimation { get; set; }
    public bool EnableAnimation { get; set; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private bool setCanvas;
    private List<FramesProvider> providers;
    private DateTime prior;
    private int frameCount;

    #region Constructors
    public AnimationBuilder() : this(0, 0, 0, 0, "Visualization", 1, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName, false) { }
    public AnimationBuilder(int width, int height) : this(width, height, "Visualization") { }
    public AnimationBuilder(int width, int height, string name) : this(width, height, name, 1) { }
    public AnimationBuilder(int width, int height, int fps) : this(width, height, "Visualization", fps) { }
    public AnimationBuilder(int width, int height, string name, string target) : this(width, height, 0, 0, name, target) { }
    public AnimationBuilder(int width, int height, string name, int fps) : this(width, height, 0, 0, name, fps) { }
    public AnimationBuilder(int width, int height, int startX, int startY) : this(width, height, startX, startY, "Visualization") { }
    public AnimationBuilder(int width, int height, int startX, int startY, int fps) : this(width, height, startX, startY, "Visualization", fps) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name) : this(width, height, startX, startY, name, 1) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, string target) : this(width, height, startX, startY, name, 1, target, true) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, int fps) : this(width, height, startX, startY, name, fps, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName, true) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, int fps, string target) : this(width, height, startX, startY, name, fps, target, true) { }

    private AnimationBuilder(int width, int height, int startX, int startY, string name, int fps, string target, bool setCanvas) {
      if (fps > 60)
        throw new ArgumentException("fps can not be higher than 60.");

      Name = name;
      FPS = fps;
      Target = target;
      Width = width;
      Height = height;
      StartX = startX;
      StartY = startY;

      this.setCanvas = setCanvas;
      this.providers = new List<FramesProvider>();
      this.prior = DateTime.MinValue;
      this.frameCount = 1;
    }
    #endregion

    #region CreateAnimation
    public Animation Animate(string name, Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      CheckType(shape0, shape1);
      Animation animation = new Animation(name, shape0, shape1, time0, time1, fill, stroke, strokeWidth, keep, this);
      AddProvider(animation);
      return animation;
    }

    public AdvancedAnimation Animate(string name, AdvancedShape shape, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visible) {
      AdvancedAnimation rectAnimation = new AdvancedAnimation(name, shape, fill, stroke, strokeWidth, visible);
      AddProvider(rectAnimation);
      return rectAnimation;
    }

    public void Remove(FramesProvider animation) {
      providers.Remove(animation);
    }
    #endregion

    private void CheckType(Shape shape0, Shape shape1) {
      if (shape0.GetType() != shape1.GetType())
        throw new ArgumentException("Both shapes need to have the same type.");
    }

    public void StartBuilding() {
      if (EnableAnimation) {
        stringWriter = new StringWriter();
        writer = new JsonTextWriter(stringWriter);

        writer.WriteStartObject();

        writer.WritePropertyName("name");
        writer.WriteValue(Name);

        writer.WritePropertyName("fps");
        writer.WriteValue(FPS);

        if (setCanvas) {
          writer.WritePropertyName("width");
          writer.WriteValue(Width);

          writer.WritePropertyName("height");
          writer.WriteValue(Height);

          writer.WritePropertyName("startX");
          writer.WriteValue(StartX);

          writer.WritePropertyName("startY");
          writer.WriteValue(StartY);
        }

        writer.WritePropertyName("frames");
        writer.WriteStartArray();
      }
    }

    public void AddProvider(FramesProvider provider) {
      providers.Add(provider);
    }

    public void Step(DateTime now) {
      if (EnableAnimation) {
        int startFrameNumber = Convert.ToInt32((prior - env.StartDate).TotalSeconds * FPS) + 1;
        int stopFrameNumber = Convert.ToInt32((now - env.StartDate).TotalSeconds * FPS);
        int totalFrameNumber = stopFrameNumber - startFrameNumber + 1;

        // Console.WriteLine(prior + " - " + now);
        // Console.WriteLine(startFrameNumber + " - " + stopFrameNumber + ": " + totalFrameNumber);

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

                    if (DebugAnimation || !first)
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
          prior = now;
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
      if (EnableAnimation) {
        writer.WriteEndArray();
        writer.WriteEndObject();
      }
    }

    public void Play() {
      if (Player != null) {
        Player.Play(Target, stringWriter.ToString());
      } else {
        WriteJson();
      }
    }

    private void WriteJson() {
      File.WriteAllText(Path.Combine(Target, Regex.Replace(Name, @"\s+", "") + ".json"), stringWriter.ToString());
    }

    private void WriteFrameNumber() {
      if (DebugAnimation) {
        writer.WritePropertyName("frame");
        writer.WriteValue(frameCount);
        frameCount++;
      }
    }
  }
}