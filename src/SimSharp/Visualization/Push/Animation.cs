using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push {
  public class Animation : FramesProvider {

    public string Name { get; }
    public Shape Type { get; }

    private AnimationBuilder animationBuilder;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private SortedList<DateTime, AnimationProps> propsList;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep, AnimationBuilder animationBuilder) 
      : this(name, time0, time1, animationBuilder) {
      AnimationProps props = new AnimationProps(shape0, shape1, time0, time1, fill, stroke, strokeWidth, keep);
      propsList.Add(time0, props);

      if (animationBuilder.EnableAnimation)
        FillUnits(props, false);
    }

    private Animation(string name, DateTime time0, DateTime time1, AnimationBuilder animationBuilder) {
      Name = Regex.Replace(name, @"\s+", "");
      this.animationBuilder = animationBuilder;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new SortedList<DateTime, AnimationProps>();
      this.units = new List<AnimationUnit>();

      CheckTime(time0, time1);
    }
    #endregion

    #region Update
    public void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      CheckType(shape0, shape1);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(shape0, shape1, time0, time1, fill, stroke, strokeWidth, keep);
      Update(props);
    }

    private void Update(AnimationProps props) {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        if (propsList.Keys[j] >= props.Time0)
          propsList.RemoveAt(j);
        else
          break;
      }
      propsList.Add(props.Time0, props);

      if (animationBuilder.EnableAnimation) {
        int startFrameNumber = Convert.ToInt32((props.Time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS) + 1;
        int stopFrameNumber = Convert.ToInt32((props.Time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS);

        units.RemoveAll(unit => unit.Start >= startFrameNumber);
        foreach (AnimationUnit unit in units) {
          if (unit.Stop >= startFrameNumber) {
            int keepFrames = startFrameNumber - unit.Start;
            unit.Frames.RemoveRange(keepFrames, unit.Frames.Count - keepFrames);
            unit.Stop = startFrameNumber - 1;
          }
        }

        if (propsList.Count >= 2) {
          AnimationProps prev = propsList.Values[propsList.Count - 2];
          FillUnits(props, (!prev.Keep && prev.Time1 < props.Time0) ? false : true);
        }
        else {
          FillUnits(props, false);
        }
      }
    }
    #endregion

    #region Get animation props
    public Shape GetShape0() {
      return GetCurrentProps().Shape0;
    }

    public Shape GetShape1() {
      return GetCurrentProps().Shape1;
    }

    public string GetFillColor() {
      return GetCurrentProps().Fill;
    }

    public string GetLineColor() {
      return GetCurrentProps().Stroke;
    }
    
    public int GetLineWidth() {
      return GetCurrentProps().StrokeWidth;
    }

    public DateTime GetTime0() {
      return GetCurrentProps().Time0;
    }

    public DateTime GetTime1() {
      return GetCurrentProps().Time1;
    }

    public bool GetKeep() {
      return GetCurrentProps().Keep;
    }
    #endregion

    private void FillUnits(AnimationProps props, bool currVisible) {
      int startFrameNumber = Convert.ToInt32((props.Time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS) + 1;
      int stopFrameNumber = Convert.ToInt32((props.Time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS);
      int totalFrameNumber = stopFrameNumber - startFrameNumber + 1;

      // Console.WriteLine(props.Time0 + " - " + props.Time1);
      // Console.WriteLine(startFrameNumber + " - " + stopFrameNumber + ": " + frameNumber);
      // Console.WriteLine();

      if (currVisible && startFrameNumber >= stopFrameNumber && !props.Keep) {
        AnimationUnit unit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        unit.AddFrame(GetRemoveFrame());
        units.Add(unit);
      } else if (ShapesEqual(props) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        unit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(unit);
      } else if (!ShapesEqual(props) && startFrameNumber >= stopFrameNumber && props.Keep) {
        AnimationUnit unit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        unit.AddFrame(GetInitFrame(props, 1, currVisible));
        units.Add(unit);
      } else if (ShapesEqual(props) && startFrameNumber < stopFrameNumber && !props.Keep) {
        AnimationUnit firstUnit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        firstUnit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(stopFrameNumber + 1, stopFrameNumber + 1, 1);
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      } else if (!ShapesEqual(props) && startFrameNumber < stopFrameNumber) {
        AnimationUnit animationUnit = props.Keep ? new AnimationUnit(startFrameNumber, stopFrameNumber, totalFrameNumber) : new AnimationUnit(startFrameNumber, stopFrameNumber + 1, totalFrameNumber + 1);
        animationUnit.AddFrame(GetInitFrame(props, 0, currVisible));

        Dictionary<string, int[]> startTransformation = GetAttributes(props, 0);
        Dictionary<string, int[]> stopTransformation = GetAttributes(props, 1);
        Dictionary<string, List<int>[]> interpolation = new Dictionary<string, List<int>[]>(startTransformation.Count);

        foreach (KeyValuePair<string, int[]> attr in startTransformation) {
          stopTransformation.TryGetValue(attr.Key, out int[] value);
          if (!attr.Value.SequenceEqual(value))
            interpolation.Add(attr.Key, GetInterpolation(attr.Value, value, totalFrameNumber - 1));
        }

        for (int i = 0; i < totalFrameNumber - 1; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          foreach (KeyValuePair<string, List<int>[]> attr in interpolation) {
            writer.WritePropertyName(attr.Key);
            if (attr.Value[i].Count < 2) {
              writer.WriteValue(attr.Value[i][0]);
            } else {
              writer.WriteStartArray();
              foreach (int val in attr.Value[i]) {
                writer.WriteValue(val);
              }
              writer.WriteEndArray();
            }
          }

          writer.WriteEndObject();
          animationUnit.AddFrame(stringWriter.ToString());
          Flush();
        }

        if (!props.Keep) {
          animationUnit.AddFrame(GetRemoveFrame());
        }

        units.Add(animationUnit);
      }
    }

    private AnimationProps GetCurrentProps() {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        if (propsList.Keys[j] <= animationBuilder.Env.Now)
          return propsList.Values[j];
      }
      return propsList.Values[0];
    }

    private AnimationProps GetLastWrittenProps() {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        AnimationProps props = propsList.Values[j];
        if (props.Written)
          return props;
      }
      return null;
    }

    private void CheckTime(DateTime time0, DateTime time1) {
      if (time0 > time1)
        throw new ArgumentException("time1 must be after time0.");
      if (time0 > animationBuilder.Env.Now)
        throw new ArgumentException("time0 can not be in the past");
    }
    
    private void CheckType(Shape shape0, Shape shape1) {
      if (shape0.GetType() != shape1.GetType())
        throw new ArgumentException("Both shapes need to have the same type.");
      if (shape0.GetType() != GetShape0().GetType()) {
        throw new ArgumentException("This animation is not of type " + shape0.GetType());
      }
    }

    private bool ShapesEqual(AnimationProps props) {
      return props.Shape0.Equals(props.Shape1);
    }

    private string GetInitFrame(AnimationProps props, int z, bool currVisible) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      AnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null) {
        writer.WritePropertyName("type");
        writer.WriteValue(props.Shape0.GetType().Name.ToLower());

        writer.WritePropertyName("fill");
        writer.WriteValue(props.Fill);

        writer.WritePropertyName("stroke");
        writer.WriteValue(props.Stroke);

        writer.WritePropertyName("strokeWidth");
        writer.WriteValue(props.StrokeWidth);
      } else {

        if (prevWritten.Fill != props.Fill) {
          writer.WritePropertyName("fill");
          writer.WriteValue(props.Fill);
        }

        if (prevWritten.Stroke != props.Stroke) {
          writer.WritePropertyName("stroke");
          writer.WriteValue(props.Stroke);
        }

        if (prevWritten.StrokeWidth != props.StrokeWidth) {
          writer.WritePropertyName("strokeWidth");
          writer.WriteValue(props.StrokeWidth);
        }
      }

      if (!currVisible) {
        writer.WritePropertyName("visibility");
        writer.WriteValue(true);
      }

      foreach (KeyValuePair<string, int[]> attr in GetAttributes(props, z)) {
        writer.WritePropertyName(attr.Key);
        if (attr.Value.Length < 2) {
          writer.WriteValue(attr.Value[0]);
        }
        else {
          writer.WriteStartArray();
          foreach (int val in attr.Value) {
            writer.WriteValue(val);
          }
          writer.WriteEndArray();
        }
      }

      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

      props.Written = true;
      return frame;
    }

    private string GetRemoveFrame() {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      writer.WritePropertyName("visibility");
      writer.WriteValue(false);

      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

      return frame;
    }

    private Dictionary<string, int[]> GetAttributes(AnimationProps props, int z) {
      return z == 0 ? props.Shape0.GetAttributes() : props.Shape1.GetAttributes();
    }

    // excl. start, incl. stop
    private List<int>[] GetInterpolation(int[] start, int[] stop, int frameNumber) {
      double interval = 1 / Convert.ToDouble(frameNumber);
      List<int>[] interpolation = new List<int>[frameNumber];

      for (int i = 1; i <= frameNumber; i++) {
        List<int> l = new List<int>(start.Length);

        for (int j = 0; j < start.Length; j++) {
          double t = interval * i;
          int val = Convert.ToInt32((1 - t) * start[j] + t * stop[j]);
          l.Add(val);
        }
        interpolation[i - 1] = l;
      }
      return interpolation;
    }

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    public List<AnimationUnit> FramesFromTo(int start, int stop) {
      List<AnimationUnit> affectedUnits = new List<AnimationUnit>();
      
      foreach (AnimationUnit unit in units) {
        if (unit.Start >= start && unit.Start <= stop) {
          affectedUnits.Add(unit);
        }
        else if (unit.Start < start && unit.Stop >= start) {
          int removeFrames = start - unit.Start;
          int keepFrames = unit.Frames.Count - removeFrames;
          AnimationUnit temp = new AnimationUnit(start, unit.Stop, keepFrames);
          
          temp.AddFrameRange(unit.Frames.GetRange(removeFrames, keepFrames));
          affectedUnits.Add(temp);
        }
      }
      return affectedUnits;
    }
  }
}