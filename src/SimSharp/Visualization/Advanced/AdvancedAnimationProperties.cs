using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Advanced.AdvancedShapes;
using SimSharp.Visualization.Advanced.AdvancedStyles;

namespace SimSharp.Visualization.Advanced {
  public class AdvancedAnimationProperties {
    public AdvancedShape Shape { get; set; }
    public AdvancedStyle Style { get; set; }
    public AnimationAttribute<bool> Visibility { get; set; }

    public bool Written { get; set; }

    protected string typeStr;
    protected string removeStr;

    public AdvancedAnimationProperties(AdvancedShape shape, AdvancedStyle style, AnimationAttribute<bool> visibility) {
      Shape = shape;
      Style = style;
      Visibility = visibility;
      Written = false;

      this.typeStr = Shape.GetType().Name.ToLower();
      this.removeStr = "advanced";
    }

    public AdvancedAnimationProperties(AdvancedAnimationProperties props) {
      Shape = props.Shape;
      Style = props.Style;
      Visibility = props.Visibility;
      Written = false;

      this.typeStr = Shape.GetType().Name.ToLower();
      this.removeStr = "advanced";
    }
    
    public void WriteValueJson(JsonTextWriter writer, bool currVisible, AdvancedAnimationProperties compare) {
      if (compare == null) {
        writer.WritePropertyName("type");
        writer.WriteValue(typeStr.Remove(typeStr.IndexOf(removeStr), removeStr.Length));

        Style.WriteValueJson(writer, null);

        writer.WritePropertyName("visibility");
        writer.WriteValue(Visibility.Value);

        Shape.WriteValueJson(writer, null);
      } else {
        Style.WriteValueJson(writer, compare.Style);

        if (!currVisible) {
          writer.WritePropertyName("visibility");
          writer.WriteValue(Visibility.Value);
        }

        Shape.WriteValueJson(writer, compare.Shape);
      }
      Written = true;
    }

    public void WriteValueAtJson(int i, JsonTextWriter writer, bool currVisible, AdvancedStyle.State compare, Dictionary<string, int[]> prevAttributes) {
      if (compare == null) {
        writer.WritePropertyName("type");
        writer.WriteValue(typeStr.Remove(typeStr.IndexOf(removeStr), removeStr.Length));

        Style.WriteValueAtJson(i, writer, null);

        writer.WritePropertyName("visibility");
        writer.WriteValue(true);
      } else {
        Style.WriteValueAtJson(i, writer, compare);

        if (!currVisible) {
          writer.WritePropertyName("visibility");
          writer.WriteValue(true);
        }
      }
      Visibility.CurrValue = true;
      Shape.WriteValueAtJson(i, writer, prevAttributes);

      Written = true;
    }

    public bool AllValues() {
      if (!Shape.AllValues())
        return false;
      if (!Style.AllValues())
        return false;
      if (Visibility.Function != null)
        return false;
      return true;
    }
  }
}
