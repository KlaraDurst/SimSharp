using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  public class AdvancedText : AdvancedShape {
    public AnimationAttribute<int> X { get; set; }
    public AnimationAttribute<int> Y { get; set; }
    public AnimationAttribute<int> TextLength { get; set; }
    public AnimationAttribute<int> FontSize { get; set; }

    public AdvancedText(AnimationAttribute<int> x, AnimationAttribute<int> y, AnimationAttribute<int> textLength, AnimationAttribute<int> fontSize) {
      X = x;
      Y = y;
      TextLength = textLength;
      FontSize = fontSize;
    }

    public void WriteValueJson(JsonTextWriter writer, AdvancedShape compare) {
      if (compare == null) {
        writer.WritePropertyName("x");
        writer.WriteValue(X.Value);

        writer.WritePropertyName("y");
        writer.WriteValue(Y.Value);

        writer.WritePropertyName("textLength");
        writer.WriteValue(TextLength.Value);

        writer.WritePropertyName("font-size");
        writer.WriteValue(FontSize.Value);
      } else {
        AdvancedText t = (AdvancedText)compare;

        if (t.X.CurrValue != X.Value) {
          writer.WritePropertyName("x");
          writer.WriteValue(X.Value);
        }

        if (t.Y.CurrValue != Y.Value) {
          writer.WritePropertyName("y");
          writer.WriteValue(Y.Value);
        }

        if (t.TextLength.CurrValue != TextLength.Value) {
          writer.WritePropertyName("textLength");
          writer.WriteValue(TextLength.Value);
        }

        if (t.FontSize.CurrValue != FontSize.Value) {
          writer.WritePropertyName("font-size");
          writer.WriteValue(FontSize.Value);
        }
      }
    }

    public void WriteValueAtJson(int i, JsonTextWriter writer, Dictionary<string, int[]> compare) {
      if (compare == null) {
        int x = X.GetValueAt(i);
        writer.WritePropertyName("x");
        writer.WriteValue(x);
        X.CurrValue = x;

        int y = Y.GetValueAt(i);
        writer.WritePropertyName("y");
        writer.WriteValue(y);
        Y.CurrValue = y;

        int textLength = TextLength.GetValueAt(i);
        writer.WritePropertyName("textLength");
        writer.WriteValue(textLength);
        TextLength.CurrValue = textLength;

        int fontSize = FontSize.GetValueAt(i);
        writer.WritePropertyName("font-size");
        writer.WriteValue(fontSize);
        FontSize.CurrValue = fontSize;
      } else {
        compare.TryGetValue("x", out int[] prevX);
        compare.TryGetValue("y", out int[] prevY);
        compare.TryGetValue("textLength", out int[] prevWidht);
        compare.TryGetValue("font-size", out int[] prevFontSize);

        int x = X.GetValueAt(i);
        if (prevX[0] != x) {
          writer.WritePropertyName("x");
          writer.WriteValue(x);
        }
        X.CurrValue = x;

        int y = Y.GetValueAt(i);
        if (prevY[0] != y) {
          writer.WritePropertyName("y");
          writer.WriteValue(y);
        }
        Y.CurrValue = y;

        int textLength = TextLength.GetValueAt(i);
        if (prevWidht[0] != textLength) {
          writer.WritePropertyName("textLength");
          writer.WriteValue(textLength);
        }
        TextLength.CurrValue = textLength;

        int fontSize = FontSize.GetValueAt(i);
        if (prevFontSize[0] != fontSize) {
          writer.WritePropertyName("font-size");
          writer.WriteValue(fontSize);
        }
        FontSize.CurrValue = fontSize;
      }
    }

    public Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "x", new int[] { X.CurrValue } },
        { "y", new int[] { Y.CurrValue } },
        { "textLength", new int[] { TextLength.CurrValue } },
        { "font-size", new int[] { FontSize.CurrValue } }
      };
    }

    public bool AllValues() {
      if (X.Function != null)
        return false;
      if (Y.Function != null)
        return false;
      if (TextLength.Function != null)
        return false;
      if (FontSize.Function != null)
        return false;
      return true;
    }
  }
}
