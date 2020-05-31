using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  public class AdvancedEllipse : AdvancedShape {
    public AnimationAttribute<int> Cx { get; set; }
    public AnimationAttribute<int> Cy { get; set; }
    public AnimationAttribute<int> Rx { get; set; }
    public AnimationAttribute<int> Ry { get; set; }

    public AdvancedEllipse(AnimationAttribute<int> cx, AnimationAttribute<int> cy, AnimationAttribute<int> rx, AnimationAttribute<int> ry) {
      Cx = cx;
      Cy = cy;
      Rx = rx;
      Ry = ry;
    }

    public override void WriteValueJson(JsonTextWriter writer, AdvancedShape compare) {
      if (compare == null) {
        writer.WritePropertyName("cx");
        writer.WriteValue(Cx.Value);

        writer.WritePropertyName("cy");
        writer.WriteValue(Cy.Value);

        writer.WritePropertyName("rx");
        writer.WriteValue(Rx.Value);

        writer.WritePropertyName("ry");
        writer.WriteValue(Ry.Value);
      } else {
        AdvancedEllipse e = (AdvancedEllipse)compare;

        if (e.Cx.CurrValue != Cx.Value) {
          writer.WritePropertyName("cx");
          writer.WriteValue(Cx.Value);
        }

        if (e.Cy.CurrValue != Cy.Value) {
          writer.WritePropertyName("cy");
          writer.WriteValue(Cy.Value);
        }

        if (e.Rx.CurrValue != Rx.Value) {
          writer.WritePropertyName("rx");
          writer.WriteValue(Rx);
        }

        if (e.Ry.CurrValue != Ry.Value) {
          writer.WritePropertyName("ry");
          writer.WriteValue(Ry);
        }
      }
    }

    public override void WriteValueAtJson(int i, JsonTextWriter writer, Dictionary<string, int[]> compare) {
      if (compare == null) {
        int cx = Cx.GetValueAt(i);
        writer.WritePropertyName("cx");
        writer.WriteValue(cx);
        Cx.CurrValue = cx;

        int cy = Cy.GetValueAt(i);
        writer.WritePropertyName("cy");
        writer.WriteValue(cy);
        Cy.CurrValue = cy;

        int rx = Rx.GetValueAt(i);
        writer.WritePropertyName("rx");
        writer.WriteValue(rx);
        Rx.CurrValue = rx;

        int ry = Ry.GetValueAt(i);
        writer.WritePropertyName("ry");
        writer.WriteValue(ry);
        Ry.CurrValue = ry;
      } else {
        compare.TryGetValue("cx", out int[] prevCx);
        compare.TryGetValue("cy", out int[] prevCy);
        compare.TryGetValue("rx", out int[] prevRx);
        compare.TryGetValue("ry", out int[] prevRy);

        int cx = Cx.GetValueAt(i);
        if (prevCx[0] != cx) {
          writer.WritePropertyName("cx");
          writer.WriteValue(cx);
        }
        Cx.CurrValue = cx;

        int cy = Cy.GetValueAt(i);
        if (prevCy[0] != cy) {
          writer.WritePropertyName("cy");
          writer.WriteValue(cy);
        }
        Cy.CurrValue = cy;

        int rx = Rx.GetValueAt(i);
        if (prevRx[0] != rx) {
          writer.WritePropertyName("rx");
          writer.WriteValue(rx);
        }
        Rx.CurrValue = rx;

        int ry = Ry.GetValueAt(i);
        if (prevRy[0] != ry) {
          writer.WritePropertyName("ry");
          writer.WriteValue(ry);
        }
        Ry.CurrValue = ry;
      }
    }

    public override Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "cx", new int[] { Cx.CurrValue } },
        { "cy", new int[] { Cy.CurrValue } },
        { "rx", new int[] { Rx.CurrValue } },
        { "ry", new int[] { Ry.CurrValue } }
      };
    }

    public override bool AllValues() {
      if (Cx.Function != null)
        return false;
      if (Cy.Function != null)
        return false;
      if (Rx.Function != null)
        return false;
      if (Ry.Function != null)
        return false;
      return true;
    }
  }
}
