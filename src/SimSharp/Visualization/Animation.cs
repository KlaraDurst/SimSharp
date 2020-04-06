using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  public class Animation {
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
        FillUnits(props);
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.ellipse, time0, time1, env) {
      AnimationProps props = new AnimationProps(ellipse0, ellipse1, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props);
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.polygon, time0, time1, env) {
      AnimationProps props = new AnimationProps(polygon0, polygon0, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(time0, props);

      if (env.FillAnimation)
        FillUnits(props);
    }

    private Animation(string name, Shape type, DateTime time0, DateTime time1, Simulation env) {
      CheckTime(time0, time1);

      Name = name;
      Type = type;
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new SortedList<DateTime, AnimationProps>();
      this.units = new List<AnimationUnit>();
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
        FillUnits(props);
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

    private void FillUnits(AnimationProps props) {
      if (propsList.Count > 1 && props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetRemoveFrame());
        units.Add(unit);
      }
      else if (ShapesEqual(props) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props, 0));
        units.Add(unit);
      } else if (!ShapesEqual(props) && props.Time0.Equals(props.Time1) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props, 1));
        units.Add(unit);
      } else if (ShapesEqual(props) && !props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit firstUnit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        firstUnit.AddFrame(GetInitFrame(props, 0));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(props.Time1, props.Time1.AddSeconds(1), 1);
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      } else if (!ShapesEqual(props) && !props.Time0.Equals(props.Time1)) {
        int frameNumber = Convert.ToInt32((props.Time1 - props.Time0).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
        AnimationUnit animationUnit = props.Keep ? new AnimationUnit(props.Time0, props.Time1, frameNumber) : new AnimationUnit(props.Time0, props.Time1.AddSeconds(1), frameNumber + 1);
        animationUnit.AddFrame(GetInitFrame(props, 0));

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
          animationUnit.AddFrame(writer.ToString());
          writer.Flush();
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
        animationUnit.AddFrame(writer.ToString());
        writer.Flush();

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
      if ((time1 - time0).TotalMilliseconds < 1000)
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

    private string GetInitFrame(AnimationProps props, int z) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      if (propsList.Count < 2) {
        writer.WritePropertyName("type");
        writer.WriteValue(Type.ToString());
      }

      if (propsList.Count >= 2 && propsList.Values[propsList.Count - 2].FillColor != props.FillColor) {
        writer.WritePropertyName("fillColor");
        writer.WriteValue(props.FillColor);
      }

      if (propsList.Count >= 2 && propsList.Values[propsList.Count - 2].LineColor != props.LineColor) {
        writer.WritePropertyName("lineColor");
        writer.WriteValue(props.LineColor);
      }

      if (propsList.Count >= 2 && propsList.Values[propsList.Count - 2].LineWidth != props.LineWidth) {
        writer.WritePropertyName("lineWidth");
        writer.WriteValue(props.LineWidth);
      }

      writer.WritePropertyName("visible");
      writer.WriteValue(true);

      writer.WritePropertyName("t");
      writer.WriteStartArray();
      foreach (int t in GetTransformation(props, z)) {
        writer.WriteValue(t);
      }
      writer.WriteEndArray();

      writer.WriteEndObject();
      string frame = writer.ToString();
      writer.Flush();

      return frame;
    }

    private string GetRemoveFrame() {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      writer.WritePropertyName("visible");
      writer.WriteValue(false);

      writer.WriteEndObject();
      string frame = writer.ToString();
      writer.Flush();

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
      double interval = 1 / frameNumber;
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

    // TODO: start and stop included or excluded?
    public List<AnimationUnit> FramesFromTo(DateTime start, DateTime stop) {
      // TODO
    }
  }
}