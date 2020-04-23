using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push {
  public class Animation : FramesProvider {
    public enum Shape { rectangle, ellipse, polygon }

    public string Name { get; }
    public Shape Type { get; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private SortedList<DateTime, AnimationProps> propsList;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env) 
      : this(name, Shape.rectangle, time0, time1, env) {
      AnimationProps props = new AnimationProps(rectangle0, rectangle1, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props, false);
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.ellipse, time0, time1, env) {
      AnimationProps props = new AnimationProps(ellipse0, ellipse1, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props, false);
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.polygon, time0, time1, env) {
      AnimationProps props = new AnimationProps(polygon0, polygon0, time0, time1, fillColor, lineColor, lineWidth, keep);
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
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      CheckType(Shape.rectangle);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(rectangle0, rectangle1, time0, time1, fillColor, lineColor, lineWidth, keep);
      Update(props);
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      CheckType(Shape.ellipse);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(ellipse0, ellipse1, time0, time1, fillColor, lineColor, lineWidth, keep);
      Update(props);
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      CheckType(Shape.polygon);
      CheckTime(time0, time1);

      AnimationProps props = new AnimationProps(polygon0, polygon1, time0, time1, fillColor, lineColor, lineWidth, keep);
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
        units.RemoveAll(unit => unit.Time0 >= props.Time0);
        foreach (AnimationUnit unit in units) {
          if (unit.Time1 > props.Time0) {
            int  keepFrames = Convert.ToInt32((props.Time0 - unit.Time0).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
            unit.Frames.RemoveRange(keepFrames, unit.Frames.Count - keepFrames);
            unit.Time1 = props.Time0;
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
    public Rectangle GetRectangle0() {
      CheckType(Shape.rectangle);
      return GetCurrentProps().Rectangle0;
    }

    public Rectangle GetRectangle1() {
      CheckType(Shape.rectangle);
      return GetCurrentProps().Rectangle1;
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
      return GetCurrentProps().FillColor;
    }

    public string GetLineColor() {
      return GetCurrentProps().LineColor;
    }
    
    public int GetLineWidth() {
      return GetCurrentProps().LineWidth;
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
      if (currVisible && props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetRemoveFrame());
        units.Add(unit);
      } else if (ShapesEqual(props) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(unit);
      } else if (!ShapesEqual(props) && props.Time0.Equals(props.Time1) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props, 1, currVisible));
        units.Add(unit);
      } else if (ShapesEqual(props) && !props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit firstUnit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        firstUnit.AddFrame(GetInitFrame(props, 0, currVisible));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(props.Time1, props.Time1.AddSeconds(1), 1);
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      } else if (!ShapesEqual(props) && !props.Time0.Equals(props.Time1)) {
        int frameNumber = Convert.ToInt32((props.Time1 - props.Time0).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
        AnimationUnit animationUnit = props.Keep ? new AnimationUnit(props.Time0, props.Time1, frameNumber) : new AnimationUnit(props.Time0, props.Time1.AddSeconds(1), frameNumber + 1);
        animationUnit.AddFrame(GetInitFrame(props, 0, currVisible));

        foreach (List<int> i in GetInterpolation(GetTransformation(props, 0), GetTransformation(props, 1), frameNumber - 2)) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          writer.WritePropertyName("t");
          writer.WriteStartArray();
          foreach (int t in i) {
            writer.WriteValue(t);
          }
          writer.WriteEndArray();

          writer.WriteEndObject();
          animationUnit.AddFrame(stringWriter.ToString());
          Flush();
        }

        writer.WritePropertyName(Name);
        writer.WriteStartObject();

        writer.WritePropertyName("t");
        writer.WriteStartArray();
        foreach (int t in GetTransformation(props, 1)) {
          writer.WriteValue(t);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
        animationUnit.AddFrame(stringWriter.ToString());
        Flush();

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
        case Shape.rectangle: return props.Rectangle0.Equals(props.Rectangle1);
        case Shape.ellipse: return props.Ellipse0.Equals(props.Ellipse1);
        case Shape.polygon: return props.Polygon0.Equals(props.Polygon1);
        default: return false;
      }
    }

    private string GetInitFrame(AnimationProps props, int z, bool currVisible) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      if (propsList.Count < 2) {
        writer.WritePropertyName("type");
        writer.WriteValue(Type.ToString());

        writer.WritePropertyName("fillColor");
        writer.WriteValue(props.FillColor);

        writer.WritePropertyName("lineColor");
        writer.WriteValue(props.LineColor);

        writer.WritePropertyName("lineWidth");
        writer.WriteValue(props.LineWidth);
      } else {
        AnimationProps prev = propsList.Values[propsList.Count - 2];

        if (prev.FillColor != props.FillColor) {
          writer.WritePropertyName("fillColor");
          writer.WriteValue(props.FillColor);
        }

        if (prev.LineColor != props.LineColor) {
          writer.WritePropertyName("lineColor");
          writer.WriteValue(props.LineColor);
        }

        if (prev.LineWidth != props.LineWidth) {
          writer.WritePropertyName("lineWidth");
          writer.WriteValue(props.LineWidth);
        }
      }

      if (!currVisible) {
        writer.WritePropertyName("visible");
        writer.WriteValue(true);
      }

      writer.WritePropertyName("t");
      writer.WriteStartArray();
      foreach (int t in GetTransformation(props, z)) {
        writer.WriteValue(t);
      }
      writer.WriteEndArray();

      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

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

    private int[] GetTransformation(AnimationProps props, int z) {
      switch (Type) {
        case Shape.rectangle: return z==0 ? props.Rectangle0.GetTransformation() : props.Rectangle1.GetTransformation();
        case Shape.ellipse: return z == 0 ? props.Ellipse0.GetTransformation() : props.Ellipse1.GetTransformation();
        case Shape.polygon: return z == 0 ? props.Polygon0.GetTransformation() : props.Polygon1.GetTransformation();
        default: return null;
      }
    }

    private IEnumerable<List<int>> GetInterpolation(int[] start, int[] stop, int frameNumber) {
      double interval = 1 / Convert.ToDouble(frameNumber);
      List<List<int>> interpolation = new List<List<int>>(frameNumber);

      for (int i = 0; i < frameNumber; i++) {
        List<int> l = new List<int>(start.Length);

        for (int j = 0; j < start.Length; j++) {
          double t = interval * i;
          int val = Convert.ToInt32((1 - t) * start[j] + t * stop[j]);
          l.Add(val);
        }

        interpolation.Add(l);
      }

      return interpolation;
    }

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    public List<AnimationUnit> FramesFromTo(DateTime start, DateTime stop) {
      List<AnimationUnit> affectedUnits = new List<AnimationUnit>();
      
      foreach (AnimationUnit unit in units) {
        if (unit.Time0 >= start && unit.Time0 < stop) {
          affectedUnits.Add(unit);
        }
        else if (unit.Time0 <= start && unit.Time1 > start) {
          int removeFrames = Convert.ToInt32((start - unit.Time0).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
          int keepFrames = unit.Frames.Count - removeFrames;
          AnimationUnit temp = new AnimationUnit(start, unit.Time1, keepFrames);
          
          temp.AddFrameRange(unit.Frames.GetRange(removeFrames, keepFrames));
          affectedUnits.Add(temp);
        }
      }
      return affectedUnits;
    }
  }
}