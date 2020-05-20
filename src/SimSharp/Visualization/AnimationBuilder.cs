using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Pull;
using SimSharp.Visualization.Push;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    public string Name { get; }
    public double TimeStep { get; }
    public string Target { get; }

    public int Width { get; }
    public int Height { get; }

    public int StartX { get; }
    public int StartY { get; }

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
    public AnimationBuilder() : this(0, 0, 0, 0, "Visualization", 0.25, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName + @"\" + Regex.Replace("Visualization", @"\s+", "") + ".json", false) { }
    public AnimationBuilder(int width, int height) : this(width, height, "Visualization") { }
    public AnimationBuilder(int width, int height, string name) : this(width, height, name, 0.25) { }
    public AnimationBuilder(int width, int height, double timeStep) : this(width, height, "Visualization", timeStep) { }
    public AnimationBuilder(int width, int height, string name, string target) : this(width, height, 0, 0, name, target) { }
    public AnimationBuilder(int width, int height, string name, double timeStep) : this(width, height, 0, 0, name, timeStep) { }
    public AnimationBuilder(int width, int height, int startX, int startY) : this(width, height, startX, startY, "Visualization") { }
    public AnimationBuilder(int width, int height, int startX, int startY, double timeStep) : this(width, height, startX, startY, "Visualization", timeStep) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name) : this(width, height, startX, startY, name, 0.25) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, string target) : this(width, height, startX, startY, name, 0.25, target, true) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, double timeStep) : this(width, height, startX, startY, name, timeStep, Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName + @"\" + Regex.Replace(name, @"\s+", "") + ".json", true) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, double timeStep, string target) : this(width, height, startX, startY, name, timeStep, target, true) { }

    private AnimationBuilder(int width, int height, int startX, int startY, string name, double timeStep, string target, bool setCanvas) {
      if (timeStep > 1)
        throw new ArgumentException("The time lapsed between each pair of consecutive frames can not be more than 1 second.");
      if (Math.Floor(1 / timeStep) != (1 / timeStep))
        throw new ArgumentException("The time lapsed between each pair of consecutive frames has to devide 1 second without rest.");

      Name = name;
      TimeStep = timeStep;
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
    public Animation Animate(string name, Rect rect0, Rect rect1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      Animation animation = new Animation(name, rect0, rect1, time0, time1, fill, stroke, strokeWidth, keep, this);
      AddProvider(animation);
      return animation;
    }

    public Animation Animate(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      Animation animation = new Animation(name, ellipse0, ellipse1, time0, time1, fill, stroke, strokeWidth, keep, this);
      AddProvider(animation);
      return animation;
    }

    public Animation Animate(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      Animation animation = new Animation(name, polygon0, polygon1, time0, time1, fill, stroke, strokeWidth, keep, this);
      AddProvider(animation);
      return animation;
    }

    public RectAnimation AnimateRect(string name, AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visible) {
      RectAnimation rectAnimation = new RectAnimation(name, x, y, width, height, fill, stroke, strokeWidth, visible);
      AddProvider(rectAnimation);
      return rectAnimation;
    }
    #endregion

    public void StartBuilding() {
      if (EnableAnimation) {
        stringWriter = new StringWriter();
        writer = new JsonTextWriter(stringWriter);

        writer.WriteStartObject();

        writer.WritePropertyName("name");
        writer.WriteValue(Name);

        writer.WritePropertyName("timeStep");
        writer.WriteValue(TimeStep);

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
        int startFrameNumber = Convert.ToInt32((prior - env.StartDate).TotalSeconds / TimeStep) + 1;
        int stopFrameNumber = Convert.ToInt32((now - env.StartDate).TotalSeconds / TimeStep);
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

        WriteJson();
      }
    }

    private void WriteJson() {
      File.WriteAllText(Target, stringWriter.ToString());
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