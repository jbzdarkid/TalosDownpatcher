using System;

namespace TalosDownpatcher {
  class Logging {
    public static void Log(string message) {
#if DEBUG
      Console.Write(message + "\n");
#else
      System.Diagnostics.Trace(message);
#endif
    }

    public static void MessageBox(string message, string title) {
      Log(message);
      System.Windows.MessageBox.Show(message, title);
    }
  }
}
