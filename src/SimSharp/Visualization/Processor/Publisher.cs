using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace SimSharp.Visualization.Processor {
  public class Publisher : FramesProcessor{
    private StringWriter stringWriter;
    private JsonTextWriter writer;
    private IModel model;

    public Publisher() {
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);

      var connectionFactory = new ConnectionFactory() {
        UserName = "guest",
        Password = "guest",
        HostName = "localhost"
      };

      //Main entry point to the RabbitMQ .NET AMQP client
      var connection = connectionFactory.CreateConnection();
      model = connection.CreateModel();

      // Create Exchange
      model.ExchangeDeclare("simSharpExchange", ExchangeType.Direct);

      // Create Queue
      model.QueueDeclare("simSharpQueue", true, false, false, null);

      // Bind Queue to Exchange
      model.QueueBind("simSharpQueue", "simSharpExchange", "directexchange_key");
    }

    public void SendStart(AnimationConfig config) {
      writer.WriteStartObject();

      writer.WritePropertyName("start");
      writer.WriteValue(true);
      config.WriteJson(writer);

      writer.WriteEndObject();

      string frame = stringWriter.ToString();
      PublishFrame(frame);
      Flush();
    }

    public void SendFrame(string frame) {
      PublishFrame(frame);
    }

    public void SendStop() {
      writer.WriteStartObject();

      writer.WritePropertyName("stop");
      writer.WriteValue(true);

      writer.WriteEndObject();

      string frame = stringWriter.ToString();
      PublishFrame(frame);
      Flush();
    }

    private void PublishFrame(string frame) {
      var properties = model.CreateBasicProperties();
      properties.Persistent = false;

      byte[] messagebuffer = Encoding.Default.GetBytes(frame);
      model.BasicPublish("simSharpExchange", "directexchange_key", properties, messagebuffer);
    }

    private void Flush() {
      writer.Flush();
      StringBuilder sb = stringWriter.GetStringBuilder();
      sb.Remove(0, sb.Length);
    }
  }
}
