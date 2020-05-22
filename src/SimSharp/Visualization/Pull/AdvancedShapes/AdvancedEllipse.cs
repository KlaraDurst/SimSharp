using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp.Visualization.Pull.AdvancedShapes {
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
      Cx.CurrValue = currValues.Values.ElementAt(0)[0];
      Cy.CurrValue = currValues.Values.ElementAt(1)[0];
      Rx.CurrValue = currValues.Values.ElementAt(2)[0];
      Ry.CurrValue = currValues.Values.ElementAt(3)[0];
    }

    public override Dictionary<string, int[]> GetValueAttributesAt(int i) {
      return new Dictionary<string, int[]> {
        { "cx", new int[] { Cx.GetValueAt(i) } },
        { "cy", new int[] { Cy.GetValueAt(i) } },
        { "rx", new int[] { Rx.GetValueAt(i) } },
        { "ry", new int[] { Ry.GetValueAt(i) } }
      };
    }

    public override bool CompareValues(int[] a, int[] b) {
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
