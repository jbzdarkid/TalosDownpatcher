﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using TalosDownpatcher.Properties;

namespace TalosDownpatcher {
  class Logging : IDisposable {
    private static Logging instance;
    private string buffer;
    private int buffSize;
    private readonly FileStream fs;
    private readonly StreamWriter sw;
    private readonly string pid;

    public static void Init() { if (instance == null) instance = new Logging(); }
    private Logging() {
      AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
      fs = new FileStream(Settings.Default.oldVersionLocation + "/TalosDownpatcher.log" , FileMode.Append, FileAccess.Write, FileShare.Write);
      sw = new StreamWriter(fs);
      pid = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
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
      lock(instance) {
        instance.LogInternal(message);
      }
    }

    private void LogInternal(string message) {
      message = pid + "\t" + DateTime.Now.ToString("yyyy/mm/dd hh:mm:ss.fff\t: ", CultureInfo.InvariantCulture) + message + "\n";
      Console.Write(message);

      buffer += message;
      if (++buffSize >= 5) Flush();
    }

    public static void MessageBox(string message, string title) {
      Log(message);
      System.Windows.MessageBox.Show(message, title);
    }
  }
}
