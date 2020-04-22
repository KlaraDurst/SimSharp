using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Pull.Attributes;

namespace SimSharp.Visualization.Pull {
  public class RectangleAnimation : FramesProvider {
    public string Name { get; set; }

    private Simulation env;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<RectangleAnimationProps> propsList;

    public RectangleAnimation (string name, IntAttribute x, IntAttribute y, IntAttribute width, IntAttribute height, StringAttribute fillColor, StringAttribute lineColor, IntAttribute lineWidth, BoolAttribute visible, Simulation env) {
      Name = name;
      this.env = env;
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);

      RectangleAnimationProps props = new RectangleAnimationProps(x, y, width, height, fillColor, lineColor, lineWidth, visible);
      propsList.Add(props);
    }

    #region Get animation props
    public IntAttribute GetX() {
      return propsList[propsList.Count - 1].X;
    }

    public IntAttribute GetY() {
      return propsList[propsList.Count - 1].Y;
    }

    public IntAttribute GetWidth() {
      return propsList[propsList.Count - 1].Width;
    }

    public IntAttribute GetHeight() {
      return propsList[propsList.Count - 1].Height;
    }

    public StringAttribute GetFillColor() {
      return propsList[propsList.Count - 1].FillColor;
    }

    public StringAttribute GetLineColor() {
      return propsList[propsList.Count - 1].LineColor;
    }

    public IntAttribute GetLineWidth() {
      return propsList[propsList.Count - 1].LineWidth;
    }

    public BoolAttribute GetVisible() {
      return propsList[propsList.Count - 1].Visible;
    }
    #endregion

    #region Set animation props
    public void SetX(IntAttribute x) {
      propsList[propsList.Count - 1].X = x;
    }

    public void SetY(IntAttribute y) {
      propsList[propsList.Count - 1].Y = y;
    }

    public void SetWidth(IntAttribute width) {
      propsList[propsList.Count - 1].Width = width;
    }

    public void SetHeight(IntAttribute height) {
      propsList[propsList.Count - 1].Height = height;
    }

    public void SetFillColor(StringAttribute fillColor) {
      propsList[propsList.Count - 1].FillColor = fillColor;
    }

    public void SetLineColor(StringAttribute lineColor) {
      propsList[propsList.Count - 1].LineColor = lineColor;
    }

    public void SetLineWidth(IntAttribute lineWidth) {
      propsList[propsList.Count - 1].LineWidth = lineWidth;
    }

    public void SetVisible(BoolAttribute visible) {
      propsList[propsList.Count - 1].Visible = visible;
    }
    #endregion

    private bool AllValues(RectangleAnimationProps props) {
      if (props.X.Function != null)
        return false;
      if (props.Y.Function != null)
        return false;
      if (props.Width.Function != null)
        return false;
      if (props.Height.Function != null)
        return false;
      if (props.FillColor.Function != null)
        return false;
      if (props.LineColor.Function != null)
        return false;
      if (props.LineWidth.Function != null)
        return false;
      if (props.Visible.Function != null)
        return false;
      return true;
    }

    private string GetInitFrame(RectangleAnimationProps props) {
      if (!AllValues(props))
        throw new ArgumentException("Init frame can not be generated with function attribute.");

      writer.WritePropertyName(Name);
      writer.WriteStartObject();

      if (propsList.Count < 2) {
        writer.WritePropertyName("type");
        writer.WriteValue("rectangle");

        writer.WritePropertyName("fillColor");
        writer.WriteValue(props.FillColor);

        writer.WritePropertyName("lineColor");
        writer.WriteValue(props.LineColor);

        writer.WritePropertyName("lineWidth");
        writer.WriteValue(props.LineWidth);

        writer.WritePropertyName("visible");
        writer.WriteValue(props.Visible);
      } else {
        if (propsList[propsList.Count - 2].FillColor != props.FillColor) {
          writer.WritePropertyName("fillColor");
          writer.WriteValue(props.FillColor);
        }

        if (propsList[propsList.Count - 2].LineColor != props.LineColor) {
          writer.WritePropertyName("lineColor");
          writer.WriteValue(props.LineColor);
        }

        if (propsList[propsList.Count - 2].LineWidth != props.LineWidth) {
          writer.WritePropertyName("lineWidth");
          writer.WriteValue(props.LineWidth);
        }

        if (propsList[propsList.Count - 2].Visible != props.Visible) {
          writer.WritePropertyName("visible");
          writer.WriteValue(props.Visible);
        }
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

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }

    public List<AnimationUnit> FramesFromTo(DateTime start, DateTime stop) {
      RectangleAnimationProps props = propsList[propsList.Count - 1];
      if (AllValues(props)) {
        AnimationUnit unit = new AnimationUnit(start, start.AddSeconds(1), 1);
        unit.AddFrame(GetInitFrame(props));

        return new List<AnimationUnit>(1) { unit };
      } else {
        int frameNumber = Convert.ToInt32((stop - start).TotalSeconds / env.AnimationBuilder.Props.TimeStep);
        AnimationUnit unit = new AnimationUnit(start, stop, frameNumber);
        List<AnimationUnit> units = new List<AnimationUnit>(1) { unit };

        List<IEnumerator<string>> AttrEnums = new List<IEnumerator<string>>(frameNumber); // TODO

        for (int i = 0; i < frameNumber; i++) {
          writer.WritePropertyName(Name);
          writer.WriteStartObject();
          bool first = true;

          for (int j = AttrEnums.Count - 1; j >= 0; j--) {
            if (!AttrEnums[j].MoveNext()) {
              AttrEnums.RemoveAt(j);
              continue;
            }
            string attr = AttrEnums[j].Current;

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
        return units;
      }
    }
  }
}
