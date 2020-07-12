using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace SimSharp.Visualization.Processor {
  public class Publisher : FramesProcessor {
    protected string target = Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName;
    protected StringWriter stringWriter;
    protected JsonTextWriter writer;
    protected IModel model;
    protected IConnection connection;

    public Publisher() {
      this.stringWriter = new StringWriter();
      this.writer = new JsonTextWriter(stringWriter);

      int index = target.LastIndexOf(@"\SimSharp");
      string targetPlayerPath = Path.Combine(target.Substring(0, index + 10), @"src\SimSharp\Visualization\Processor\Consumer");

      var connectionFactory = new ConnectionFactory() {
        UserName = "guest",
        Password = "guest",
        HostName = "localhost"
      };

      //Main entry point to the RabbitMQ .NET AMQP client
      connection = connectionFactory.CreateConnection();
      model = connection.CreateModel();

      // Create Exchange
      model.ExchangeDeclare("simSharpExchange", ExchangeType.Direct);

      // Create Queue
      model.QueueDeclare("simSharpQueue", true, false, false, null);

      // Bind Queue to Exchange
      model.QueueBind("simSharpQueue", "simSharpExchange", "directexchange_key");

      Play(targetPlayerPath);

      // TODO: Batchvorgang abbrechen
    }

    private void Play(string targetPlayerPath) {
      NpmStart(targetPlayerPath);
      OpenUrl("http://localhost:3000");
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

      connection.Close();
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

    private void NpmStart(string path) {
      try {
        // Windows
        path = path.Replace("&", "^&");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c cd {path} && npm start") { CreateNoWindow = true });
      } catch {
        try {
          // Linux
          throw new NotImplementedException();
        } catch {
          try {
            // OSX
            throw new NotImplementedException();
          } catch {
            throw;
          }
        }
      }
    }

    private void OpenUrl(string url) {
      try {
        System.Diagnostics.Process.Start(url);
      } catch {
        try {
          // Windows
          url = url.Replace("&", "^&");
          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        } catch {
          try {
            // Linux
            System.Diagnostics.Process.Start("xdg-open", url);
          } catch {
            try {
              // OSX
              System.Diagnostics.Process.Start("open", url);
            } catch {
              throw;
            }
          }
        }
      }
    }
  }
}
