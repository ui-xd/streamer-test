using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static DevSim.Win32.ADVAPI32;
using static DevSim.Win32.User32;

namespace DevSim.Win32
{
    // TODO: Use https://github.com/dotnet/pinvoke for all p/invokes.  Remove signatures from this project.
    public class Win32Interop
    {
        private static IntPtr _lastInputDesktop;

        public static bool GetCurrentDesktop(out string desktopName)
        {
            var inputDesktop = OpenInputDesktop();
            try
            {
                byte[] deskBytes = new byte[256];
                if (!GetUserObjectInformationW(inputDesktop, UOI_NAME, deskBytes, 256, out uint lenNeeded))
                {
                    desktopName = string.Empty;
                    return false;
                }

                desktopName = Encoding.Unicode.GetString(deskBytes.Take((int)lenNeeded).ToArray()).Replace("\0", "");
                return true;
            }
            finally
            {
                CloseDesktop(inputDesktop);
            }
        }

        public static IntPtr OpenInputDesktop()
        {
            return User32.OpenInputDesktop(0, true, ACCESS_MASK.GENERIC_ALL);
        }

        public static void SetMonitorState(MonitorState state)
        {
            SendMessage(0xFFFF, 0x112, 0xF170, (int)state);
        }

        public static MessageBoxResult ShowMessageBox(IntPtr owner,
            string message,
            string caption,
            MessageBoxType messageBoxType)
        {
            return (MessageBoxResult)MessageBox(owner, message, caption, (long)messageBoxType);
        }

        public static bool SwitchToInputDesktop()
        {
            try
            {
                CloseDesktop(_lastInputDesktop);
                var inputDesktop = OpenInputDesktop();

                if (inputDesktop == IntPtr.Zero)
                {
                    return false;
                }

                var result = SetThreadDesktop(inputDesktop);
                var errCode = Marshal.GetLastWin32Error();
                result = SwitchDesktop(inputDesktop);
                _lastInputDesktop = inputDesktop;
                
                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}
