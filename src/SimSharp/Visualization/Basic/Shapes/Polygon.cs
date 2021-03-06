﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization.Basic.Shapes {
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

    public void WriteJson(JsonTextWriter writer, Shape compare) {
      if (compare == null) {
        writer.WritePropertyName("points");
        writer.WriteStartArray();
        foreach(int p in Points) {
          writer.WriteValue(p);
        }
        writer.WriteEndArray();
      } else {
        Polygon p = (Polygon)compare;

        if (!CompareAttributeValues(Points, p.Points)) {
          writer.WritePropertyName("points");
          writer.WriteStartArray();
          foreach (int val in Points) {
            writer.WriteValue(val);
          }
          writer.WriteEndArray();
        }
      }
    }

    public Dictionary<string, int[]> GetAttributes() {
      return new Dictionary<string, int[]> {
        { "points", Points },
      };
    }

    public bool CompareAttributeValues(int[] a, int[] b) {
      if (!a.Length.Equals(b.Length))
        return false;

      for (int i = 0; i < a.Length; i++) {
        if (a[i] != b[i])
          return false;
      }

      return true;
    }

    public bool CompareAttributeValues(List<int> a, int[] b) {
      if (!a.Count.Equals(b.Length))
        return false;

      for (int i = 0; i < a.Count; i++) {
        if (a[i] != b[i])
          return false;
      }

      return true;
    }

    public void MoveUp(int space) {
      for (int i = 1; i < Points.Length; i += 2) {
        Points[i] -= space;
      }
    }

    public void MoveRight(int space) {
      for (int i = 0; i < Points.Length; i += 2) {
        Points[i] += space;
      }
    }

    public void MoveDown(int space) {
      for (int i = 1; i < Points.Length; i += 2) {
        Points[i] += space;
      }
    }

    public void MoveLeft(int space) {
      for (int i = 0; i < Points.Length; i += 2) {
        Points[i] -= space;
      }
    }

    public Shape Copy() {
      return new Polygon(Points);
    }

    public Shape CopyAndSet(Dictionary<string, int[]> attributes) {
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
