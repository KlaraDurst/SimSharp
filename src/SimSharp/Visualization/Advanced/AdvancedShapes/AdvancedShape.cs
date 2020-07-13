using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  public interface AdvancedShape {
    void WriteValueJson(JsonTextWriter writer, AdvancedShape compare);
    void WriteValueAtJson(int i, JsonTextWriter writer, Dictionary<string, int[]> compare);
    Dictionary<string, int[]> GetCurrValueAttributes();
    bool AllValues();
  }
}
