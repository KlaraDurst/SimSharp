using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Advanced;
using SimSharp.Visualization.Advanced.AdvancedShapes;
using SimSharp.Visualization.Basic;
using SimSharp.Visualization.Basic.Resources;
using SimSharp.Visualization.Basic.Shapes;
using SimSharp.Visualization.Player;

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
    private List<string> names; 
    private DateTime prior;
    private int frameCount;
    private int animationCount;
    private int queueCount;
    private int levelCount;

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
      this.names = new List<string>();
      this.prior = DateTime.MinValue;
      this.frameCount = 1;
      this.animationCount = 0;
      this.queueCount = 0;
      this.levelCount = 0;
    }
    #endregion

    #region CreateAnimation
    public Animation Animate(Shape shape, Style style, bool keep = true) {
      return Animate(shape, Env.Now, style, keep);
    }

    public Animation Animate(string name, Shape shape, Style style, bool keep = true) {
      return Animate(name, shape, Env.Now, style, keep);
    }

    public Animation Animate(Shape shape, DateTime time, Style style, bool keep = true) {
      return Animate(shape, time, time, style, keep);
    }

    public Animation Animate(string name, Shape shape, DateTime time, Style style, bool keep = true) {
      return Animate(name, shape, time, time, style, keep);
    }

    public Animation Animate(Shape shape, DateTime time0, DateTime time1, Style style, bool keep = true) {
      return Animate(GetNextAnimationName(), shape, shape, time0, time1, style, keep);
    }

    public Animation Animate(string name, Shape shape, DateTime time0, DateTime time1, Style style, bool keep = true) {
      return Animate(name, shape, shape, time0, time1, style, keep);
    }

    public Animation Animate(Shape shape0, Shape shape1, DateTime time0, DateTime time1, Style style, bool keep = true) {
      return Animate(GetNextAnimationName(), shape0, shape1, time0, time1, style, keep);
    }

    public Animation Animate(string name, Shape shape0, Shape shape1, DateTime time0, DateTime time1, Style style, bool keep = true) {
      CheckType(shape0, shape1);
      Animation animation = new Animation(name, shape0, shape1, time0, time1, style, keep, this);
      AddProvider(animation);
      return animation;
    }

    public AdvancedAnimation Animate(AdvancedShape shape, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visible) {
      return Animate(GetNextAnimationName(), shape, fill, stroke, strokeWidth, visible);
    }

    public AdvancedAnimation Animate(string name, AdvancedShape shape, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visible) {
      AdvancedAnimation rectAnimation = new AdvancedAnimation(name, shape, fill, stroke, strokeWidth, visible);
      AddProvider(rectAnimation);
      return rectAnimation;
    }

    public QueueAnimation AnimateQueue(Shape shape, Style style, int space, int maxLength, QueueAnimation.QueueOrientation orientation = QueueAnimation.QueueOrientation.East) {
      return AnimateQueue(GetNextQueueName(), shape, style, space, maxLength, orientation);
    }

    public QueueAnimation AnimateQueue(string name, Shape shape, Style style, int space, int maxLength, QueueAnimation.QueueOrientation orientation = QueueAnimation.QueueOrientation.East) {
      return new QueueAnimation(name, shape, style, space, maxLength, this, orientation);
    }

    public LevelAnimation AnimateLevel(Rect rect, Style style) {
      return AnimateLevel(GetNextLevelName(), rect, style);
    }

    public LevelAnimation AnimateLevel(string name, Rect rect, Style style) {
      return new LevelAnimation(name, rect, style, this);
    }
    #endregion

    public AnimationUtil GetAnimationUtil() {
      return new AnimationUtil(this);
    }

    #region Get Names
    private string GetNextAnimationName() {
      string animationName = "anim" + animationCount.ToString();
      animationCount++;
      return animationName;
    }

    private string GetNextQueueName() {
      string queueName = "queue" + queueCount.ToString();
      queueCount++;
      return queueName;
    }

    private string GetNextLevelName() {
      string levelName = "level" + levelCount.ToString();
      levelCount++;
      return levelName;
    }
    #endregion

    private void CheckType(Shape shape0, Shape shape1) {
      if (shape0.GetType() != shape1.GetType())
        throw new ArgumentException("Both shapes need to have the same type.");
    }

    public void AddName(string name) {
      CheckName(name);
      names.Add(name);
    }

    private void CheckName(string name) {
      if (name.Contains("/"))
        throw new ArgumentException("name must not contain '/'");
      if (names.Contains(name))
        throw new ArgumentException("Animations need to habe a unique name.");
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
      string name = provider.GetName();
      AddName(name);
      providers.Add(provider);
    }

    public void Step(DateTime now) {
      if (EnableAnimation) {
        int totalStart = Convert.ToInt32((prior - env.StartDate).TotalSeconds * FPS) + 1;
        int totalStop = Convert.ToInt32((now - env.StartDate).TotalSeconds * FPS);
        int totalFrameNumber = totalStop - totalStart + 1;

        // Console.WriteLine(prior + " - " + now);
        // Console.WriteLine(startFrameNumber + " - " + stopFrameNumber + ": " + totalFrameNumber);

        if (totalFrameNumber > 0) {
          if (providers.Count > 0) {
            SortedList<int, List<IEnumerator<string>>> frames = new SortedList<int, List<IEnumerator<string>>>();

            foreach (FramesProvider provider in providers) {
              List<AnimationUnit> units = provider.FramesFromTo(totalStart, totalStop);

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
              int stop = frames.Count > 1 ? frames.Keys[1] : totalStop + 1;

              int precedingFramesNumber = start - totalStart;
              WriteEmptyObjects(precedingFramesNumber);

              while (start <= totalStop) {
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
                stop = frames.Count > 1 ? frames.Keys[1] : totalStop + 1;
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