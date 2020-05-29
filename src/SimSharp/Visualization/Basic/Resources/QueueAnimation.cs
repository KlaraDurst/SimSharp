using System;
using System.Collections.Generic;
using System.Text;
using SimSharp.Visualization.Basic.Shapes;

namespace SimSharp.Visualization.Basic.Resources {
  public class QueueAnimation {
    public enum QueueOrientation { North, East, South, West }

    public string Name { get; }
    public Shape Shape { get; }
    public string Fill { get; }
    public string Stroke { get; }
    public int StrokeWidth { get; }
    public int Space { get; }
    public int MaxLength { get; }
    public QueueOrientation Orientation { get; }

    private AnimationBuilder animationBuilder;
    private List<Animation> elementList;
    private int elementCount;
    private int totalCount;

    public QueueAnimation(string name, Shape shape, string fill, string stroke, int strokeWidth, int space, int maxLength, AnimationBuilder animationBuilder, QueueOrientation orientation) {
      Name = name;
      Shape = shape;
      Fill = fill;
      Stroke = stroke;
      StrokeWidth = strokeWidth;
      Space = space;
      MaxLength = maxLength;
      Orientation = orientation;

      this.animationBuilder = animationBuilder;
      this.elementList = new List<Animation>(MaxLength);
      this.elementCount = 0;
      this.totalCount = 0;
    }

    public void Enqueue() {
      if (elementCount < MaxLength) {
        Shape newShape = Shape.Copy();

        switch (Orientation) {
          case QueueOrientation.North: {
              newShape.MoveUp(Space * elementCount);
              break;
            }
          case QueueOrientation.East: {
              newShape.MoveRight(Space*elementCount);
              break;
            }
          case QueueOrientation.South: {
              newShape.MoveDown(Space * elementCount);
              break;
            }
          case QueueOrientation.West: {
              newShape.MoveLeft(Space * elementCount);
              break;
            }
          default: break;
        }

        elementList.Add(animationBuilder.Animate(Name + totalCount, newShape, Fill, Stroke, StrokeWidth, true));
      }
      elementCount++;
      totalCount++;
    }

    public void Dequeue() {
      if (elementCount > 0) {
        if (elementCount <= MaxLength) {
          Animation removeElement = elementList[elementCount - 1];
          elementList.Remove(removeElement);
          removeElement.Update(Shape, Fill, Stroke, StrokeWidth, false);
        }
        elementCount--;
      }
    }
  }
}
