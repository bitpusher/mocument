using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace Mocument.IPC
{
    [StructLayout(LayoutKind.Sequential)]
    public class SECURITY_ATTRIBUTES : IDisposable
    {
        internal int nLength;
        internal IntPtr lpSecurityDescriptor;
        internal int bInheritHandle;

        public static SECURITY_ATTRIBUTES GetNullDacl()
        {
            // Build NULL DACL (Allow everyone full access)
            var gsd = new RawSecurityDescriptor(ControlFlags.DiscretionaryAclPresent, null, null, null, null);

            // Construct SECURITY_ATTRIBUTES structure
            var sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(typeof (SECURITY_ATTRIBUTES));
            sa.bInheritHandle = 1;

            // Get binary form of the security descriptor and copy it into place
            var desc = new byte[gsd.BinaryLength];
            gsd.GetBinaryForm(desc, 0);
            sa.lpSecurityDescriptor = Marshal.AllocHGlobal(desc.Length);
                // This Alloc is Freed by the Disposer or Finalizer
            Marshal.Copy(desc, 0, sa.lpSecurityDescriptor, desc.Length);

            return sa;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (lpSecurityDescriptor != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(lpSecurityDescriptor);
                    lpSecurityDescriptor = IntPtr.Zero;
                }
            }
        }

        ~SECURITY_ATTRIBUTES()
        {
            Dispose();
        }
    }
}