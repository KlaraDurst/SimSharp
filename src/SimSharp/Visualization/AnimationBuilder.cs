using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Advanced;
using SimSharp.Visualization.Advanced.AdvancedShapes;
using SimSharp.Visualization.Advanced.AdvancedStyles;
using SimSharp.Visualization.Basic;
using SimSharp.Visualization.Basic.Resources;
using SimSharp.Visualization.Basic.Shapes;
using SimSharp.Visualization.Basic.Styles;
using SimSharp.Visualization.Processor;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    public AnimationConfig Config { get; }

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
    private List<FramesProcessor> processors;
    private List<FramesProvider> providers;
    private List<string> names; 
    private DateTime prior;
    private int frameCount;
    private int animationCount;
    private int queueCount;
    private int levelCount;

    #region Constructors
    public AnimationBuilder() : this(0, 0, 0, 0, "Visualization", 1, false) { }
    public AnimationBuilder(int width, int height) : this(width, height, "Visualization") { }
    public AnimationBuilder(int width, int height, string name) : this(width, height, 0, 0, name, 1) { }
    public AnimationBuilder(int width, int height, int fps) : this(width, height, "Visualization", fps) { }
    public AnimationBuilder(int width, int height, string name, int fps) : this(width, height, 0, 0, name, fps) { }
    public AnimationBuilder(int width, int height, int startX, int startY) : this(width, height, startX, startY, "Visualization") { }
    public AnimationBuilder(int width, int height, int startX, int startY, int fps) : this(width, height, startX, startY, "Visualization", fps) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name) : this(width, height, startX, startY, name, 1) { }
    public AnimationBuilder(int width, int height, int startX, int startY, string name, int fps) : this(width, height, startX, startY, name, fps, true) { }

    private AnimationBuilder(int width, int height, int startX, int startY, string name, int fps, bool setCanvas) {
      if (fps > 60)
        throw new ArgumentException("fps can not be higher than 60.");

      Config = new AnimationConfig(name, fps, width, height, startX, startY, setCanvas);

      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.processors = new List<FramesProcessor>();
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

    public AdvancedAnimation Animate(AdvancedShape shape, AdvancedStyle style, AnimationAttribute<bool> visibility) {
      return Animate(GetNextAnimationName(), shape, style, visibility);
    }

    public AdvancedAnimation Animate(string name, AdvancedShape shape, AdvancedStyle style, AnimationAttribute<bool> visibility) {
      AdvancedAnimation animation = new AdvancedAnimation(name, shape, style, visibility);
      AddProvider(animation);
      return animation;
    }

    public AdvancedGroupAnimation Animate(AdvancedGroup group, AdvancedStyle style, AnimationAttribute<bool> visibility) {
      return Animate(GetNextAnimationName(), group, style, visibility);
    }

    public AdvancedGroupAnimation Animate(string name, AdvancedGroup group, AdvancedStyle style, AnimationAttribute<bool> visibility) {
      AdvancedGroupAnimation groupAnimation = new AdvancedGroupAnimation(name, group, style, visibility);
      AddProvider(groupAnimation);
      return groupAnimation;
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

    private void AddName(string name) {
      CheckName(name);
      names.Add(name);
    }

    private void CheckName(string name) {
      if (name.Equals("start"))
        throw new ArgumentException("name must not be 'start'");
      if (name.Equals("stop"))
        throw new ArgumentException("name must not be 'stop'");
      if (name.Contains("/"))
        throw new ArgumentException("name must not contain '/'");
      if (names.Contains(name))
        throw new ArgumentException("Animations need to have a unique name.");
    }

    public void StartBuilding() {
      if (EnableAnimation && processors.Count > 0) {
        SendStart(Config);
      }
    }

    public void AddProcessor(FramesProcessor processor) {
      processors.Add(processor);
    }

    private void AddProvider(FramesProvider provider) {
      string name = provider.GetName();
      AddName(name);
      providers.Add(provider);
    }

    public void Step(DateTime now) {
      if (EnableAnimation && processors.Count > 0) {
        int totalStart = Convert.ToInt32((prior - env.StartDate).TotalSeconds * Config.FPS) + 1;
        int totalStop = Convert.ToInt32((now - env.StartDate).TotalSeconds * Config.FPS);
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
                    string animFrame = framesEnums[j].Current;

                    if (DebugAnimation || !first)
                      writer.WriteRaw(",");
                    writer.WriteRaw(animFrame);

                    first = false;
                  }
                  writer.WriteEndObject();

                  string frame = stringWriter.ToString();
                  SendFrame(frame);
                  Flush();
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

        string frame = stringWriter.ToString();
        SendFrame(frame);
        Flush();
      }
    }

    public void StopBuilding() {
      if (EnableAnimation && processors.Count > 0) {
        SendStop();
      }
    }

    private void WriteFrameNumber() {
      if (DebugAnimation) {
        writer.WritePropertyName("frame");
        writer.WriteValue(frameCount);
        frameCount++;
      }
    }

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    private void SendStart(AnimationConfig config) {
      foreach (FramesProcessor processor in processors) {
        processor.SendStart(config);
      }
    }

    private void SendFrame(string frame) {
      foreach (FramesProcessor processor in processors) {
        processor.SendFrame(frame);
      }
    }

    private void SendStop() {
      foreach(FramesProcessor processor in processors) {
        processor.SendStop();
      }
    }
  }
}