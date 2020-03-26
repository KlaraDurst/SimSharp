using System;
using System.Collections.Generic;
using System.Text;
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

    private Array frames;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate) 
      : this(name, Shape.Rectangle, fillColor, lineColor, lineWidth, time0, time1, keep, animate) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;

      Initialize();
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate)
      : this(name, Shape.Ellipse, fillColor, lineColor, lineWidth, time0, time1, keep, animate) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;

      Initialize();
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate)
      : this(name, Shape.Polygon, fillColor, lineColor, lineWidth, time0, time1, keep, animate) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;

      Initialize();
    }

    private Animation(string name, Shape type, string fillColor, string lineColor, int lineWidth, DateTime time0, DateTime time1, bool keep, bool animate) {
      Name = name;
      Type = type;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = LineWidth;
      Time0 = time0;
      Time1 = time1;
      Keep = keep;
      this.animate = animate;
    }
    #endregion

    private void Initialize() {
      
    }

    #region Update
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Rectangle'");
      } else {
        Update();
      }
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.Ellipse) {
        throw new ArgumentException("This animation is not of type 'Ellipse'");
      } else {
        Update();
      }
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (Type != Shape.Polygon) {
        throw new ArgumentException("This animation is not of type 'Polygon'");
      } else {
        Update();
      }
    }

    private void Update() {

    }
    #endregion

    public IEnumerator<string> FramesFromTo(DateTime start, DateTime stop) {
      
    }
  }
}