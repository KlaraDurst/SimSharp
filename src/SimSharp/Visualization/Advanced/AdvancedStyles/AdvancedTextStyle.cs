using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedStyles {
  public class AdvancedTextStyle : AdvancedStyle {
    public class TextState : State {
      public string Text { get; }

      public TextState(AdvancedTextStyle style) : base (style) {
        Text = style.Text.CurrValue;
      }
    }

    public AnimationAttribute<string> Text { get; set; }

    public AdvancedTextStyle(AnimationAttribute<string> fill, AnimationAttribute<string> stroke, AnimationAttribute<int> strokeWidth, AnimationAttribute<string> text) 
      : base (fill, stroke, strokeWidth) {
      Text = text;
    }

    public override State GetState() {
      return new TextState(this);
    }

    public override void WriteValueJson(JsonTextWriter writer, AdvancedStyle compare) {
      base.WriteValueJson(writer, compare);

      if (compare == null) {
        writer.WritePropertyName("text");
        writer.WriteValue(Text.Value);
      } else {
        AdvancedTextStyle textCompare = (AdvancedTextStyle)compare;
        if (textCompare.Text.CurrValue != Text.Value) {
          writer.WritePropertyName("text");
          writer.WriteValue(Text.Value);
        }
      }
    }

    public override void WriteValueAtJson(int i, JsonTextWriter writer, State compare) {
      base.WriteValueAtJson(i, writer, compare);

      if (compare == null) {
        string text = Text.GetValueAt(i);
        writer.WritePropertyName("text");
        writer.WriteValue(text);
        Text.CurrValue = text;
      } else {
        TextState textCompare = (TextState)compare;
        string text = Text.GetValueAt(i);
        if (textCompare.Text != text) {
          writer.WritePropertyName("text");
          writer.WriteValue(text);
        }
        Text.CurrValue = text;
      }
    }

    public override bool AllValues() {
      if (!base.AllValues())
        return false;
      if (Text.Function != null)
        return false;
      return true;
    }
  }
}
