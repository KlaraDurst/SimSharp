using System;
using System.Collections.Generic;
using System.Text;

namespace SimSharp.Visualization.Push.Shapes {
  public class Polygon : Shape {
    public int[] Points { get; private set; }

    public Polygon(params int[] points) {
      if (points.Length % 2 != 0) {
        throw new ArgumentException("A polygon needs the same number of y and x coordinates");
      }
      else {
        Points = points;
      }
    }

    public override Dictionary<string, int[]> GetAttributes() {
      return new Dictionary<string, int[]> {
        { "points", Points },
      };
    }

    public override bool CompareAttributeValues(int[] a, int[] b) {
      if (!a.Length.Equals(b.Length))
        return false;

      for (int i = 0; i < a.Length; i++) {
        if (a[i] != b[i])
          return false;
      }

      return true;
    }

    public override bool CompareAttributeValues(List<int> a, int[] b) {
      if (!a.Count.Equals(b.Length))
        return false;

      for (int i = 0; i < a.Count; i++) {
        if (a[i] != b[i])
          return false;
      }

      return true;
    }

    public override void MoveRight(int space) {
      for (int i = 0; i < Points.Length; i += 2) {
        Points[i] += space;
      }
    }

    public override Shape Copy() {
      return new Polygon(Points);
    }

    public override Shape CopyAndSet(Dictionary<string, int[]> attributes) {
      attributes.TryGetValue("points", out int[] points);
      return new Polygon(points);
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        Polygon p = (Polygon)obj;
        if (!Points.Length.Equals(p.Points))
          return false;

        for (int i = 0; i < Points.Length; i++) {
          if (Points[i] != p.Points[i])
            return false;
        }

        return true;
      }
    }

    public override int GetHashCode() {
      return Points.GetHashCode();
    }
  }
}
