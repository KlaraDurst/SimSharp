using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SimSharp.Visualization.Processor {
  public class HtmlPlayer : JsonWriter{
    private string playerPath = Path.Combine(Directory.GetParent(System.Environment.CurrentDirectory).Parent.Parent.Parent.ToString(), @"SimSharp\Visualization\Processor\Templates");
    private string jsFile = @"js\scripts.js";
    private string htmlFile = "Player.html";

    public override void SendStop() {
      writer.WriteEndArray();
      writer.WriteEndObject();

      Play(stringWriter.ToString());
    }

    public void Play(string data) {
      string targetPlayerPath = Path.Combine(target, "Player");

      CopyDirectory(playerPath, targetPlayerPath);

      using (StreamWriter sw = File.AppendText(Path.Combine(targetPlayerPath, jsFile))) {
        sw.WriteLine("data = '" + data + "';");
      }

      OpenUrl(Path.Combine(targetPlayerPath, htmlFile));
    }

    private void CopyDirectory(string source, string dest) {
      DirectoryInfo dir = new DirectoryInfo(source);
      DirectoryInfo[] dirs = dir.GetDirectories();
      FileInfo[] files = dir.GetFiles();

      if (!Directory.Exists(dest)) {
        Directory.CreateDirectory(dest);
      }

      foreach (FileInfo file in files) {
        string temppath = Path.Combine(dest, file.Name);
        file.CopyTo(temppath, true);
      }
      
      foreach (DirectoryInfo subdir in dirs) {
        string temppath = Path.Combine(dest, subdir.Name);
        CopyDirectory(subdir.FullName, temppath);
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
