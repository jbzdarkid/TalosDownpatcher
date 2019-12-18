using System;
using System.IO;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  class Logging : IDisposable {
    private static Logging instance;
    private string buffer;
    private int buffSize;
    private readonly FileStream fs;
    private readonly BinaryWriter sw;

    public static void Init() { if (instance == null) instance = new Logging(); }
    private Logging() {
      AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
      fs = new FileStream(Settings.Default.oldVersionLocation + "/TalosDownpatcher.log" , FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
      sw = new BinaryWriter(fs);
    }

    private void OnProcessExit(object sender, EventArgs e) {
      Dispose();
    }

    public void Dispose() {
      Flush();
      sw.Close();
      fs.Close();
    }

    private void Flush() {
      sw.Write(buffer);
      buffer = "";
      buffSize = 0;
    }

    public static void Log(string message) {
      message += "\n";
      Console.Write(message);

      lock(instance) {
        instance.buffer += message;
        if (++instance.buffSize >= 5) instance.Flush();
      }
    }

    public static void MessageBox(string message, string title) {
      Log(message);
      System.Windows.MessageBox.Show(message, title);
    }
  }
}
