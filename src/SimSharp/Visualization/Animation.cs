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
    private List<AnimationProps> propsList;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env) 
      : this(name, Shape.rectangle, time0, time1, env) {
      AnimationProps props = new AnimationProps(rectangle0, rectangle1, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(props);

      if (env.FillAnimation)
        Initialize(props);
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.ellipse, time0, time1, env) {
      AnimationProps props = new AnimationProps(ellipse0, ellipse1, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(props);

      if (env.FillAnimation)
        Initialize(props);
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.polygon, time0, time1, env) {
      AnimationProps props = new AnimationProps(polygon0, polygon0, time0, time1, fillColor, lineColor, lineWidth, keep);
      propsList.Add(props);

      if (env.FillAnimation)
        Initialize(props);
    }

    private Animation(string name, Shape type, DateTime time0, DateTime time1, Simulation env) {
      CheckTime(time0, time1);

      Name = name;
      Type = type;
      this.env = env;
    }
    #endregion

    private void Initialize(AnimationProps props) {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);
      units = new List<AnimationUnit>();

      if (ShapesEqual(props) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props, 0));
        units.Add(unit);
      }
      else if (!ShapesEqual(props) && props.Time0.Equals(props.Time1) && props.Keep) {
        AnimationUnit unit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props, 1));
        units.Add(unit);
      }
      else if (ShapesEqual(props) && !props.Time0.Equals(props.Time1) && !props.Keep) {
        AnimationUnit firstUnit = new AnimationUnit(props.Time0, props.Time0.AddSeconds(1), 1);
        firstUnit.AddFrame(GetInitFrame(props, 0));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(props.Time1, props.Time1.AddSeconds(1), 1); // TODO
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      }
      else if (!ShapesEqual(props) && !props.Time0.Equals(props.Time1)) {
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

    #region Update
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.rectangle) {
        throw new ArgumentException("This animation is not of type 'Rectangle'.");
      } else {
        if (env.FillAnimation)
          Update(time0, time1, fillColor, lineColor, lineWidth, keep);
      }
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.ellipse) {
        throw new ArgumentException("This animation is not of type 'Ellipse'.");
      } else {
        if (env.FillAnimation)
          Update(time0, time1, fillColor, lineColor, lineWidth, keep);
      }
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.polygon) {
        throw new ArgumentException("This animation is not of type 'Polygon'.");
      } else {
        if (env.FillAnimation)
          Update(time0, time1, fillColor, lineColor, lineWidth, keep);
      }
    }
    #endregion

    private void Update(DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      CheckTime(time0, time1);

      units.RemoveAll(unit => unit.Time0 >= time0);
      foreach (AnimationUnit unit in units) {
        if (unit.Time1 > time0) { // TODO: > or >=
          // TODO
        }
      }
    }

    private void CheckTime(DateTime time0, DateTime time1) {
      if (time0 > time1)
        throw new ArgumentException("time1 must be after time0.");
      if ((time1 - time0).TotalMilliseconds < 1000)
        throw new ArgumentException("the difference between time0 and time1 must be greater than or equal to 1 second.");
      if (time0 > env.Now)
        throw new ArgumentException("time0 can not be in the past");
    }

    private string GetInitFrame(AnimationProps props, int z) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      writer.WritePropertyName("type");
      writer.WriteValue(Type.ToString());

      writer.WritePropertyName("fillColor");
      writer.WriteValue(props.FillColor);

      writer.WritePropertyName("lineColor");
      writer.WriteValue(props.LineColor);

      writer.WritePropertyName("lineWidth");
      writer.WriteValue(props.LineWidth);

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

    private bool ShapesEqual(AnimationProps props) {
      switch(Type) {
        case Shape.rectangle: return props.Rectangle0.Equals(props.Rectangle1);
        case Shape.ellipse: return props.Ellipse0.Equals(props.Ellipse1);
        case Shape.polygon: return props.Polygon0.Equals(props.Polygon1);
        default: return false;
      }
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