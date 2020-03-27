using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  public class Animation {
    public enum Shape { Rectangle, Ellipse, Polygon }

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
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<AnimationUnit> units;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate, double timeStep) 
      : this(name, Shape.Rectangle, fillColor, lineColor, lineWidth, time0, time1, keep, animate, timeStep) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;

      if (animate)
        Initialize();
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate, double timeStep)
      : this(name, Shape.Ellipse, fillColor, lineColor, lineWidth, time0, time1, keep, animate, timeStep) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;

      if (animate)
        Initialize();
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate, double timeStep)
      : this(name, Shape.Polygon, fillColor, lineColor, lineWidth, time0, time1, keep, animate, timeStep) {
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
    }
    #endregion

    private void Initialize() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);
      units = new List<AnimationUnit>();

      int frameNumber = Convert.ToInt32((Time1 - Time0).TotalSeconds / timeStep);
      AnimationUnit unit = new AnimationUnit(Time0, Time1, frameNumber);


    }

    #region Update
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Rectangle'");
      } else {
        if (animate)
          Update();
      }
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.Ellipse) {
        throw new ArgumentException("This animation is not of type 'Ellipse'");
      } else {
        if (animate)
          Update();
      }
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.Polygon) {
        throw new ArgumentException("This animation is not of type 'Polygon'");
      } else {
        if (animate)
          Update();
      }
    }

    private void Update() {

    }
    #endregion

    public IEnumerator<int> GetInterpolation(int x, int y) {

    }

    public IEnumerator<string> FramesFromTo(DateTime start, DateTime stop) {
      
    }
  }
}