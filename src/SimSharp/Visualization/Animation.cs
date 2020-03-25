using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Frames;
using SimSharp.Visualization.Shapes;

namespace SimSharp.Visualization {
  public class Animation {
    public Rectangle Rectangle0 { get; }
    public Rectangle Rectangle1 { get; }

    public Ellipse Ellipse0 { get; }
    public Ellipse Ellipse1 { get; }

    public Polygon Polygon0 { get; }
    public Polygon Polygon1 { get; }

    private DateTime time0;
    private DateTime time1;
    private bool keep;
    private bool animate;
    private Frame currState;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate) : this(time0, time1, keep, animate) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;

      List<int> props = new List<int>(4) {
        Rectangle0.X,
        Rectangle0.Y,
        Rectangle0.Width,
        Rectangle0.Height
      };

      currState = new Frame(name, true, FrameProperties.Shape.Rectangle, fillColor, lineColor, lineWidth, props);
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate) : this(time0, time1, keep, animate) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;

      List<int> props = new List<int>(4) {
        Ellipse0.X,
        Ellipse0.Y,
        Ellipse0.Radius1,
        Ellipse0.Radius2
      };

      currState = new Frame(name, true, FrameProperties.Shape.Ellipse, fillColor, lineColor, lineWidth, props);
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep, bool animate) : this(time0, time1, keep, animate) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;

      List<int> props = new List<int>(Polygon0.XYList.Count);
      props.AddRange(Polygon0.XYList);

      currState = new Frame(name, true, FrameProperties.Shape.Polygon, fillColor, lineColor, lineWidth, props);
    }

    private Animation(DateTime time0, DateTime time1, bool keep, bool animate) {
      this.time0 = time0;
      this.time1 = time1;
      this.keep = keep;
      this.animate = animate;
    }
    #endregion

    #region Update
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (currState.Props.Type != FrameProperties.Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Rectangle'");
      } else {

      }
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (currState.Props.Type != FrameProperties.Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Ellipse'");
      } else {

      }
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (currState.Props.Type != FrameProperties.Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Polygon'");
      } else {

      }
    }
    #endregion

    public IEnumerator<Frame> StatesUntil(int frameNumber) {

    }
  }
}