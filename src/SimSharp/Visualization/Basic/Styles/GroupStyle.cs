using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SimSharp.Visualization.Basic.Shapes;

namespace SimSharp.Visualization.Basic.Styles {
  public class GroupStyle : Style {
    public Dictionary<string, (Shape, Style)> Children { get; }

    public GroupStyle(string fill, string stroke, int strokeWidth) : base (fill, stroke, strokeWidth) {
      Children = new Dictionary<string, (Shape, Style)>();
    }

    public void AddChild(string name, Shape shape, Style style) {
      CheckName(name);
      Children.Add(name, (shape, style));
    }

    private void CheckName(string name) {
      if (name.Contains("/"))
        throw new ArgumentException("name must not contain '/'");
      if (Children.ContainsKey(name))
        throw new ArgumentException("Animation Children need to have a unique name.");
    }

    public override void WriteJson(string name, JsonTextWriter writer, Style compare) {
      base.WriteJson(name, writer, compare);

      if (compare == null) {
        if (Children.Count > 0) {
          writer.WritePropertyName("shapes");
          writer.WriteStartObject();

          foreach (KeyValuePair<string, (Shape, Style)> child in Children)
            WriteJson(child, name, writer);
          
          writer.WriteEndObject();
        }
      } else {
        GroupStyle groupCompare = (GroupStyle)compare;
        if (!EqualChildren(groupCompare.Children)) {
          writer.WritePropertyName("shapes");
          writer.WriteStartObject();

          foreach (KeyValuePair<string, (Shape, Style)> child in Children) {
            if (groupCompare.Children.ContainsKey(child.Key)) 
              CompareAndWriteJson(child, groupCompare.Children[child.Key], name, writer);
            else 
              WriteJson(child, name, writer);
          }

          foreach (KeyValuePair<string, (Shape, Style)> child in groupCompare.Children) {
            if (!Children.ContainsKey(child.Key)) {
              writer.WritePropertyName(name + "/" + child.Key);
              writer.WriteStartObject();

              writer.WritePropertyName("remove");
              writer.WriteValue(true);

              writer.WriteEndObject();
            }
          }
          writer.WriteEndObject();
        }
      }
    }

    private void CompareAndWriteJson(KeyValuePair<string, (Shape, Style)> child, (Shape, Style) other, string name, JsonTextWriter writer) {
      if (!child.Value.Item2.Equals(other.Item2) || !child.Value.Item1.Equals(other.Item1)) {
        writer.WritePropertyName(name + "/" + child.Key);
        writer.WriteStartObject();

        child.Value.Item2.WriteJson(child.Key, writer, other.Item2);
        child.Value.Item1.WriteJson(writer, other.Item1);

        writer.WriteEndObject();
      }
    }

    private void WriteJson(KeyValuePair<string, (Shape, Style)> child, string name, JsonTextWriter writer) {
      writer.WritePropertyName(name + "/" + child.Key);
      writer.WriteStartObject();

      writer.WritePropertyName("type");
      writer.WriteValue(child.Value.Item1.GetType().Name.ToLower());

      child.Value.Item2.WriteJson(child.Key, writer, null);
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
