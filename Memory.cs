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

  internal class Memory : IDisposable {
    private readonly Process _process;
    private readonly ProcessModule _module;

    public Memory(string processName) {
      foreach (var process in Process.GetProcesses()) {
        if (process.ProcessName == processName) {
          _process = process;
          foreach (ProcessModule module in _process.Modules) {
            if (module.ModuleName == "steamclient.dll") {
              _module = module;
              break;
            }
          }
          break;
        }
      }
      if (_process == null || _module == null) return; // TODO: Some kind of error
    }

    private static int Find(byte[] data, byte[] search, int startIndex = 0) {
      for (int i=startIndex; i<data.Length - search.Length; i++) {
          bool match = true;
          for (int j=0; j<search.Length; j++) {
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

    public void FindAndReplace(byte[] search, byte[] replace) {
      var data = new byte[0x1100];
      for (IntPtr i = _module.BaseAddress; (long)i < (long)(_module.BaseAddress + _module.ModuleMemorySize); i += 0x1000) {
        if (!NativeMethods.ReadProcessMemory(_process.Handle, i, data, data.Length, out _)) {
          continue;
        }

        int index = Find(data, search);
        if (index == -1) continue;

        NativeMethods.WriteProcessMemory(_process.Handle, i + index, replace, replace.Length, out _);
        break;
      }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          _process.Dispose();
        }

        disposedValue = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }
    #endregion
  }
}
