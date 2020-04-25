using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace TalosDownpatcher {
  internal static class Utils {
    /// <summary>
    /// Removes all event handlers subscribed to the specified routed event from the specified element.
    /// https://stackoverflow.com/a/16392387
    /// </summary>
    /// <param name="element">The UI element on which the routed event is defined.</param>
    /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
    internal static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent) {
      if (element == null) return;

      // Get the EventHandlersStore instance which holds event handlers for the specified element.
      // The EventHandlersStore class is declared as internal.
      var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
          "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
      object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

      if (eventHandlersStore == null) return;

      // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
      // for getting an array of the subscribed event handlers.
      var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
          "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
          eventHandlersStore, new object[] { routedEvent });

      // Iteratively remove all routed event handlers from the element.
      foreach (var routedEventHandler in routedEventHandlers) {
        element.RemoveHandler(routedEvent, routedEventHandler.Handler);
      }
    }

    internal static int Find(byte[] data, byte[] search, int startIndex = 0) {
      for (int i = startIndex; i < data.Length - search.Length; i++) {
        bool match = true;
        for (int j = 0; j < search.Length; j++) {
          if (data[i + j] == search[j]) {
            continue;
          }
          match = false;
          break;
        }
        if (match) return i;
      }
      return -1;
    }

    internal static long GetFolderSize(string folder) {
      long size = 0;
      var src = new DirectoryInfo(folder);
      if (src.Exists) {
        var files = src.GetFiles("*", SearchOption.AllDirectories);
        foreach (var file in files) size += file.Length;
      }
      return size;
    }

    // https://stackoverflow.com/a/1310148
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void RunAsync(Action func) {
      var thread = new Thread(() => { func(); });
      thread.Name = new StackFrame(1, true).GetMethod().Name;
      thread.IsBackground = true;
      thread.Start();
    }

    /// <summary>
    /// Fast file move with big buffers
    /// Author: Frank T. Clark
    /// Downloaded from: https://www.codeproject.com/Tips/777322/A-Faster-File-Copy
    /// License: CPOL
    /// Modifications:
    ///   - Formatted code
    ///   - Removed trailing "Delete"
    ///   - Renamed variables
    ///   - Changed from move to copy (overwrite implied)
    ///   - Add callback for progress indicator
    ///   - Added automatic folder creation for copying to non-existant paths
    /// </summary>
    /// <param name="source">Source file path</param> 
    /// <param name="destination">Destination file path</param> 
    /// <param name="onCopyBytes">Callback to fire after copying bytes (used for progress bars)</param>
    [SuppressMessage("Style", "IDE1005", Justification = "The if check is relevant, as onCopyBytes may be null.")]
    internal static void FCopy(string source, string destination, Action<long> onCopyBytes = null) {
      int buffSize = (1 << 20); // 1 MB
      byte[] buff = new byte[buffSize];
      using (FileStream fsread = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None, buffSize)) {
        using (BinaryReader bwread = new BinaryReader(fsread)) {
          new FileInfo(destination).Directory.Create(); // Ensure target folder exists
          using (FileStream fswrite = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, buffSize)) {
            using (BinaryWriter bwwrite = new BinaryWriter(fswrite)) {
              while (true) {
                int readBytes = bwread.Read(buff, 0, buffSize);
                if (readBytes == 0) break;
                bwwrite.Write(buff, 0, readBytes);
                if (onCopyBytes != null) onCopyBytes(readBytes);
              }
            }
          }
        }
      }
    }
  }
}
