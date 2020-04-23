using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull.Attributes {
  public class BoolAttribute {
    public bool Value { get; }
    public Func<int, bool> Function { get; }

    public BoolAttribute(bool value) {
      Value = value;
    }

    public BoolAttribute(Func<int, bool> function) {
      Function = function;
    }

    public bool GetValueAt(int t) {
      if (Function == null)
        return Value;
      else
        return Function(t);
    }

    public static implicit operator BoolAttribute(bool value) {
      return new BoolAttribute(value);
    }

    public static implicit operator BoolAttribute(Func<int, bool> function) {
      return new BoolAttribute(function);
    }
  }
}
