using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SimSharp.Visualization {
  public class AnimationUnit {
    public int Start { get; set;  }
    public int Stop { get; set;  }
    public List<string> Frames { get; set; }

    public AnimationUnit(int start, int stop, int frameNumber) {
      Start = start;
      Stop = stop;
      Frames = new List<string>(frameNumber);
    }

    public void AddFrame(string frame) {
      Frames.Add(frame);
    }

    public void AddFrameRange(IEnumerable<string> frames) {
      Frames.AddRange(frames);
    }
    
    public Dictionary<string, int[]> GetAttributesAt(int f, List<string> attributeNames) {
      Dictionary<string, int[]> attributes = new Dictionary<string, int[]>();

      for (int i = f - Start; i >= 0; i--) {
        if (attributes.Count < attributeNames.Count) {
          Dictionary<string, int[]> frame = GetAttributes(Frames[i]);
          foreach (string attr in attributeNames) {
            if (!attributes.ContainsKey(attr)) {
              if (frame.ContainsKey(attr)) {
                frame.TryGetValue(attr, out int[] value);
                attributes.Add(attr, value);
              }
            }
          }
        } else { // all values found
          break;
        }
      }
      return attributes;
    }

    private Dictionary<string, int[]> GetAttributes(string content) {
      string frame = "{" + content + "}";
      JsonTextReader reader = new JsonTextReader(new StringReader(frame));
      Dictionary<string, int[]> frameAttributes = new Dictionary<string, int[]>();
      string currentAttr = string.Empty;

      while(reader.Read()) {
        if (reader.Value != null) {
          if (reader.TokenType == JsonToken.PropertyName)
            currentAttr = reader.Value.ToString();

          else if (reader.TokenType == JsonToken.Integer)
            frameAttributes.Add(currentAttr, new int[] { int.Parse(reader.Value.ToString()) });

          else if (reader.TokenType == JsonToken.StartArray) {
            List<int> l = new List<int>();
            while (reader.TokenType == JsonToken.Integer) {
              l.Add(int.Parse(reader.Value.ToString()));
              reader.Read();
            }
            frameAttributes.Add(currentAttr, l.ToArray());
          }
        }
      }
      return frameAttributes;
    }
  }
}