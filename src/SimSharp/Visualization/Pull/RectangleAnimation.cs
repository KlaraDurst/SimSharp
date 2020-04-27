using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Pull {
  public class RectangleAnimation : FramesProvider {
    public string Name { get; set; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<RectangleAnimationProps> propsList;

    public RectangleAnimation (string name, AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fillColor, AnimationAttribute<string> lineColor, AnimationAttribute<int> lineWidth, AnimationAttribute<bool> visible, Simulation env) {
      Name = name;
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<RectangleAnimationProps>();

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
        writer.WriteValue(props.Visible.Value);

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

        if (prevWritten.Visible.CurrValue != props.Visible.Value) {
          writer.WritePropertyName("visible");
          writer.WriteValue(props.Visible.Value);
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
        if (prevWritten.Visible.CurrValue != props.Visible.Value) {
          writer.WritePropertyName("visible");
          writer.WriteValue(props.Visible.Value);
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

    private IEnumerator<string> GetEnum<T>(AnimationAttribute<T> attr, string name, bool isFirst, T last, int start, int stop) {
      if (attr.Function == null) {
        if (isFirst || !attr.Value.Equals(last)) {
          writer.WritePropertyName(name);
          writer.WriteValue(attr.Value);

          string attrStr = stringWriter.ToString();
          Flush();

          return new List<string>(1) { attrStr }.GetEnumerator();
        }
        return null;
      } else {
        return GetEnumWithFunction(attr, name, isFirst, last, start, stop);
      }
    }

    private IEnumerator<string> GetEnumWithFunction<T>(AnimationAttribute<T> attr, string name, bool isFirst, T last, int start, int stop) {
      T prev = last;

      // first frame
      T curr = attr.Function(start);
      if (isFirst || !curr.Equals(prev)) {
        writer.WritePropertyName(name);
        writer.WriteValue(curr);
        attr.CurrValue = curr;

        string attrStr = stringWriter.ToString();
        Flush();
        prev = curr;

        yield return attrStr;
      } else {
        yield return string.Empty;
      }

      // rest of the frames
      for (int i = start + 1; i <= stop; i++) {
        curr = attr.Function(i);
        if (!curr.Equals(prev)) {
          writer.WritePropertyName(name);
          writer.WriteValue(curr);
          attr.CurrValue = curr;

          string attrStr = stringWriter.ToString();
          Flush();
          prev = curr;

          yield return attrStr;
        } else {
          yield return string.Empty;
        }
      }
    }

    private IEnumerator<string> GetTransformationEnum<T>(AnimationAttribute<T>[] attr, bool isFirst, T[] last, int start, int stop) {
      bool AllValues() {
        for (int i = 0; i < attr.Length; i++) {
          if (attr[i].Function != null)
            return false;
        }
        return true;
      }

      if (AllValues()) {
        List<T> attrValues = new List<T>(attr.Length);
        for (int i = 0; i < attr.Length; i++) {
          attrValues.Add(attr[i].Value);
        }

        if (isFirst || !AllEqual(attrValues.ToArray(), last)) {
          writer.WritePropertyName("t");
          writer.WriteStartArray();
          for (int i = 0; i < attr.Length; i++) {
            writer.WriteValue(attr[i].Value);
          }
          writer.WriteEndArray();

          string attrStr = stringWriter.ToString();
          Flush();

          return new List<string>(1) { attrStr }.GetEnumerator();
        }
        return null;
      } else {
        return GetTransformationEnumWithFunction(attr, isFirst, last, start, stop);
      }
    }

    private IEnumerator<string> GetTransformationEnumWithFunction<T>(AnimationAttribute<T>[] attr, bool isFirst, T[] last, int start, int stop) {
      T[] prev = last;

      // first frame
      List<T> currList = new List<T>(attr.Length);
      for (int j = 0; j < attr.Length; j++) {
        currList.Add(attr[j].GetValueAt(start));
      }
      T[] currArray = currList.ToArray();

      if (isFirst || !AllEqual(currArray, prev)) {
        writer.WritePropertyName("t");
        writer.WriteStartArray();
        for (int z = 0; z < attr.Length; z++) {
          writer.WriteValue(currArray[z]);
          attr[z].CurrValue = currArray[z];
        }
        writer.WriteEndArray();

        string attrStr = stringWriter.ToString();
        Flush();
        prev = currArray;

        yield return attrStr;
      } else {
        yield return string.Empty;
      }

      // rest of the frames
      for (int i = start + 1; i <= stop; i++) {
        currList = new List<T>(attr.Length);
        for (int j = 0; j < attr.Length; j++) {
          currList.Add(attr[i].GetValueAt(i));
        }
        currArray = currList.ToArray();

        if (!AllEqual(currArray, prev)) {
          writer.WritePropertyName("t");
          writer.WriteStartArray();
          for (int z = 0; z < attr.Length; z++) {
            writer.WriteValue(currArray[z]);
            attr[z].CurrValue = currArray[z];
          }
          writer.WriteEndArray();

          string attrStr = stringWriter.ToString();
          Flush();
          prev = currArray;

          yield return attrStr;
        } else {
          yield return string.Empty;
        }
      }
    }

    private List<IEnumerator<string>> GetAttrEnums(RectangleAnimationProps props, int start, int stop) {
      bool isFirst;
      int[] prevTrans;
      string prevFillColor;
      string prevLineColor;
      int prevLineWidth;
      bool prevVisible;

      RectangleAnimationProps prevWritten = GetLastWrittenProps();
      if (prevWritten == null) {
        isFirst = true;
        prevTrans = null;
        prevFillColor = null;
        prevLineColor = null;
        prevLineWidth = default;
        prevVisible = default;
      } else {
        isFirst = false;
        prevTrans = new int[] { prevWritten.X.CurrValue, prevWritten.Y.CurrValue, prevWritten.Width.CurrValue, prevWritten.Height.CurrValue };
        prevFillColor = prevWritten.FillColor.CurrValue;
        prevLineColor = prevWritten.LineColor.CurrValue;
        prevLineWidth = prevWritten.LineWidth.CurrValue;
        prevVisible = prevWritten.Visible.CurrValue;
      }
      List<IEnumerator<string>> enums = new List<IEnumerator<string>>(8);

      IEnumerator<string> transformationEnum = GetTransformationEnum(new AnimationAttribute<int>[] { props.X, props.Y, props.Width, props.Height }, isFirst, prevTrans, start, stop);
      if (transformationEnum != null)
        enums.Add(transformationEnum);
      IEnumerator<string> fillColorEnum = GetEnum(props.FillColor, "fillColor", isFirst, prevFillColor, start, stop);
      if (fillColorEnum != null)
        enums.Add(fillColorEnum);
      IEnumerator<string> lineColorEnum = GetEnum(props.LineColor, "lineColor", isFirst, prevLineColor, start, stop);
      if (lineColorEnum != null)
        enums.Add(lineColorEnum);
      IEnumerator<string> lineWidthEnum = GetEnum(props.LineWidth, "lineWidth", isFirst, prevLineWidth, start, stop);
      if (lineWidthEnum != null)
        enums.Add(lineWidthEnum);
      IEnumerator<string> visibleEnum = GetEnum(props.Visible, "visible", isFirst, prevVisible, start, stop);
      if (visibleEnum != null)
        enums.Add(visibleEnum);

      return enums;
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
        int frameNumber = stop - start + 1;
        AnimationUnit unit = new AnimationUnit(start, stop, frameNumber);

        List<IEnumerator<string>> attrEnums = GetAttrEnums(props, start, stop);

        for (int i = 0; i < frameNumber; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();
          bool first = true;

          for (int j = attrEnums.Count - 1; j >= 0; j--) {
            attrEnums[j].MoveNext();
            string attr = attrEnums[j].Current;

            if (!first)
              writer.WriteRaw(",");
            writer.WriteRaw(attr);

            first = false;
          }
          writer.WriteEndObject();
          unit.AddFrame(stringWriter.ToString());
          Flush();
        }

        RectangleAnimationProps newProps = new RectangleAnimationProps(props);
        propsList.Add(newProps);
        affectedUnits.Add(unit);
        return affectedUnits;
      }
    }
  }
}
