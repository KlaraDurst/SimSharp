using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Advanced.AdvancedShapes;
using SimSharp.Visualization.Advanced.AdvancedStyles;
using SimSharp.Visualization.Basic.Styles;

namespace SimSharp.Visualization.Advanced {
  public class AdvancedGroupAnimation : AdvancedAnimation {
    public List<AdvancedAnimation> Children { get; }

    private List<string> names;

    public AdvancedGroupAnimation(string name, AdvancedGroup group, AdvancedStyle style, AnimationAttribute<bool> visibility) 
      : base(name, group, style, visibility) {
      Children = new List<AdvancedAnimation>();
      this.names = new List<string>();
    }

    public AdvancedAnimation AddChild(string name, AdvancedShape shape, AnimationAttribute<bool> visibility) {
      return AddChild(name, shape, new AdvancedStyle(), visibility);
    }

    public AdvancedAnimation AddChild(string name, AdvancedShape shape, AdvancedStyle style, AnimationAttribute<bool> visibility) {
      AddName(name);
      AdvancedAnimation animation = new AdvancedAnimation(Name + "/" + name, shape, style, visibility);
      Children.Add(animation);
      return animation;
    }

    private void AddName(string name) {
      CheckName(name);
      names.Add(name);
    }

    private void CheckName(string name) {
      if (name.Contains("/"))
        throw new ArgumentException("name must not contain '/'");
      if (names.Contains(name))
        throw new ArgumentException("Animation Children need to have a unique name.");
    }

    private void WriteChildrenValueJson() {
      if (Children.Count > 0) {
        writer.WritePropertyName("shapes");
        writer.WriteStartObject();
        bool first = true;

        foreach (AdvancedAnimation child in Children) {
          if (!first)
            writer.WriteRaw(",");
          writer.WriteRaw(child.GetValueInitFrame());

          first = false;
        }

        writer.WriteEndObject();
      }
    }

    private void WriteChildrenValueAtJson(int i) {
      if (Children.Count > 0) {
        List<List<AnimationUnit>> units = new List<List<AnimationUnit>>();
        foreach (AdvancedAnimation child in Children) {
          List<AnimationUnit> childUnits = child.FramesFromTo(i, i);
          if (childUnits.Count > 0)
            units.Add(childUnits);
        }

        if (units.Count > 0) {
          writer.WritePropertyName("shapes");
          writer.WriteStartObject();
          bool first = true;

          foreach (List<AnimationUnit> l in units) {
            if (!first)
              writer.WriteRaw(",");
            writer.WriteRaw(l[0].Frames[0]);

            first = false;
          }

          writer.WriteEndObject();
        }
      }
    }

    protected override void WriteValueJson(AdvancedAnimationProperties props, AdvancedAnimationProperties prevWritten) {
      base.WriteValueJson(props, prevWritten);
      WriteChildrenValueJson();
    }

    public override bool AllValues() {
      if (!base.AllValues())
        return false;

      foreach (AdvancedAnimation child in Children) {
        if (!child.AllValues())
          return false;
      }

      return true;
    }

    protected override void WriteValueAtJson(AdvancedAnimationProperties props, int i, AdvancedStyle.State propsState, Dictionary<string, int[]> prevAttributes) {
      base.WriteValueAtJson(props, i, propsState, prevAttributes);
      WriteChildrenValueAtJson(i);
    }
  }
}
