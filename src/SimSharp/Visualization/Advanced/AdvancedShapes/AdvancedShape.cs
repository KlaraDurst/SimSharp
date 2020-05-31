using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Advanced.AdvancedShapes {
  abstract public class AdvancedShape {
    public abstract void WriteValueJson(JsonTextWriter writer, AdvancedShape compare);
    public abstract void WriteValueAtJson(int i, JsonTextWriter writer, Dictionary<string, int[]> compare);
    public abstract Dictionary<string, int[]> GetCurrValueAttributes();
    public abstract bool AllValues();
  }
}
