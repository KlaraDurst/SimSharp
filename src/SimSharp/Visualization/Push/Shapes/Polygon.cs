using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimSharp.Visualization.Push.Shapes {
  public class Polygon {
    public int[] Points { get; }

    public Polygon(params int[] points) {
      if (points.Length % 2 != 0) {
        throw new ArgumentException("A polygon needs the same number of y and x coordinates");
      }
      else {
        Points = points;
      }
    }

    public Dictionary<string, int[]> GetTransformation() {
      return new Dictionary<string, int[]> {
        { "points", Points },
      };
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Polygon p = (Polygon)obj;
        return Points.SequenceEqual(p.Points);
      }
    }

    public override int GetHashCode() {
      return Points.GetHashCode();
    }
  }
}
