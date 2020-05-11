using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Pull {
  public class RectAnimation : FramesProvider {
    public string Name { get; set; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<RectAnimationProps> propsList;
    private bool currVisible;

    public RectAnimation (string name, AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visible, Simulation env) {
      Name = Regex.Replace(name, @"\s+", "");
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<RectAnimationProps>();
      this.currVisible = false;

      RectAnimationProps props = new RectAnimationProps(x, y, width, height, fill, stroke, strokeWidth, visible);
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

    public AnimationAttribute<string> GetFill() {
      return propsList[propsList.Count - 1].Fill;
    }

    public AnimationAttribute<string> GetStroke() {
      return propsList[propsList.Count - 1].Stroke;
    }

    public AnimationAttribute<int> GetStrokeWidth() {
      return propsList[propsList.Count - 1].StrokeWidth;
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

    public void SetFillColor(AnimationAttribute<string> fill) {
      propsList[propsList.Count - 1].Fill = fill;
    }

    public void SetLineColor(AnimationAttribute<string> stroke) {
      propsList[propsList.Count - 1].Stroke = stroke;
    }

    public void SetLineWidth(AnimationAttribute<int> strokeWidth) {
      propsList[propsList.Count - 1].StrokeWidth = strokeWidth;
    }

    public void SetVisible(AnimationAttribute<bool> visible) {
      propsList[propsList.Count - 1].Visible = visible;
    }
    #endregion

    private RectAnimationProps GetLastWrittenProps() {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        RectAnimationProps props = propsList[j];
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

    private string GetValueInitFrame(RectAnimationProps props) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      RectAnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null && props.Visible.Value) {
        writer.WritePropertyName("type");
        writer.WriteValue("rect");

        writer.WritePropertyName("fill");
        writer.WriteValue(props.Fill.Value);

        writer.WritePropertyName("stroke");
        writer.WriteValue(props.Stroke.Value);

        writer.WritePropertyName("strokeWidth");
        writer.WriteValue(props.StrokeWidth.Value);

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
        if (prevWritten.Fill.CurrValue != props.Fill.Value) {
          writer.WritePropertyName("fill");
          writer.WriteValue(props.Fill.Value);
        }

        if (prevWritten.Stroke.CurrValue != props.Stroke.Value) {
          writer.WritePropertyName("stroke");
          writer.WriteValue(props.Stroke.Value);
        }

        if (prevWritten.StrokeWidth.CurrValue != props.StrokeWidth.Value) {
          writer.WritePropertyName("strokeWidth");
          writer.WriteValue(props.StrokeWidth.Value);
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
      RectAnimationProps props = propsList[propsList.Count - 1];
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

        RectAnimationProps prevWritten = GetLastWrittenProps();
        if (prevWritten == null) {
          init = true;
          prevFillColor = null;
          prevLineColor = null;
          prevLineWidth = default;
          prevTransformation = null;
        } else {
          init = false;
          prevFillColor = prevWritten.Fill.CurrValue;
          prevLineColor = prevWritten.Stroke.CurrValue;
          prevLineWidth = prevWritten.StrokeWidth.CurrValue;
          prevTransformation = new int[] { prevWritten.X.CurrValue, prevWritten.Y.CurrValue, prevWritten.Width.CurrValue, prevWritten.Height.CurrValue };
        }

        for (int i = start; i <= stop; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          bool visible = props.Visible.GetValueAt(i);
          if (visible) {
            if (init) {
              writer.WritePropertyName("type");
              writer.WriteValue("rect");

              string fill = props.Fill.GetValueAt(i);
              writer.WritePropertyName("fill");
              writer.WriteValue(fill);
              props.Fill.CurrValue = fill;
              prevFillColor = fill;

              string stroke = props.Stroke.GetValueAt(i);
              writer.WritePropertyName("stroke");
              writer.WriteValue(stroke);
              props.Stroke.CurrValue = stroke;
              prevLineColor = stroke;

              int strokeWidth = props.StrokeWidth.GetValueAt(i);
              writer.WritePropertyName("strokeWidth");
              writer.WriteValue(strokeWidth);
              props.StrokeWidth.CurrValue = strokeWidth;
              prevLineWidth = strokeWidth;

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
              string fill = props.Fill.GetValueAt(i);
              if (prevFillColor != fill) {
                writer.WritePropertyName("fill");
                writer.WriteValue(fill);
                prevFillColor = fill;
              }
              props.Fill.CurrValue = fill;

              string stroke = props.Stroke.GetValueAt(i);
              if (prevLineColor != stroke) {
                writer.WritePropertyName("stroke");
                writer.WriteValue(stroke);
                prevLineColor = stroke;
              }
              props.Stroke.CurrValue = stroke;

              int strokeWidth = props.StrokeWidth.GetValueAt(i);
              if (prevLineWidth != strokeWidth) {
                writer.WritePropertyName("strokeWidth");
                writer.WriteValue(strokeWidth);
                prevLineWidth = strokeWidth;
              }
              props.StrokeWidth.CurrValue = strokeWidth;

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
