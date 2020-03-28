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
      if (time0 > time1)
        throw new ArgumentException("time1 must be after time0.");
      if ((time1 - time0).TotalMilliseconds < 1000)
        throw new ArgumentException("the difference between time0 and time1 must be greater than or equal to 1 second.");

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
        AnimationUnit unit = new AnimationUnit(Time0, Time0, 1);
        unit.AddFrame(GetInitFrame());
        units.Add(unit);
      }
      else if (ShapesEqual() && !Time0.Equals(Time1) && !Keep) {
        AnimationUnit firstUnit = new AnimationUnit(Time0, Time0, 1);
        firstUnit.AddFrame(GetInitFrame());
        units.Add(firstUnit);

        AnimationUnit secondUnit = new AnimationUnit(Time1, Time1, 1);
        writer.WritePropertyName(Name);
        writer.WriteStartObject();

        writer.WritePropertyName("visible");
        writer.WriteValue(false);

        writer.WriteEndObject();
        secondUnit.AddFrame(writer.ToString());
        writer.Flush();
        units.Add(secondUnit);
      }
      else if (!ShapesEqual() && !Time0.Equals(Time1) && Keep) {
        int frameNumber = Convert.ToInt32((Time1 - Time0).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
        AnimationUnit animationUnit = new AnimationUnit(Time0, Time1, frameNumber);
        animationUnit.AddFrame(GetInitFrame());

        // TODO
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

    private void Update(DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (time0 > time1)
        throw new ArgumentException("time1 must be after time0.");
      if ((time1 - time0).TotalMilliseconds < 1000)
        throw new ArgumentException("the difference between time0 and time1 must be greater than or equal to 1 second.");
      if (time0 > env.Now)
        throw new ArgumentException("time0 can not be in the past");
    }
    #endregion

    private string GetInitFrame() {
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
      foreach (int t in GetTransformation(0)) {
        writer.WriteValue(t);
      }
      writer.WriteEndArray();

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

    private List<int> GetTransformation(int z) {
      switch (Type) {
        case Shape.rectangle: return z==0 ? Rectangle0.GetTransformation() : Rectangle1.GetTransformation();
        case Shape.ellipse: return z == 0 ? Ellipse0.GetTransformation() : Ellipse1.GetTransformation();
        case Shape.polygon: return z == 0 ? Polygon0.GetTransformation() : Polygon1.GetTransformation();
        default: return null;
      }
    }

    public IEnumerator<List<int>> GetInterpolation(List<int> start, List<int> stop) {
      // TODO
    }

    public IEnumerator<string> FramesFromTo(DateTime start, DateTime stop) {
      // TODO
    }
  }
}