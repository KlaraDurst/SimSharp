using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Advanced.AdvancedShapes;

namespace SimSharp.Visualization.Advanced {
  public class AdvancedAnimation : FramesProvider {
    public string Name { get; set; }

    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<AdvancedAnimationProps> propsList;
    private bool currVisible;

    private string typeStr;
    private string removeStr;

    public AdvancedAnimation (string name, AdvancedShape shape, AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<bool> visibility) {
      Name = Regex.Replace(name, @"\s+", "");
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<AdvancedAnimationProps>();
      this.currVisible = false;

      AdvancedAnimationProps props = new AdvancedAnimationProps(shape, fill, stroke, strokeWidth, visibility);
      propsList.Add(props);

      this.typeStr = props.Shape.GetType().Name.ToLower();
      this.removeStr = "advanced";
    }

    #region Get animation props
    public AdvancedShape GetShape() {
      return propsList[propsList.Count - 1].Shape;
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
    public void SetShape(AdvancedShape shape) {
      CheckType(shape);
      propsList[propsList.Count - 1].Shape = shape;
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

    private void CheckType(AdvancedShape shape) {
      if (shape.GetType() != propsList[propsList.Count - 1].Shape.GetType()) {
        throw new ArgumentException("This animation is not of type " + shape.GetType());
      }
    }

    private AdvancedAnimationProps GetLastWrittenProps() {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        AdvancedAnimationProps props = propsList[j];
        if (props.Written)
          return props;
      }
      return null;
    }

    private string GetValueInitFrame(AdvancedAnimationProps props) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      AdvancedAnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null && props.Visibility.Value) {
        writer.WritePropertyName("type");
        writer.WriteValue(typeStr.Remove(typeStr.IndexOf(removeStr), removeStr.Length));

        writer.WritePropertyName("fill");
        writer.WriteValue(props.Fill.Value);

        writer.WritePropertyName("stroke");
        writer.WriteValue(props.Stroke.Value);

        writer.WritePropertyName("stroke-width");
        writer.WriteValue(props.StrokeWidth.Value);

        writer.WritePropertyName("visibility");
        writer.WriteValue(true);
        currVisible = true;

        foreach (KeyValuePair<string, int[]> attr in props.Shape.GetValueAttributes()) {
          writer.WritePropertyName(attr.Key);
          if (attr.Value.Length < 2) {
            writer.WriteValue(attr.Value[0]);
          } else {
            writer.WriteStartArray();
            foreach (int val in attr.Value) {
              writer.WriteValue(val);
            }
            writer.WriteEndArray();
          }
        }

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
          writer.WritePropertyName("stroke-width");
          writer.WriteValue(props.StrokeWidth.Value);
        }

        if (!currVisible) {
          writer.WritePropertyName("visibility");
          writer.WriteValue(true);
          currVisible = true;
        }

        Dictionary<string, int[]> valueAttributes = props.Shape.GetValueAttributes();
        Dictionary<string, int[]> currValueAttributes = prevWritten.Shape.GetCurrValueAttributes();

        foreach (KeyValuePair<string, int[]> attr in valueAttributes) {
          currValueAttributes.TryGetValue(attr.Key, out int[] currValue);
          if (!props.Shape.CompareAttributeValues(currValue, attr.Value)) {
            writer.WritePropertyName(attr.Key);
            if (attr.Value.Length < 2) {
              writer.WriteValue(attr.Value[0]);
            } else {
              writer.WriteStartArray();
              foreach (int val in attr.Value) {
                writer.WriteValue(val);
              }
              writer.WriteEndArray();
            }
          }
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
      AdvancedAnimationProps props = propsList[propsList.Count - 1];
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
        Dictionary<string, int[]> prevAttributes;

        AdvancedAnimationProps prevWritten = GetLastWrittenProps();
        if (prevWritten == null) {
          init = true;
          prevFill = null;
          prevStroke = null;
          prevStrokeWidth = default;
          prevAttributes = new Dictionary<string, int[]>();
        } else {
          init = false;
          prevFill = prevWritten.Fill.CurrValue;
          prevStroke = prevWritten.Stroke.CurrValue;
          prevStrokeWidth = prevWritten.StrokeWidth.CurrValue;
          prevAttributes = prevWritten.Shape.GetCurrValueAttributes();
        }

        for (int i = start; i <= stop; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          bool visibility = props.Visibility.GetValueAt(i);
          if (visibility) {
            if (init) {
              writer.WritePropertyName("type");
              writer.WriteValue(typeStr.Remove(typeStr.IndexOf(removeStr), removeStr.Length));

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
              writer.WritePropertyName("stroke-width");
              writer.WriteValue(strokeWidth);
              props.StrokeWidth.CurrValue = strokeWidth;
              prevStrokeWidth = strokeWidth;

              writer.WritePropertyName("visibility");
              writer.WriteValue(true);
              props.Visibility.CurrValue = true;
              currVisible = true;

              foreach (KeyValuePair<string, int[]> attr in props.Shape.GetValueAttributesAt(i)) {
                int[] valArr = attr.Value;

                writer.WritePropertyName(attr.Key);
                if (valArr.Length < 2) {
                  writer.WriteValue(valArr[0]);
                } else {
                  writer.WriteStartArray();
                  foreach (int val in valArr) {
                    writer.WriteValue(val);
                  }
                  writer.WriteEndArray();
                }

                prevAttributes.Add(attr.Key, valArr);
              }
              props.Shape.SetCurrValueAttributes(prevAttributes);

              init = false;
              props.Written = true;
            } else {
              Dictionary<string, int[]> currValues = new Dictionary<string, int[]>();

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
                writer.WritePropertyName("stroke-width");
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

              foreach (KeyValuePair<string, int[]> attr in props.Shape.GetValueAttributesAt(i)) {
                int[] valArr = attr.Value;
                prevAttributes.TryGetValue(attr.Key, out int[] prevValue);
                
                if (!props.Shape.CompareAttributeValues(prevValue, valArr)) {
                  writer.WritePropertyName(attr.Key);
                  if (valArr.Length < 2) {
                    writer.WriteValue(valArr[0]);
                  } else {
                    writer.WriteStartArray();
                    foreach (int val in valArr) {
                      writer.WriteValue(val);
                    }
                    writer.WriteEndArray();
                  }
                  prevAttributes[attr.Key] = valArr;
                }
                currValues.Add(attr.Key, valArr);
              }
              props.Shape.SetCurrValueAttributes(currValues);

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

    public string GetName() {
      return Name;
    }
  }
}
