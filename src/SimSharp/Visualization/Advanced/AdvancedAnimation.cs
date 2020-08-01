using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SimSharp.Visualization.Advanced.AdvancedShapes;
using SimSharp.Visualization.Advanced.AdvancedStyles;

namespace SimSharp.Visualization.Advanced {
  public class AdvancedAnimation : FramesProvider {
    public string Name { get; set; }

    protected StringWriter stringWriter;
    protected JsonTextWriter writer;
    protected List<AdvancedAnimationProperties> propsList;
    protected bool currVisible;

    public AdvancedAnimation (string name, AdvancedShape shape, AdvancedStyle style, AnimationAttribute<bool> visibility, bool isChild = false) {
      Name = Regex.Replace(name, @"\s+", "");
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<AdvancedAnimationProperties>();
      this.currVisible = false;

      AdvancedAnimationProperties props = new AdvancedAnimationProperties(shape, style, visibility, isChild);
      propsList.Add(props);
    }

    #region Get animation props
    public AdvancedShape GetShape() {
      return propsList[propsList.Count - 1].Shape;
    }

    public AdvancedStyle GetStyle() {
      return propsList[propsList.Count - 1].Style;
    }

    public AnimationAttribute<bool> GetVisibility() {
      return propsList[propsList.Count - 1].Visibility;
    }
    #endregion

    #region Set animation props
    public void SetShape(AdvancedShape shape) {
      CheckType(shape);
      propsList[propsList.Count - 1].Shape = shape;
      propsList[propsList.Count - 1].Written = false;
    }

    public void SetStyle(AdvancedStyle style) {
      propsList[propsList.Count - 1].Style = style;
      propsList[propsList.Count - 1].Written = false;
    }

    public void SetVisibility(AnimationAttribute<bool> visibility) {
      propsList[propsList.Count - 1].Visibility = visibility;
      propsList[propsList.Count - 1].Written = false;
    }
    #endregion

    private void CheckType(AdvancedShape shape) {
      if (shape.GetType() != propsList[propsList.Count - 1].Shape.GetType()) {
        throw new ArgumentException("This animation is not of type " + shape.GetType());
      }
    }

    protected AdvancedAnimationProperties GetLastWrittenProps() {
      for (int j = propsList.Count - 1; j >= 0; j--) {
        AdvancedAnimationProperties props = propsList[j];
        if (props.Written)
          return props;
      }
      return null;
    }

    protected virtual void WriteValueJson(AdvancedAnimationProperties props, AdvancedAnimationProperties prevWritten) {
      props.WriteValueJson(writer, currVisible, prevWritten);
    }

    public virtual string GetValueInitFrame() {
      AdvancedAnimationProperties props = propsList[propsList.Count - 1];

      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      AdvancedAnimationProperties prevWritten = GetLastWrittenProps();
      if (props.Visibility.Value) {
        WriteValueJson(props, prevWritten);
        currVisible = props.Visibility.Value;
      } else {
        if (currVisible) {
          writer.WritePropertyName("visibility");
          writer.WriteValue(false);
          currVisible = false;
        }
      }
      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

      return frame;
    }

    protected void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    public virtual bool AllValues() {
      AdvancedAnimationProperties props = propsList[propsList.Count - 1];
      return props.AllValues();
    }

    protected virtual void WriteValueAtJson(AdvancedAnimationProperties props, int i, AdvancedStyle.State propsState, Dictionary<string, int[]> prevAttributes) {
      props.WriteValueAtJson(i, writer, currVisible, propsState, prevAttributes);
    }

    public virtual List<AnimationUnit> FramesFromTo(int start, int stop) {
      AdvancedAnimationProperties props = propsList[propsList.Count - 1];
      List<AnimationUnit> affectedUnits = new List<AnimationUnit>();

      if (AllValues()) {
        string frame = GetValueInitFrame();
        if (frame.Length > Name.Length + 5) { // json object is not empty
          AnimationUnit unit = new AnimationUnit(start, start, 1);
          unit.AddFrame(frame);
          affectedUnits.Add(unit);
        }
      } else {
        List<string> frames = new List<string>();
        int unitStart = start;
        AdvancedStyle.State styleState;
        Dictionary<string, int[]> shapeState;

        AdvancedAnimationProperties prevWritten = GetLastWrittenProps();
        if (prevWritten == null) {
          styleState = null;
          shapeState = null;
        } else {
          styleState = prevWritten.Style.GetCurrValueAttributes();
          shapeState = prevWritten.Shape.GetCurrValueAttributes();
        }

        for (int i = start; i <= stop; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();

          bool visibility = props.Visibility.GetValueAt(i);
          if (visibility) {
            WriteValueAtJson(props, i, styleState, shapeState);
            styleState = props.Style.GetCurrValueAttributes();
            shapeState = props.Shape.GetCurrValueAttributes();
            currVisible = props.Visibility.CurrValue;
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
      }

      if (propsList.Count > 1)
        propsList.RemoveAt(0);

      AdvancedAnimationProperties propsNew = new AdvancedAnimationProperties(props);
      propsNew.Written = true;
      propsList.Add(propsNew);
      return affectedUnits;
    }

    public string GetName() {
      return Name;
    }
  }
}
