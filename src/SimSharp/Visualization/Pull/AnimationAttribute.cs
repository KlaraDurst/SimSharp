﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Pull {
  public class AnimationAttribute<T> {
    public T Value { get; }
    public Func<int, T> Function { get; }

    public AnimationAttribute(T value) {
      Value = value;
    }

    public AnimationAttribute(Func<int, T> function) {
      Function = function;
    }

    public T GetValueAt(int t) {
      if (Function == null)
        return Value;
      else
        return Function(t);
    }

    public static implicit operator AnimationAttribute<T>(T value) {
      return new AnimationAttribute<T>(value);
    }

    public static implicit operator AnimationAttribute<T>(Func<int, T> function) {
      return new AnimationAttribute<T>(function);
    }
  }
}
