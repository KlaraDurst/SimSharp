using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Pull {
  public class RectangleAnimation : FramesProvider {
    public string Name { get; set; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<RectangleAnimationProps> propsList;
    private bool currVisible;

    public RectangleAnimation (string name, AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fillColor, AnimationAttribute<string> lineColor, AnimationAttribute<int> lineWidth, AnimationAttribute<bool> visible, Simulation env) {
      Name = Regex.Replace(name, @"\s+", "");
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<RectangleAnimationProps>();
      this.currVisible = false;

      RectangleAnimationProps props = new RectangleAnimationProps(x, y, width, height, fillColor, lineColor, lineWidth, visible);
      propsList.Add(props);
    }

    #region Get animation props
    public AnimationAttribute<int> GetX() {
      return propsList[propsList.Count - 1].X;
    }

    public AnimationAttribute<int> GetY() {
      return propsList[propsList.Count - 1].Y;
    }

    public AnimationAttribute<int> GetWidth() {
      return propsList[propsList.Count - 1].Width;
    }

    public AnimationAttribute<int> GetHeight() {
      return propsList[propsList.Count - 1].Height;
    }

    public AnimationAttribute<string> GetFillColor() {
      return propsList[propsList.Count - 1].FillColor;
    }

    public AnimationAttribute<string> GetLineColor() {
      return propsList[propsList.Count - 1].LineColor;
    }

    public AnimationAttribute<int> GetLineWidth() {
      return propsList[propsList.Count - 1].LineWidth;
    }

    public AnimationAttribute<bool> GetVisible() {
      return propsList[propsList.Count - 1].Visible;
    }
    #endregion

    #region Set animation props
    public void SetX(AnimationAttribute<int> x) {
      propsList[propsList.Count - 1].X = x;
    }

    public void SetY(AnimationAttribute<int> y) {
      propsList[propsList.Count - 1].Y = y;
    }

    public void SetWidth(AnimationAttribute<int> width) {
      propsList[propsList.Count - 1].Width = width;
    }

    public void SetHeight(AnimationAttribute<int> height) {
      propsList[propsList.Count - 1].Height = height;
    }

    public void SetFillColor(AnimationAttribute<string> fillColor) {
      propsList[propsList.Count - 1].FillColor = fillColor;
    }

    public void SetLineColor(AnimationAttribute<string> lineColor) {
      propsList[propsList.Count - 1].LineColor = lineColor;
    }

    public void SetLineWidth(AnimationAttribute<int> lineWidth) {
      propsList[propsList.Count - 1].LineWidth = lineWidth;
    }

    public void SetVisible(AnimationAttribute<bool> visible) {
      propsList[propsList.Count - 1].Visible = visible;
    }
    #endregion

    private RectangleAnimationProps GetLastWrittenProps() {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        RectangleAnimationProps props = propsList[j];
        if (props.Written)
          return props;
      }
      return null;
    }

    private bool AllEqual<T>(T[] a, T[] b) {
      if (b == null)
        return true;

      if (a.Length != b.Length)
        return false;

      for (int i = 0; i < a.Length; i++) {
        if (!a[i].Equals(b[i]))
          return false;
      }
      return true;
    }

    private string GetValueInitFrame(RectangleAnimationProps props) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      RectangleAnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null && props.Visible.Value) {
        writer.WritePropertyName("type");
        writer.WriteValue("rectangle");

        writer.WritePropertyName("fillColor");
        writer.WriteValue(props.FillColor.Value);

        writer.WritePropertyName("lineColor");
        writer.WriteValue(props.LineColor.Value);

        writer.WritePropertyName("lineWidth");
        writer.WriteValue(props.LineWidth.Value);

        writer.WritePropertyName("visible");
        writer.WriteValue(true);
        currVisible = true;

        writer.WritePropertyName("t");
        writer.WriteStartArray();
        foreach (int t in new int[] { props.X.Value, props.Y.Value, props.Width.Value, props.Height.Value }) {
          writer.WriteValue(t);
        }
        writer.WriteEndArray();

        props.Written = true;
      } else if (prevWritten != null && props.Visible.Value) {
        if (prevWritten.FillColor.CurrValue != props.FillColor.Value) {
          writer.WritePropertyName("fillColor");
          writer.WriteValue(props.FillColor.Value);
        }

        if (prevWritten.LineColor.CurrValue != props.LineColor.Value) {
          writer.WritePropertyName("lineColor");
          writer.WriteValue(props.LineColor.Value);
        }

        if (prevWritten.LineWidth.CurrValue != props.LineWidth.Value) {
          writer.WritePropertyName("lineWidth");
          writer.WriteValue(props.LineWidth.Value);
        }

        if (!currVisible) {
          writer.WritePropertyName("visible");
          writer.WriteValue(true);
          currVisible = true;
        }

        int[] transformation = new int[] { props.X.Value, props.Y.Value, props.Width.Value, props.Height.Value };
        int[] prevTransformation = new int[] { prevWritten.X.Value, prevWritten.Y.Value, prevWritten.Width.Value, prevWritten.Height.Value };

        if (!AllEqual(transformation, prevTransformation)) {
          writer.WritePropertyName("t");
          writer.WriteStartArray();
          foreach (int t in transformation) {
            writer.WriteValue(t);
          }
          writer.WriteEndArray();
        }

        props.Written = true;
      } else if (prevWritten != null && !props.Visible.Value) {
        if (currVisible) {
          writer.WritePropertyName("visible");
          writer.WriteValue(false);
          currVisible = false;
        }
      }

      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

      if (frame.Length <= Name.Length + 5) // json object is empty
        return string.Empty;
      else
        return frame;
    }

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    public List<AnimationUnit> FramesFromTo(int start, int stop) {
      RectangleAnimationProps props = propsList[propsList.Count - 1];
      List<AnimationUnit> affectedUnits = new List<AnimationUnit>();

      if (props.AllValues()) {
        string frame = GetValueInitFrame(props);
        if (!frame.Equals(string.Empty)) {
          AnimationUnit unit = new AnimationUnit(start, start, 1);
          unit.AddFrame(frame);
          affectedUnits.Add(unit);
        }
        return affectedUnits;
      } else {
        List<string> frames = new List<string>();
        int unitStart = start;
        bool init;
        int[] prevTransformation;
        string prevFillColor;
        string prevLineColor;
        int prevLineWidth;

        RectangleAnimationProps prevWritten = GetLastWrittenProps();
        if (prevWritten == null) {
          init = true;
          prevFillColor = null;
          prevLineColor = null;
          prevLineWidth = default;
          prevTransformation = null;
        } else {
          init = false;
          prevFillColor = prevWritten.FillColor.CurrValue;
          prevLineColor = prevWritten.LineColor.CurrValue;
          prevLineWidth = prevWritten.LineWidth.CurrValue;
          prevTransformation = new int[] { prevWritten.X.CurrValue, prevWritten.Y.CurrValue, prevWritten.Width.CurrValue, prevWritten.Height.CurrValue };
        }

        for (int i = start; i <= stop; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          bool visible = props.Visible.GetValueAt(i);
          if (visible) {
            if (init) {
              writer.WritePropertyName("type");
              writer.WriteValue("rectangle");

              string fillColor = props.FillColor.GetValueAt(i);
              writer.WritePropertyName("fillColor");
              writer.WriteValue(fillColor);
              props.FillColor.CurrValue = fillColor;
              prevFillColor = fillColor;

              string lineColor = props.LineColor.GetValueAt(i);
              writer.WritePropertyName("lineColor");
              writer.WriteValue(lineColor);
              props.LineColor.CurrValue = lineColor;
              prevLineColor = lineColor;

              int lineWidth = props.LineWidth.GetValueAt(i);
              writer.WritePropertyName("lineWidth");
              writer.WriteValue(lineWidth);
              props.LineWidth.CurrValue = lineWidth;
              prevLineWidth = lineWidth;

              writer.WritePropertyName("visible");
              writer.WriteValue(true);
              props.Visible.CurrValue = true;
              currVisible = true;

              int[] transformation = new int[] { props.X.GetValueAt(i), props.Y.GetValueAt(i), props.Width.GetValueAt(i), props.Height.GetValueAt(i) };
              writer.WritePropertyName("t");
              writer.WriteStartArray();
              foreach (int t in transformation) {
                writer.WriteValue(t);
              }
              props.X.CurrValue = transformation[0];
              props.Y.CurrValue = transformation[1];
              props.Width.CurrValue = transformation[2];
              props.Height.CurrValue = transformation[3];
              prevTransformation = transformation;

              writer.WriteEndArray();

              init = false;
              props.Written = true;
            } else {
              string fillColor = props.FillColor.GetValueAt(i);
              if (prevFillColor != fillColor) {
                writer.WritePropertyName("fillColor");
                writer.WriteValue(fillColor);
                prevFillColor = fillColor;
              }
              props.FillColor.CurrValue = fillColor;

              string lineColor = props.LineColor.GetValueAt(i);
              if (prevLineColor != lineColor) {
                writer.WritePropertyName("lineColor");
                writer.WriteValue(lineColor);
                prevLineColor = lineColor;
              }
              props.LineColor.CurrValue = lineColor;

              int lineWidth = props.LineWidth.GetValueAt(i);
              if (prevLineWidth != lineWidth) {
                writer.WritePropertyName("lineWidth");
                writer.WriteValue(lineWidth);
                prevLineWidth = lineWidth;
              }
              props.LineWidth.CurrValue = lineWidth;

              if (!currVisible) {
                writer.WritePropertyName("visible");
                writer.WriteValue(true);
                currVisible = true;
              }
              props.Visible.CurrValue = visible;

              int[] transformation = new int[] { props.X.GetValueAt(i), props.Y.GetValueAt(i), props.Width.GetValueAt(i), props.Height.GetValueAt(i) };
              if (!AllEqual(transformation, prevTransformation)) {
                writer.WritePropertyName("t");
                writer.WriteStartArray();
                foreach (int t in transformation) {
                  writer.WriteValue(t);
                }
                prevTransformation = transformation;
                writer.WriteEndArray();
              }
              props.X.CurrValue = transformation[0];
              props.Y.CurrValue = transformation[1];
              props.Width.CurrValue = transformation[2];
              props.Height.CurrValue = transformation[3];

              props.Written = true;
            }
          } else {
            if (currVisible) {
              writer.WritePropertyName("visible");
              writer.WriteValue(false);
              props.Visible.CurrValue = false;
              currVisible = false;
            }
          }

          writer.WriteEndObject();
          string frame = stringWriter.ToString();
          Flush();

          if (frame.Length <= Name.Length + 5) { // json object is empty
            int unitStop = unitStart + frames.Count;

            if (frames.Count > 0) {
              AnimationUnit unit = new AnimationUnit(unitStart, unitStop, frames.Count);
              unit.AddFrameRange(frames);
              affectedUnits.Add(unit);
              frames = new List<string>();
            }

            unitStart = unitStop + 1;
          } else {
            frames.Add(frame);
          }
        }
        if (frames.Count > 0) {
          int unitStop = unitStart + frames.Count;
          AnimationUnit unit = new AnimationUnit(unitStart, unitStop, frames.Count);
          unit.AddFrameRange(frames);
          affectedUnits.Add(unit);
        }
        return affectedUnits;
      }
    }
  }
}
