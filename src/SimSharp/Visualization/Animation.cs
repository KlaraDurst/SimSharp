using System;
using System.Collections.Generic;
using System.Text;
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

    private FrameObjekt currState;

    #region Constructors
    public Animation(string name, Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) : this(time0, time1, keep) {
      Rectangle0 = rectangle0;
      Rectangle1 = rectangle1;

      List<int> props = new List<int>(4) {
        Rectangle0.X,
        Rectangle0.Y,
        Rectangle0.Width,
        Rectangle0.Height
      };

      currState = new FrameObjekt(name, FrameObjekt.Shape.Rectangle, fillColor, lineColor, lineWidth, true, props);
    }

    public Animation(string name, Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) : this(time0, time1, keep) {
      Ellipse0 = ellipse0;
      Ellipse1 = ellipse1;

      List<int> props = new List<int>(4) {
        Ellipse0.X,
        Ellipse0.Y,
        Ellipse0.Radius1,
        Ellipse0.Radius2
      };

      currState = new FrameObjekt(name, FrameObjekt.Shape.Ellipse, fillColor, lineColor, lineWidth, true, props);
    }

    public Animation(string name, Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) : this(time0, time1, keep) {
      Polygon0 = polygon0;
      Polygon1 = polygon1;

      List<int> props = new List<int>(Polygon0.XYList.Count);
      props.AddRange(Polygon0.XYList);

      currState = new FrameObjekt(name, FrameObjekt.Shape.Polygon, fillColor, lineColor, lineWidth, true, props);
    }

    private Animation(DateTime time0, DateTime time1, bool keep) {
      this.time0 = time0;
      this.time1 = time1;
      this.keep = keep;
    }
    #endregion

    #region Update
    public void Update(Rectangle rectangle0, Rectangle rectangle1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (currState.Type != FrameObjekt.Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Rectangle'");
      } else {

      }
    }

    public void Update(Ellipse ellipse0, Ellipse ellipse1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (currState.Type != FrameObjekt.Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Ellipse'");
      } else {

      }
    }

    public void Update(Polygon polygon0, Polygon polygon1, DateTime time0, DateTime time1, string fillColor, string lineColor, int lineWidth, bool keep) {
      if (currState.Type != FrameObjekt.Shape.Rectangle) {
        throw new ArgumentException("This animation is not of type 'Polygon'");
      } else {

      }
    }
    #endregion
  }
}