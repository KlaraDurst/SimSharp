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
    private List<AnimationUnit> units;

    public RectangleAnimation (string name, AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> width, AnimationAttribute<int> height, AnimationAttribute<string> fillColor, AnimationAttribute<string> lineColor, AnimationAttribute<int> lineWidth, AnimationAttribute<bool> visible, Simulation env) {
      Name = name;
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);
      this.propsList = new List<RectangleAnimationProps>();
      this.units = new List<AnimationUnit>();

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

    private string GetValueInitFrame(RectangleAnimationProps props) {
      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      if (propsList.Count < 2) {
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
      } else {
        RectangleAnimationProps prev = propsList[propsList.Count - 2];

        // TODO
      }

      writer.WritePropertyName("t");
      writer.WriteStartArray();
      foreach (int t in new int[] { props.X.Value, props.Y.Value, props.Width.Value, props.Height.Value }) {
        writer.WriteValue(t);
      }
      writer.WriteEndArray();

      writer.WriteEndObject();
      string frame = stringWriter.ToString();
      Flush();

      return frame;
    }

    #region GetEnum
    private IEnumerator<string> GetEnum<T>(AnimationAttribute<T> attr, string name, T last, DateTime start, DateTime stop) {
      if (attr.Function == null) {
        if (units.Count < 2 || !attr.Value.Equals(last)) {
          writer.WritePropertyName(name);
          writer.WriteValue(attr.Value);

          string attrStr = stringWriter.ToString();
          Flush();

          yield return attrStr;
        }
        yield return null;
      } else {
        T prev = last;
        int startFrameNumber = Convert.ToInt32((start - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep) + 1;
        int stopFrameNumber = Convert.ToInt32((stop - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep);

        for (int i = startFrameNumber; i <= stopFrameNumber; i++) {
          T curr = attr.Function(i);
          if (units.Count < 2 || !curr.Equals(prev)) {
            writer.WritePropertyName(name);
            writer.WriteValue(attr.Value);

            string attrStr = stringWriter.ToString();
            Flush();
            prev = curr;

            yield return attrStr;
          }
          else {
            yield return String.Empty;
          }
        }
      }
    }

    private IEnumerator<string> GetTransformationEnum<T>(AnimationAttribute<T>[] attr, string[] name, T[] last, DateTime start, DateTime stop) {
      bool AllValues() {
        for (int i = 0; i < attr.Length; i++) {
          if (attr[i].Function != null)
            return false;
        }
        return true;
      }

      bool AllEqualAt(int t, T[] comp) {
        if (comp == null)
          return true;

        for (int i = 0; i < attr.Length; i++) {
          if (!attr[i].GetValueAt(t).Equals(comp[i]))
            return false;
        }
        return true;
      }

      int startFrameNumber = Convert.ToInt32((start - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep) + 1;
      int stopFrameNumber = Convert.ToInt32((stop - env.StartDate).TotalSeconds / env.AnimationBuilder.Props.TimeStep);

      if (AllValues()) {
        if (units.Count < 2 || !AllEqualAt(startFrameNumber, last)) {
          writer.WritePropertyName("t");
          writer.WriteStartArray();
          for (int i = 0; i < attr.Length; i++) {
            writer.WriteValue(attr[i].Value);
          }
          writer.WriteEndArray();
          
          string attrStr = stringWriter.ToString();
          Flush();

          yield return attrStr;
        }
        yield return null;
      } else {
        T[] prev = last;

        for (int i = startFrameNumber; i <= stopFrameNumber; i++) {
          if (units.Count < 2 || !AllEqualAt(i, prev)) {
            List<T> currList = new List<T>(attr.Length);

            for (int j = 0; j < attr.Length; j++) {
              currList.Add(attr[i].GetValueAt(i));
            }
            T[] currArray = currList.ToArray();

            writer.WritePropertyName("t");
            writer.WriteStartArray();
            for (int z = 0; z < attr.Length; z++) {
              writer.WriteValue(attr[z].GetValueAt(i));
            }
            writer.WriteEndArray();

            string attrStr = stringWriter.ToString();
            Flush();
            prev = currArray;

            yield return attrStr;
          } else {
            yield return String.Empty;
          }
        }
      }
    }
    #endregion

    private List<IEnumerator<string>> GetAttrEnums(RectangleAnimationProps props, DateTime start, DateTime stop) {
      int[] prevTrans;
      string prevFillColor;
      string prevLineColor;
      int prevLineWidth;
      bool prevVisible;

      if (units.Count < 2) {
        prevTrans = null;
        prevFillColor = null;
        prevLineColor = null;
        prevLineWidth = default(int);
        prevVisible = default(bool);
      } else {
        AnimationUnit prev = units[units.Count - 2];

        // TODO
        prevTrans = null;
        prevFillColor = null;
        prevLineColor = null;
        prevLineWidth = default(int);
        prevVisible = default(bool);
      }

      return new List<IEnumerator<string>>(8) {
          GetTransformationEnum(new AnimationAttribute<int>[] {props.X, props.Y, props.Width, props.Height}, new string[] {"x", "y", "width", "height"}, prevTrans, start, stop),
          GetEnum(props.FillColor, "fillColor", prevFillColor, start, stop),
          GetEnum(props.LineColor, "lineColor", prevLineColor, start, stop),
          GetEnum(props.LineWidth, "lineWidth", prevLineWidth, start, stop),
          GetEnum(props.Visible, "visible", prevVisible, start, stop)
      };
    }

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    // TODO: what to do with empty frames
    public List<AnimationUnit> FramesFromTo(DateTime start, DateTime stop) {
      RectangleAnimationProps props = propsList[propsList.Count - 1];
      if (props.AllValues()) {
        AnimationUnit unit = new AnimationUnit(start, start.AddSeconds(1), 1);
        unit.AddFrame(GetValueInitFrame(props));

        return new List<AnimationUnit>(1) { unit };
      } else {
        int frameNumber = Convert.ToInt32((stop - start).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
        AnimationUnit unit = new AnimationUnit(start, stop, frameNumber);
        List<AnimationUnit> affectedUnits = new List<AnimationUnit>(1) { unit };

        List<IEnumerator<string>> attrEnums = GetAttrEnums(props, start, stop);

        for (int i = 0; i < frameNumber; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();
          bool first = true;

          for (int j = attrEnums.Count - 1; j >= 0; j--) {
            if (!attrEnums[j].MoveNext()) {
              attrEnums.RemoveAt(j);
              continue;
            }
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
        units.Add(unit);
        return affectedUnits;
      }
    }
  }
}
