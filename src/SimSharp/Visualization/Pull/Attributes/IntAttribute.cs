using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull.Attributes {
  public class IntAttribute {
    public int Value { get; }
    public Func<int, int> Function { get; }

    public IntAttribute(int value) {
      Value = value;
    }

    public IntAttribute(Func<int, int> function) {
      Function = function;
    }

    public static implicit operator IntAttribute(int value) {
      return new IntAttribute(value);
    }

    public static implicit operator IntAttribute(Func<int, int> function) {
      return new IntAttribute(function);
    }
  }
}
