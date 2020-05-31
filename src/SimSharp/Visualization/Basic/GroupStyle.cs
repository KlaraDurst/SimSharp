using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Basic.Shapes;

namespace SimSharp.Visualization.Basic {
  public class GroupStyle : Style {
    public Dictionary<string, (Shape, Style)> Children { get; }

    private int count;

    public GroupStyle(string fill, string stroke, int strokeWidth) : base (fill, stroke, strokeWidth) {
      Children = new Dictionary<string, (Shape, Style)>();
      this.count = 0;
    }

    public string AddChild(Shape shape, Style style) {
      string name = "Elem" + count.ToString();
      AddChild(name, shape, style);
      count++;
      return name;
    }

    public void AddChild(string name, Shape shape, Style style) {
      Children.Add(name, (shape, style));
    }

    public void RemoveChild(string name) {
      Children.Remove(name);
    }

    public override void WriteJson(AnimationBuilder animationBuilder, string name, JsonTextWriter writer, Style compare) {
      base.WriteJson(animationBuilder, name, writer, compare);

      if (compare == null) {
        if (Children.Count > 0) {
          writer.WritePropertyName("shapes");
          writer.WriteStartArray();

          foreach (KeyValuePair<string, (Shape, Style)> child in Children)
            WriteJson(child, animationBuilder, writer);
          
          writer.WriteEndArray();
        }
      } else {
        GroupStyle groupCompare = (GroupStyle)compare;
        if (!EqualChildren(groupCompare.Children)) {
          writer.WritePropertyName("shapes");
          writer.WriteStartArray();

          foreach (KeyValuePair<string, (Shape, Style)> child in Children) {
            if (groupCompare.Children.ContainsKey(child.Key)) 
              CompareAndWriteJson(child, groupCompare.Children[child.Key], animationBuilder, writer);
            else 
              WriteJson(child, animationBuilder, writer);
          }

          foreach (KeyValuePair<string, (Shape, Style)> child in groupCompare.Children) {
            if (!Children.ContainsKey(child.Key)) {
              writer.WritePropertyName(child.Key);
              writer.WriteStartObject();

              writer.WritePropertyName("visibility");
              writer.WriteValue(false);

              writer.WriteEndObject();
            }
          }
          writer.WriteEndArray();
        }
      }
    }

    private void CompareAndWriteJson(KeyValuePair<string, (Shape, Style)> child, (Shape, Style) other, AnimationBuilder animationBuilder, JsonTextWriter writer) {
      if (child.Value.Item2.Equals(other.Item2) || child.Value.Item1.Equals(other.Item1)) {
        animationBuilder.AddName(child.Key);
        writer.WritePropertyName(child.Key);
        writer.WriteStartObject();

        child.Value.Item2.WriteJson(animationBuilder, child.Key, writer, other.Item2);
        child.Value.Item1.WriteJson(writer, other.Item1);

        writer.WriteEndObject();
      }
    }

    private void WriteJson(KeyValuePair<string, (Shape, Style)> child, AnimationBuilder animationBuilder, JsonTextWriter writer) {
      animationBuilder.AddName(child.Key);
      writer.WritePropertyName(child.Key);
      writer.WriteStartObject();

      child.Value.Item2.WriteJson(animationBuilder, child.Key, writer, null);
      child.Value.Item1.WriteJson(writer, null);

      writer.WriteEndObject();
    }

    private bool EqualChildren(Dictionary<string, (Shape, Style)> other) {
      if (Children.Count != other.Count)
        return false;

      foreach (KeyValuePair<string, (Shape, Style)> child in Children) {
        if (other.ContainsKey(child.Key)) {
          if (!child.Value.Item1.Equals(other[child.Key].Item1))
            return false;
          if (!child.Value.Item2.Equals(other[child.Key].Item2))
            return false;
        } else {
          return false;
        }
      }
      return true;
    }

    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        GroupStyle gs = (GroupStyle)obj;
        return base.Equals(obj) && EqualChildren(gs.Children);
      }
    }

    public override int GetHashCode() {
      return (Fill, Stroke, StrokeWidth, Children).GetHashCode();
    }
  }
}
