using System.Runtime.InteropServices;

namespace TalosDownpatcher {
  public class DateUtils {
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

    public static void SetYears(short delta) {
      SystemTime time = new SystemTime();
      GetSystemTime(ref time);
      time.wYear += delta;
      SetSystemTime(ref time);
    }
  }
}