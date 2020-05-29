using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public override Dictionary<string, int[]> GetValueAttributes() {
      return new Dictionary<string, int[]> {
        { "cx", new int[] { Cx.Value } },
        { "cy", new int[] { Cy.Value } },
        { "rx", new int[] { Rx.Value } },
        { "ry", new int[] { Ry.Value } }
      };
    }

    public override Dictionary<string, int[]> GetCurrValueAttributes() {
      return new Dictionary<string, int[]> {
        { "cx", new int[] { Cx.CurrValue } },
        { "cy", new int[] { Cy.CurrValue } },
        { "rx", new int[] { Rx.CurrValue } },
        { "ry", new int[] { Ry.CurrValue } }
      };
    }

    public override void SetCurrValueAttributes(Dictionary<string, int[]> currValues) {
      currValues.TryGetValue("cx", out int[] cx);
      Cx.CurrValue = cx[0];
      currValues.TryGetValue("cy", out int[] cy);
      Cy.CurrValue = cy[0];
      currValues.TryGetValue("rx", out int[] rx);
      Rx.CurrValue = rx[0];
      currValues.TryGetValue("ry", out int[] ry);
      Ry.CurrValue = ry[0];
    }

    public override Dictionary<string, int[]> GetValueAttributesAt(int i) {
      return new Dictionary<string, int[]> {
        { "cx", new int[] { Cx.GetValueAt(i) } },
        { "cy", new int[] { Cy.GetValueAt(i) } },
        { "rx", new int[] { Rx.GetValueAt(i) } },
        { "ry", new int[] { Ry.GetValueAt(i) } }
      };
    }

    public override bool CompareAttributeValues(int[] a, int[] b) {
      return a[0] == b[0];
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
