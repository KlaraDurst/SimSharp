using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Pull.Attributes;

namespace SimSharp.Visualization.Pull {
  public class RectangleAnimation : FramesProvider {
    public string Name { get; set; }
    public IntAttribute X { get; set; }
    public IntAttribute Y { get; set; }
    public IntAttribute Width { get; set; }
    public IntAttribute Height { get; set; }
    public StringAttribute FillColor { get; set; }
    public StringAttribute LineColor { get; set; }
    public IntAttribute LineWidth { get; set; }
    public BoolAttribute Visible { get; set; }

    private Simulation env;

    #region Constructors
    public RectangleAnimation (string name, IntAttribute x, IntAttribute y, IntAttribute width, IntAttribute height, StringAttribute fillColor, StringAttribute lineColor, IntAttribute lineWidth, BoolAttribute visible, Simulation env) {
      Name = name;
      X = x;
      Y = y;
      Width = width;
      Height = height;
      FillColor = fillColor;
      LineColor = lineColor;
      LineWidth = lineWidth;
      Visible = visible;
      this.env = env;
    }
    #endregion

    public List<AnimationUnit> FramesFromTo(DateTime start, DateTime stop) {
      throw new NotImplementedException();
    }
  }
}
