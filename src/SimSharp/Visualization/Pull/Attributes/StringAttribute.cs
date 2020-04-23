using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull.Attributes {
  public class StringAttribute {
    public string Value { get; }
    public Func<int, string> Function { get; }

    public StringAttribute(string value) {
      Value = value;
    }

    public StringAttribute(Func<int, string> function) {
      Function = function;
    }

    public string GetValueAt(int t) {
      if (Function == null)
        return Value;
      else
        return Function(t);
    }

    public static implicit operator StringAttribute(string value) {
      return new StringAttribute(value);
    }

    public static implicit operator StringAttribute(Func<int, string> function) {
      return new StringAttribute(function);
    }
  }
}
