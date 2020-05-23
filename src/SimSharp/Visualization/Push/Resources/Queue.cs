﻿using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Push.Shapes;

namespace SimSharp.Visualization.Push.Resources {
  public class Queue {
    public string Name { get; }
    public Shape Shape { get; }
    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }
    public int Space { get; }
    public int MaxLength { get; }

    private AnimationBuilder animationBuilder;
    private List<Animation> elementList;
    private int elementCount;

    public Queue(string name, Shape shape, string fill, string stroke, int strokeWidth, int space, int maxLength, AnimationBuilder animationBuilder) {
      Name = name;
      Shape = shape;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Space = space;
      MaxLength = maxLength;

      this.animationBuilder = animationBuilder;
      this.elementList = new List<Animation>();
      this.elementCount = 0;
    }

    public void Enqueue() {
      elementCount++;
      if (elementCount <= MaxLength) {
        Shape newShape = Shape.Copy();
        for (int i = 0; i < elementList.Count; i++) { 
          newShape.MoveRight(Space);
        }

      elementList.Add(animationBuilder.Animate(Name + elementCount, newShape, newShape, animationBuilder.Env.Now, animationBuilder.Env.Now, Fill, Stroke, StrokeWidth, true));
      }
    }

    public void Dequeue() {
      if (elementCount > 0) {
        elementCount--;
        if (elementCount < MaxLength) {
          Animation removeElement = elementList[elementCount - 1];
          elementList.Remove(removeElement);
          removeElement.Update(Shape, Shape, animationBuilder.Env.Now, animationBuilder.Env.Now, Fill, Stroke, StrokeWidth, false);
        }
      }
    }
  }
}
