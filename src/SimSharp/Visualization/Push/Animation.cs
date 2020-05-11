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
    public enum Shape { rect, ellipse, polygon }

    public string Name { get; }
    public Shape Type { get; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private SortedList<DateTime, AnimationProps> propsList;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Rect rect0, Rect rect1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep, Simulation env) 
      : this(name, Shape.rect, time0, time1, env) {
      AnimationProps props = new AnimationProps(rect0, rect1, time0, time1, fill, stroke, strokeWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props, false);
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep, Simulation env)
      : this(name, Shape.ellipse, time0, time1, env) {
      AnimationProps props = new AnimationProps(ellipse0, ellipse1, time0, time1, fill, stroke, strokeWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props, false);
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep, Simulation env)
      : this(name, Shape.polygon, time0, time1, env) {
      AnimationProps props = new AnimationProps(polygon0, polygon1, time0, time1, fill, stroke, strokeWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props, false);
    }

    private Animation(string name, Shape type, DateTime time0, DateTime time1, Simulation env) {
      Name = Regex.Replace(name, @"\s+", "");
      Type = type;
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new SortedList<DateTime, AnimationProps>();
      this.units = new List<AnimationUnit>();

      CheckTime(time0, time1);
    }
    #endregion

    #region Update
    public void Update(Rect rect0, Rect rect1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      CheckType(Shape.rect);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(rect0, rect1, time0, time1, fill, stroke, strokeWidth, keep);
      Update(props);
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      CheckType(Shape.ellipse);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(ellipse0, ellipse1, time0, time1, fill, stroke, strokeWidth, keep);
      Update(props);
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      CheckType(Shape.polygon);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(polygon0, polygon1, time0, time1, fill, stroke, strokeWidth, keep);
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

      if (env.FillAnimation) {
        int startFrameNumber = Convert.ToInt32((props.Time0 - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep) + 1;
        int stopFrameNumber = Convert.ToInt32((props.Time1 - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep);

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
    public Rect GetRect0() {
      CheckType(Shape.rect);
      return GetCurrentProps().Rect0;
    }

    public Rect GetRect1() {
      CheckType(Shape.rect);
      return GetCurrentProps().Rect1;
    }

    public Ellipse GetEllipse0() {
      CheckType(Shape.ellipse);
      return GetCurrentProps().Ellipse0;
    }

    public Ellipse GetEllipse1() {
      CheckType(Shape.ellipse);
      return GetCurrentProps().Ellipse1;
    }

    public Polygon GetPolygon0() {
      CheckType(Shape.polygon);
      return GetCurrentProps().Polygon0;
    }

    public Polygon GetPolygon1() {
      CheckType(Shape.polygon);
      return GetCurrentProps().Polygon1;
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
      int startFrameNumber = Convert.ToInt32((props.Time0 - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep) + 1;
      int stopFrameNumber = Convert.ToInt32((props.Time1 - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep);

      if (currVisible && props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit unit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        unit.AddFrame(GetRemoveFrame());
        units.Add(unit);
      } else if (ShapesEqual(props) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        unit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(unit);
      } else if (!ShapesEqual(props) && props.Time0.Equals(props.Time1) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        unit.AddFrame(GetInitFrame(props, 1, currVisible));
        units.Add(unit);
      } else if (ShapesEqual(props) && !props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit firstUnit = new AnimationUnit(startFrameNumber, startFrameNumber, 1);
        firstUnit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(stopFrameNumber + 1, stopFrameNumber + 1, 1);
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      } else if (!ShapesEqual(props) && !props.Time0.Equals(props.Time1)) {
        int frameNumber = stopFrameNumber - startFrameNumber + 1;
        AnimationUnit animationUnit = props.Keep ? new AnimationUnit(startFrameNumber, stopFrameNumber, frameNumber) : new AnimationUnit(startFrameNumber, stopFrameNumber + 1, frameNumber + 1);
        animationUnit.AddFrame(GetInitFrame(props, 0, currVisible));

        Dictionary<string, int[]> startTransformation = GetTransformation(props, 0);
        Dictionary<string, int[]> stopTransformation = GetTransformation(props, 1);
        Dictionary<string, List<int>[]> interpolation = new Dictionary<string, List<int>[]>(startTransformation.Count);

        foreach (KeyValuePair<string, int[]> attr in startTransformation) {
          stopTransformation.TryGetValue(attr.Key, out int[] value);
          if (!attr.Value.SequenceEqual(value))
            interpolation.Add(attr.Key, GetInterpolation(attr.Value, value, frameNumber - 1));
        }

        for (int i = 0; i < frameNumber - 1; i++) {
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
        if (propsList.Keys[j] <= env.Now)
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
      if (!time0.Equals(time1) && (time1 - time0).TotalMilliseconds < 1000)
        throw new ArgumentException("the difference between time0 and time1 must be greater than or equal to 1 second.");
      if (time0 > env.Now)
        throw new ArgumentException("time0 can not be in the past");
    }
    
    private void CheckType(Shape shape) {
      if (Type != shape) {
        throw new ArgumentException("This animation is not of type " + shape.ToString().ToUpper());
      }
    }

    private bool ShapesEqual(AnimationProps props) {
      switch (Type) {
        case Shape.rect: return props.Rect0.Equals(props.Rect1);
        case Shape.ellipse: return props.Ellipse0.Equals(props.Ellipse1);
        case Shape.polygon: return props.Polygon0.Equals(props.Polygon1);
        default: return false;
      }
    }

    private string GetInitFrame(AnimationProps props, int z, bool currVisible) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      AnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null) {
        writer.WritePropertyName("type");
        writer.WriteValue(Type.ToString());

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
        writer.WritePropertyName("visible");
        writer.WriteValue(true);
      }

      foreach (KeyValuePair<string, int[]> attr in GetTransformation(props, z)) {
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

      writer.WritePropertyName("visible");
      writer.WriteValue(false);

      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

      return frame;
    }

    private Dictionary<string, int[]> GetTransformation(AnimationProps props, int z) {
      switch (Type) {
        case Shape.rect: return z==0 ? props.Rect0.GetTransformation() : props.Rect1.GetTransformation();
        case Shape.ellipse: return z == 0 ? props.Ellipse0.GetTransformation() : props.Ellipse1.GetTransformation();
        case Shape.polygon: return z == 0 ? props.Polygon0.GetTransformation() : props.Polygon1.GetTransformation();
        default: return null;
      }
    }

    // excl. start, incl. stop
    private List<int>[] GetInterpolation(int[] start, int[] stop, int frameNumber) {
      double interval = 1 / Convert.ToDouble(frameNumber);
      List<int>[] interpolation = new List<int>[frameNumber];

      for (int i = 1; i <= frameNumber; i++) { // int i = 0; i < frameNumber; i++ to test if t is compared to former t when animation is updated
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