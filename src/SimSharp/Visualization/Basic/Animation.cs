using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Basic.Shapes;
using SimSharp.Visualization.Basic.Styles;

namespace SimSharp.Visualization.Basic {
  public class Animation : FramesProvider {

    public string Name { get; }

    private AnimationBuilder animationBuilder;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private SortedList<DateTime, AnimationProps> propsList;
    private List<AnimationUnit> units;

    public Animation(string name, Shape shape0, Shape shape1, DateTime time0, DateTime time1, Style style, bool keep, AnimationBuilder animationBuilder) {
      Name = Regex.Replace(name, @"\s+", "");
      this.animationBuilder = animationBuilder;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new SortedList<DateTime, AnimationProps>();
      this.units = new List<AnimationUnit>();

      CheckTime(time0, time1);
      int start = Convert.ToInt32((time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.Config.FPS) + 1;
      int stop = Convert.ToInt32((time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.Config.FPS);

      AnimationProps props = new AnimationProps(shape0, shape1, time0, time1, style, keep, start, stop);
      propsList.Add(time0, props);

      if (animationBuilder.EnableAnimation)
        FillUnits(props, false);
    }

    #region Update
    public void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, bool keep = true) {
      Update(shape0, shape1, time0, time1, GetLastWrittenProps().Style, keep);
    }

    public void Update(Shape shape0, Shape shape1, DateTime time0, DateTime time1, Style style, bool keep = true) {
      CheckType(shape0, shape1);
      CheckTime(time0, time1);
      int start = Convert.ToInt32((time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.Config.FPS) + 1;
      int stop = Convert.ToInt32((time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.Config.FPS);

      Update(new AnimationProps(shape0, shape1, time0, time1, style, keep, start, stop));
    }

    public void Update(Shape shape1, bool keep = true) {
      Update(shape1, GetLastWrittenProps().Style, keep);
    }

    public void Update(Shape shape1, Style style, bool keep = true) {
      Update(shape1, shape1, animationBuilder.Env.Now, animationBuilder.Env.Now, style, keep);
    }

    public void Update(Shape shape1, DateTime time1, bool keep = true) {
      Update(shape1, animationBuilder.Env.Now, time1, GetLastWrittenProps().Style, keep);
    }

    public void Update(Shape shape1, DateTime time0, DateTime time1, bool keep = true) {
      Update(shape1, time0, time1, GetLastWrittenProps().Style, keep);
    }

    public void Update(Shape shape1, DateTime time0, DateTime time1, Style style, bool keep = true) {
      CheckType(shape1);
      CheckTime(time0, time1);
      int start = Convert.ToInt32((time0 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.Config.FPS) + 1;
      int stop = Convert.ToInt32((time1 - animationBuilder.Env.StartDate).TotalSeconds * animationBuilder.Config.FPS);
      Dictionary<string, int[]> attributesAt = null;
      List<string> attributeNames = new List<string>();

      foreach (KeyValuePair<string, int[]> attr in shape1.GetAttributes()) {
        attributeNames.Add(attr.Key);
      }

      foreach (AnimationUnit unit in units) {
        if (unit.Stop > start) {
          attributesAt = unit.GetAttributesAt(start, attributeNames);
          break;
        }
      }

      if (attributesAt != null) {
        Shape shape0 = shape1.CopyAndSet(attributesAt);
        Update(new AnimationProps(shape0, shape1, time0, time1, style, keep, start, stop));
      } else {
        Update(new AnimationProps(GetLastWrittenProps().Shape1, shape1, time0, time1, style, keep, start, stop));
      }
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

    public Style GetStyle() {
      return GetCurrentProps().Style;
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
      if (time0 < animationBuilder.Env.Now)
        throw new ArgumentException("time0 can not be in the past");
    }
    
    private void CheckType(Shape shape0, Shape shape1) {
      if (shape0.GetType() != shape1.GetType())
        throw new ArgumentException("Both shapes need to have the same type.");
      CheckType(shape0);
    }

    private void CheckType(Shape shape) {
      if (shape.GetType() != GetCurrentProps().Shape0.GetType()) {
        throw new ArgumentException("This animation is not of type " + shape.GetType());
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
        
        props.Style.WriteJson(Name, writer, null);
      } else {
        props.Style.WriteJson(Name, writer, prevWritten.Style);
      }

      if (!currVisible) {
        writer.WritePropertyName("visibility");
        writer.WriteValue(true);
      }

      if (prevWritten != null && prevWritten.Stop < props.Start)
        WriteJson(props, z, writer, prevWritten.Shape1);
      else 
        WriteJson(props, z, writer, null);

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

    private void WriteJson(AnimationProps props, int z, JsonTextWriter writer, Shape compare) {
      if (z == 0)
        props.Shape0.WriteJson(writer, compare);
      else
        props.Shape1.WriteJson(writer, compare);
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