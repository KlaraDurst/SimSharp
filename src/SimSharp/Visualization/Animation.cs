using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  public class Animation {
    public enum Shape { rectangle, ellipse, polygon }

    public Rectangle Rectangle0 { get; }
    public Rectangle Rectangle1 { get; }

    public Ellipse Ellipse0 { get; }
    public Ellipse Ellipse1 { get; }

    public Polygon Polygon0 { get; }
    public Polygon Polygon1 { get; }

    public string Name { get; }
    public Shape Type { get; }
    public string FillColor { get; }
    public string LineColor { get; }
    public int LineWidth { get; }
    public DateTime Time0 { get; }
    public DateTime Time1 { get; }
    public bool Keep { get; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env) 
      : this(name, Shape.rectangle, fillColor, lineColor, lineWidth, time0, time1, keep, env) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;

      if (env.FillAnimation)
        Initialize();
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.ellipse, fillColor, lineColor, lineWidth, time0, time1, keep, env) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;

      if (env.FillAnimation)
        Initialize();
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, Simulation env)
      : this(name, Shape.polygon, fillColor, lineColor, lineWidth, time0, time1, keep, env) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;

      if (env.FillAnimation)
        Initialize();
    }

    private Animation(string name, Shape type, string fillColor, string lineColor, int lineWidth, DateTime time0, DateTime time1, bool keep, Simulation env) {
      CheckTime(time0, time1);

      Name = name;
      Type = type;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = LineWidth;
      Time0 = time0;
      Time1 = time1;
      Keep = keep;
      this.env = env;
    }
    #endregion

    private void Initialize() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);
      units = new List<AnimationUnit>();

      if (ShapesEqual() && Keep) {
        AnimationUnit unit = new AnimationUnit(Time0, Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(0));
        units.Add(unit);
      }
      else if (!ShapesEqual() && Time0.Equals(Time1) && Keep) {
        AnimationUnit unit = new AnimationUnit(Time0, Time0.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(1));
        units.Add(unit);
      }
      else if (ShapesEqual() && !Time0.Equals(Time1) && !Keep) {
        AnimationUnit firstUnit = new AnimationUnit(Time0, Time0.AddSeconds(1), 1);
        firstUnit.AddFrame(GetInitFrame(0));
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(Time1, Time1.AddSeconds(1), 1); // TODO
        secondUnit.AddFrame(GetRemoveFrame());
        units.Add(secondUnit);
      }
      else if (!ShapesEqual() && !Time0.Equals(Time1)) {
        int frameNumber = Convert.ToInt32((Time1 - Time0).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
        AnimationUnit animationUnit = Keep ? new AnimationUnit(Time0, Time1, frameNumber) : new AnimationUnit(Time0, Time1.AddSeconds(1), frameNumber + 1);
        animationUnit.AddFrame(GetInitFrame(0));

        foreach (List<int> i in GetInterpolation(GetTransformation(0), GetTransformation(1), frameNumber - 2)) {
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
        foreach (int t in GetTransformation(1)) {
          writer.WriteValue(t);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
        animationUnit.AddFrame(writer.ToString());
        writer.Flush();

        if (!Keep) {
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

    private string GetInitFrame(int z) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      writer.WritePropertyName("type");
      writer.WriteValue(Type.ToString());

      writer.WritePropertyName("fillColor");
      writer.WriteValue(FillColor);

      writer.WritePropertyName("lineColor");
      writer.WriteValue(LineColor);

      writer.WritePropertyName("lineWidth");
      writer.WriteValue(LineWidth);

      writer.WritePropertyName("visible");
      writer.WriteValue(true);

      writer.WritePropertyName("t");
      writer.WriteStartArray();
      foreach (int t in GetTransformation(z)) {
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

    private bool ShapesEqual() {
      switch(Type) {
        case Shape.rectangle: return Rectangle0.Equals(Rectangle1);
        case Shape.ellipse: return Ellipse0.Equals(Ellipse1);
        case Shape.polygon: return Polygon0.Equals(Polygon1);
        default: return false;
      }
    }

    private int[] GetTransformation(int z) {
      switch (Type) {
        case Shape.rectangle: return z==0 ? Rectangle0.GetTransformation() : Rectangle1.GetTransformation();
        case Shape.ellipse: return z == 0 ? Ellipse0.GetTransformation() : Ellipse1.GetTransformation();
        case Shape.polygon: return z == 0 ? Polygon0.GetTransformation() : Polygon1.GetTransformation();
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