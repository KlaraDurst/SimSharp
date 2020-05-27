using System;
using System.Collections.Generic;
using System.IO;
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

    public Animation(string name, Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep, AnimationBuilder animationBuilder) {
      Name = Regex.Replace(name, @"\s+", "");
      this.animationBuilder = animationBuilder;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new SortedList<DateTime, AnimationProps>();
      this.units = new List<AnimationUnit>();

      CheckTime(time0, time1);
      int start = Convert.ToInt32((time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS) + 1;
      int stop = Convert.ToInt32((time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS);

      AnimationProps props = new AnimationProps(shape0, shape1, time0, time1, fill, stroke, strokeWidth, keep, start, stop);
      propsList.Add(time0, props);

      if (animationBuilder.EnableAnimation)
        FillUnits(props, false);
    }

    #region Update
    public void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, string fill, string stroke, int strokeWidth, bool keep) {
      CheckType(shape0, shape1);
      CheckTime(time0, time1);
      int start = Convert.ToInt32((time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS) + 1;
      int stop = Convert.ToInt32((time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.FPS);

      AnimationProps props = new AnimationProps(shape0, shape1, time0, time1, fill, stroke, strokeWidth, keep, start, stop);
      Update(props);
    }

    private void Update(AnimationProps props) {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        if (propsList.Values[j].Start >= props.Start)
          propsList.RemoveAt(j);
        else
          break;
      }
      propsList.Add(props.Time0, props);

      if (animationBuilder.EnableAnimation) {
        units.RemoveAll(unit => unit.Start >= props.Start);
        foreach (AnimationUnit unit in units) {
          if (unit.Stop >= props.Start) {
            int keepFrames = props.Start - unit.Start;
            unit.Frames.RemoveRange(keepFrames, unit.Frames.Count - keepFrames);
            unit.Stop = props.Start - 1;
          }
        }

        if (propsList.Count >= 2) {
          AnimationProps prev = propsList.Values[propsList.Count - 2];
          FillUnits(props, (!prev.Keep && prev.Stop + 1 < props.Start) ? false : true);
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
      int totalFrameNumber = props.Stop - props.Start + 1;

      if (currVisible && props.Start >= props.Stop && !props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Start, props.Start, 1);
        unit.AddFrame(GetRemoveFrame());
        units.Add(unit);
      } else if (ShapesEqual(props) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Start, props.Start, 1);
        unit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(unit);
      } else if (!ShapesEqual(props) && props.Start >= props.Stop && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Start, props.Start, 1);
        unit.AddFrame(GetInitFrame(props, 1, currVisible));
        units.Add(unit);
      } else if (ShapesEqual(props) && props.Start < props.Stop && !props.Keep) {
        AnimationUnit firstUnit = new AnimationUnit(props.Start, props.Start, 1);
        firstUnit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(props.Stop + 1, props.Stop + 1, 1);
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      } else if (!ShapesEqual(props) && props.Start < props.Stop) {
        List<AnimationUnit> unitList = new List<AnimationUnit>();
        List<string> frames = new List<string>();
        int unitStart = props.Start;

        frames.Add(GetInitFrame(props, 0, currVisible));

        Dictionary<string, int[]> startAttributes = GetAttributes(props, 0);
        Dictionary<string, int[]> stopAttributes = GetAttributes(props, 1);
        Dictionary<string, List<int>[]> interpolation = new Dictionary<string, List<int>[]>(startAttributes.Count);

        foreach (KeyValuePair<string, int[]> attr in startAttributes) {
          stopAttributes.TryGetValue(attr.Key, out int[] value);
          if (!props.Shape0.CompareAttributeValues(attr.Value, value))
            interpolation.Add(attr.Key, GetInterpolation(attr.Value, value, totalFrameNumber - 1));
        }

        Dictionary<string, int[]> prevAttributes = startAttributes;
        for (int i = 0; i < totalFrameNumber - 1; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          foreach (KeyValuePair<string, List<int>[]> attr in interpolation) {
            prevAttributes.TryGetValue(attr.Key, out int[] prevValue);
            if (!props.Shape0.CompareAttributeValues(attr.Value[i], prevValue)) {
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
              prevAttributes[attr.Key] = attr.Value[i].ToArray();
            }
          }

          writer.WriteEndObject();
          string frame = stringWriter.ToString();
          Flush();

          if (frame.Length <= Name.Length + 5) { // json object is empty
            int unitStop = unitStart + frames.Count;
            if (frames.Count > 0) {
              AnimationUnit unit = new AnimationUnit(unitStart, unitStop, frames.Count);
              unit.AddFrameRange(frames);
              unitList.Add(unit);
              frames = new List<string>();
            }
            unitStart = unitStop + 1;
          } else {
            frames.Add(frame);
          }
        }
        if (!props.Keep) {
          frames.Add(GetRemoveFrame());
        }
        if (frames.Count > 0) {
          int unitStop = unitStart + frames.Count;
          AnimationUnit unit = new AnimationUnit(unitStart, unitStop, frames.Count);
          unit.AddFrameRange(frames);
          unitList.Add(unit);
        }
        units.AddRange(unitList);
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
        throw new ArgumentException("time0 can not be after time1.");
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

      if (prevWritten != null && prevWritten.Stop < props.Start) {
        Dictionary<string, int[]> valueAttributes = GetAttributes(props, z);
        Dictionary<string, int[]> prevValueAttributes = GetAttributes(prevWritten, 1);

        foreach (KeyValuePair<string, int[]> attr in valueAttributes) {
          prevValueAttributes.TryGetValue(attr.Key, out int[] prevValue);
          if (!props.Shape1.CompareAttributeValues(prevValue, attr.Value)) {
            writer.WritePropertyName(attr.Key);
            if (attr.Value.Length < 2) {
              writer.WriteValue(attr.Value[0]);
            } else {
              writer.WriteStartArray();
              foreach (int val in attr.Value) {
                writer.WriteValue(val);
              }
              writer.WriteEndArray();
            }
          }
        }
      } else {
        foreach (KeyValuePair<string, int[]> attr in GetAttributes(props, z)) {
          writer.WritePropertyName(attr.Key);
          if (attr.Value.Length < 2) {
            writer.WriteValue(attr.Value[0]);
          } else {
            writer.WriteStartArray();
            foreach (int val in attr.Value) {
              writer.WriteValue(val);
            }
            writer.WriteEndArray();
          }
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

    public string GetName() {
      return Name;
    }
  }
}