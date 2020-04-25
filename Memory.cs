using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TalosDownpatcher {
  internal static class NativeMethods {
    [DllImport("kernel32.dll")]
    internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);
  }

  internal static class Memory {
    public static void FindAndReplace(string processName, byte[] search, byte[] replace) {
      foreach (var process in Process.GetProcesses()) {
        if (process.ProcessName != processName) continue;
        foreach (ProcessModule module in process.Modules) {
          if (module.ModuleName != "steamclient.dll") continue;

          FindAndReplaceInternal(process, module, search, replace);
          return;
        }
      }
    }

    private static void FindAndReplaceInternal(Process process, ProcessModule module, byte[] search, byte[] replace) {
      var data = new byte[0x1100];
      for (IntPtr i = module.BaseAddress; (long)i < (long)(module.BaseAddress + module.ModuleMemorySize); i += 0x1000) {
        if (!NativeMethods.ReadProcessMemory(process.Handle, i, data, data.Length, out _)) {
          continue; // Ignore read errors by proceeding
        }

        int index = Utils.Find(data, search);
        if (index == -1) continue;

        // Always assume write succeeds
        NativeMethods.WriteProcessMemory(process.Handle, i + index, replace, replace.Length, out _);
        break;
      }
    }
  }
}
