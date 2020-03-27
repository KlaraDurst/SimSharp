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

    private bool animate;
    private double timeStep;
    private int framesPerSec;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate, double timeStep) 
      : this(name, Shape.rectangle, fillColor, lineColor, lineWidth, time0, time1, keep, animate, timeStep) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;

      if (animate)
        Initialize();
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate, double timeStep)
      : this(name, Shape.ellipse, fillColor, lineColor, lineWidth, time0, time1, keep, animate, timeStep) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;

      if (animate)
        Initialize();
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate, double timeStep)
      : this(name, Shape.polygon, fillColor, lineColor, lineWidth, time0, time1, keep, animate, timeStep) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;

      if (animate)
        Initialize();
    }

    private Animation(string name, Shape type, string fillColor, string lineColor, int lineWidth, DateTime time0, DateTime time1, bool keep, bool animate, double timeStep) {
      Name = name;
      Type = type;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = LineWidth;
      Time0 = time0;
      Time1 = time1;
      Keep = keep;
      this.animate = animate;
      this.timeStep = timeStep;
      this.framesPerSec = Convert.ToInt32(1 / timeStep);
    }
    #endregion

    private void Initialize() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);
      units = new List<AnimationUnit>();

      if (ShapesEqual()) {
        AnimationUnit makeVisibleUnit = new AnimationUnit(Time0, Time0.AddSeconds(1), framesPerSec);
        makeVisibleUnit.AddFrame(GetInitFrame());
        units.Add(makeVisibleUnit);

        if (!Keep) {
          AnimationUnit makeUnvisibleUnit = new AnimationUnit(Time1, Time1.AddSeconds(1), framesPerSec);

          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          writer.WritePropertyName("visible");
          writer.WriteValue(false);

          writer.WriteEndObject();
          string frame = writer.ToString();
          writer.Flush();

          makeUnvisibleUnit.AddFrame(frame);
          units.Add(makeUnvisibleUnit);
        }
      }
      else {
        int frameNumber = Convert.ToInt32((Time1 - Time0).TotalSeconds / timeStep);
        AnimationUnit animationUnit = new AnimationUnit(Time0, Time1, frameNumber);

      }
    }

    #region Update
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.rectangle) {
        throw new ArgumentException("This animation is not of type 'Rectangle'");
      } else {
        if (animate)
          Update();
      }
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.ellipse) {
        throw new ArgumentException("This animation is not of type 'Ellipse'");
      } else {
        if (animate)
          Update();
      }
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.polygon) {
        throw new ArgumentException("This animation is not of type 'Polygon'");
      } else {
        if (animate)
          Update();
      }
    }

    private void Update() {

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

    }

    public IEnumerator<string> FramesFromTo(DateTime start, DateTime stop) {
      
    }
  }
}