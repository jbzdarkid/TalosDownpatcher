using System.Runtime.InteropServices;

namespace TalosDownpatcher {
  public static class DateUtils {
    public static void SetYears(short delta) {
      SafeNativeMethods.SystemTime time = new SafeNativeMethods.SystemTime();
      SafeNativeMethods.GetSystemTime(ref time);
      time.wYear += delta;
      SafeNativeMethods.SetSystemTime(ref time);
    }
  }

  internal static class SafeNativeMethods {
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime {
      public short wYear;
      public short wMonth;
      public short wDayOfWeek;
      public short wDay;
      public short wHour;
      public short wMinute;
      public short wSecond;
      public short wMilliseconds;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetSystemTime(ref SystemTime systemTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetSystemTime(ref SystemTime systemTime);

  }
}