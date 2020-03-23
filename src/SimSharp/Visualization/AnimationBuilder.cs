using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization {
  public class AnimationBuilder {
    private AnimationProperties props;
    private DateTime startDate;
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private List<Animation> animations;
    private long currFrame;

    public AnimationBuilder(AnimationProperties props, DateTime startDate) {
      this.props = props;
      this.startDate = startDate;
      this.currFrame = 0;
    }

    public void StartBuilding() {
      stringWriter = new StringWriter();
      writer = new JsonTextWriter(stringWriter);

      writer.WriteStartObject();

      writer.WritePropertyName("name");
      writer.WriteValue(props.Name);

      writer.WritePropertyName("timeStep");
      writer.WriteValue(props.TimeStep);

      writer.WritePropertyName("frames");
      writer.WriteStartArray();
    }

    public void AddAnimation(Animation animation) {
      animations.Add(animation);
    }

    public void Step(DateTime now) {
      double frame = (now - startDate).TotalSeconds / props.TimeStep;

      List<IEnumerator<FrameObjekt>> states = new List<IEnumerator<FrameObjekt>>(animations.Count);
      foreach(Animation animation in animations) {
        IEnumerator<FrameObjekt> statesEnum = animation.StatesUntil(Convert.ToInt32(frame));
        if (statesEnum != null)
          states.Add(statesEnum);
      }

      for (int i = 0; i <= frame - currFrame; i++) {
        foreach(IEnumerator<FrameObjekt> stateEnum in states) {
          FrameObjekt obj = stateEnum.Current;
          stateEnum.MoveNext();


        }
      }
    }

    private void StopBuilding() {
      writer.WriteEndArray();
      writer.WriteEndObject();
    }

    public void WriteJson() {
      throw new NotImplementedException();
    }
  }
}