using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Pull {
  public class RectAnimation : FramesProvider {
    public string Name { get; set; }

    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<RectAnimationProps> propsList;
    private bool currVisible;

    public RectAnimation (string name, AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visibility) {
      Name = Regex.Replace(name, @"\s+", "");
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<RectAnimationProps>();
      this.currVisible = false;

      RectAnimationProps props = new RectAnimationProps(x, y, width, height, fill, stroke, strokeWidth, visibility);
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
      return propsList[propsList.Count - 1].Visibility;
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

    public void SetFill(AnimationAttribute<string> fill) {
      propsList[propsList.Count - 1].Fill = fill;
    }

    public void SetStroke(AnimationAttribute<string> stroke) {
      propsList[propsList.Count - 1].Stroke = stroke;
    }

    public void SetStrokeWidth(AnimationAttribute<int> strokeWidth) {
      propsList[propsList.Count - 1].StrokeWidth = strokeWidth;
    }

    public void SetVisible(AnimationAttribute<bool> visibility) {
      propsList[propsList.Count - 1].Visibility = visibility;
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

    private string GetValueInitFrame(RectAnimationProps props) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      RectAnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null && props.Visibility.Value) {
        writer.WritePropertyName("type");
        writer.WriteValue("rect");

        writer.WritePropertyName("fill");
        writer.WriteValue(props.Fill.Value);

        writer.WritePropertyName("stroke");
        writer.WriteValue(props.Stroke.Value);

        writer.WritePropertyName("strokeWidth");
        writer.WriteValue(props.StrokeWidth.Value);

        writer.WritePropertyName("visibility");
        writer.WriteValue(true);
        currVisible = true;

        writer.WritePropertyName("x");
        writer.WriteValue(props.X.Value);

        writer.WritePropertyName("y");
        writer.WriteValue(props.Y.Value);

        writer.WritePropertyName("width");
        writer.WriteValue(props.Width.Value);

        writer.WritePropertyName("height");
        writer.WriteValue(props.Height.Value);

        props.Written = true;
      } else if (prevWritten != null && props.Visibility.Value) {
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
          writer.WritePropertyName("visibility");
          writer.WriteValue(true);
          currVisible = true;
        }

        if (prevWritten.X.CurrValue != props.X.Value) {
          writer.WritePropertyName("x");
          writer.WriteValue(props.X.Value);
        }

        if (prevWritten.Y.CurrValue != props.Y.Value) {
          writer.WritePropertyName("y");
          writer.WriteValue(props.Y.Value);
        }

        if (prevWritten.Width.CurrValue != props.Width.Value) {
          writer.WritePropertyName("width");
          writer.WriteValue(props.Width.Value);
        }

        if (prevWritten.Height.CurrValue != props.Height.Value) {
          writer.WritePropertyName("height");
          writer.WriteValue(props.Height.Value);
        }

        props.Written = true;
      } else if (prevWritten != null && !props.Visibility.Value) {
        if (currVisible) {
          writer.WritePropertyName("visibility");
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
        string prevFill;
        string prevStroke;
        int prevStrokeWidth;
        int prevX;
        int prevY;
        int prevWidth;
        int prevHeight;

        RectAnimationProps prevWritten = GetLastWrittenProps();
        if (prevWritten == null) {
          init = true;
          prevFill = null;
          prevStroke = null;
          prevStrokeWidth = default;
          prevX = default;
          prevY = default;
          prevWidth = default;
          prevHeight = default;
        } else {
          init = false;
          prevFill = prevWritten.Fill.CurrValue;
          prevStroke = prevWritten.Stroke.CurrValue;
          prevStrokeWidth = prevWritten.StrokeWidth.CurrValue;
          prevX = prevWritten.X.CurrValue;
          prevY = prevWritten.Y.CurrValue;
          prevWidth = prevWritten.Width.CurrValue;
          prevHeight = prevWritten.Height.CurrValue;
        }

        for (int i = start; i <= stop; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          bool visibility = props.Visibility.GetValueAt(i);
          if (visibility) {
            if (init) {
              writer.WritePropertyName("type");
              writer.WriteValue("rect");

              string fill = props.Fill.GetValueAt(i);
              writer.WritePropertyName("fill");
              writer.WriteValue(fill);
              props.Fill.CurrValue = fill;
              prevFill = fill;

              string stroke = props.Stroke.GetValueAt(i);
              writer.WritePropertyName("stroke");
              writer.WriteValue(stroke);
              props.Stroke.CurrValue = stroke;
              prevStroke = stroke;

              int strokeWidth = props.StrokeWidth.GetValueAt(i);
              writer.WritePropertyName("strokeWidth");
              writer.WriteValue(strokeWidth);
              props.StrokeWidth.CurrValue = strokeWidth;
              prevStrokeWidth = strokeWidth;

              writer.WritePropertyName("visibility");
              writer.WriteValue(true);
              props.Visibility.CurrValue = true;
              currVisible = true;

              int x = props.X.GetValueAt(i);
              writer.WritePropertyName("x");
              writer.WriteValue(x);
              props.X.CurrValue = x;
              prevX = x;

              int y = props.Y.GetValueAt(i);
              writer.WritePropertyName("y");
              writer.WriteValue(y);
              props.Y.CurrValue = y;
              prevY = y;

              int width = props.Width.GetValueAt(i);
              writer.WritePropertyName("width");
              writer.WriteValue(width);
              props.Width.CurrValue = width;
              prevWidth = width;

              int height = props.Height.GetValueAt(i);
              writer.WritePropertyName("height");
              writer.WriteValue(height);
              props.Height.CurrValue = height;
              prevHeight = height;

              init = false;
              props.Written = true;
            } else {
              string fill = props.Fill.GetValueAt(i);
              if (prevFill != fill) {
                writer.WritePropertyName("fill");
                writer.WriteValue(fill);
                prevFill = fill;
              }
              props.Fill.CurrValue = fill;

              string stroke = props.Stroke.GetValueAt(i);
              if (prevStroke != stroke) {
                writer.WritePropertyName("stroke");
                writer.WriteValue(stroke);
                prevStroke = stroke;
              }
              props.Stroke.CurrValue = stroke;

              int strokeWidth = props.StrokeWidth.GetValueAt(i);
              if (prevStrokeWidth != strokeWidth) {
                writer.WritePropertyName("strokeWidth");
                writer.WriteValue(strokeWidth);
                prevStrokeWidth = strokeWidth;
              }
              props.StrokeWidth.CurrValue = strokeWidth;

              if (!currVisible) {
                writer.WritePropertyName("visibility");
                writer.WriteValue(true);
                currVisible = true;
              }
              props.Visibility.CurrValue = visibility;

              int x = props.X.GetValueAt(i);
              if (prevX != x) {
                writer.WritePropertyName("x");
                writer.WriteValue(x);
                prevX = x;
              }
              props.X.CurrValue = x;

              int y = props.Y.GetValueAt(i);
              if (prevY != y) {
                writer.WritePropertyName("y");
                writer.WriteValue(y);
                prevY = y;
              }
              props.Y.CurrValue = y;

              int width = props.Width.GetValueAt(i);
              if (prevWidth != width) {
                writer.WritePropertyName("width");
                writer.WriteValue(width);
                prevWidth = width;
              }
              props.Width.CurrValue = width;

              int height = props.Height.GetValueAt(i);
              if (prevHeight != height) {
                writer.WritePropertyName("height");
                writer.WriteValue(height);
                prevHeight = height;
              }
              props.Height.CurrValue = height;

              props.Written = true;
            }
          } else {
            if (currVisible) {
              writer.WritePropertyName("visibility");
              writer.WriteValue(false);
              props.Visibility.CurrValue = false;
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
