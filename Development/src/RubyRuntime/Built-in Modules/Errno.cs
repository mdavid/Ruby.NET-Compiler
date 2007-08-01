/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;
using System.Net.Sockets;

namespace Ruby
{

    public partial class Errno
    {
        internal static int errno = 0;
        internal static System.Collections.Generic.Dictionary<int, Class> syserr_tbl = new System.Collections.Generic.Dictionary<int, Class>();

        internal static Class set_syserr(int n, string name)
        {
            Class error;

            if (!syserr_tbl.TryGetValue(n, out error))
            {
                error = Class.rb_define_class_under(Ruby.Runtime.Init.rb_mErrno, name, Ruby.Runtime.Init.rb_eSystemCallError, null); // need caller
                error.define_const("Errno", n);

                syserr_tbl.Add(n, error);
            }
            else
            {
                Ruby.Runtime.Init.rb_mErrno.define_const(name, error);
            }
            return error;
        }

        internal const int EPERM = 1;
        internal const int ENOENT = 2;
        internal const int ESRCH = 3;
        internal const int EINTR = 4;
        internal const int EIO = 5;
        internal const int ENXIO = 6;
        internal const int E2BIG = 7;
        internal const int ENOEXEC = 8;
        internal const int EBADF = 9;
        internal const int ECHILD = 10;
        internal const int EAGAIN = 11;
        internal const int ENOMEM = 12;
        internal const int EACCES = 13;
        internal const int EFAULT = 14;
        internal const int EBUSY = 16;
        internal const int EEXIST = 17;
        internal const int EXDEV = 18;
        internal const int ENODEV = 19;
        internal const int ENOTDIR = 20;
        internal const int EISDIR = 21;
        internal const int EINVAL = 22;
        internal const int ENFILE = 23;
        internal const int EMFILE = 24;
        internal const int ENOTTY = 25;
        internal const int EFBIG = 27;
        internal const int ENOSPC = 28;
        internal const int ESPIPE = 29;
        internal const int EROFS = 30;
        internal const int EMLINK = 31;
        internal const int EPIPE = 32;
        internal const int EDOM = 33;
        internal const int ERANGE = 34;
        internal const int EDEADLK = 36;
        internal const int ENAMETOOLONG = 38;
        internal const int ENOLCK = 39;
        internal const int ENOSYS = 40;
        internal const int ENOTEMPTY = 41;
        internal const int EILSEQ = 42;
        internal const int EDEADLOCK = EDEADLK;

        internal const int EWOULDBLOCK = (int)SocketError.WouldBlock;                  // WSAEWOULDBLOCK;
        internal const int EINPROGRESS = (int)SocketError.InProgress;                  // WSAEINPROGRESS;
        internal const int EALREADY = (int)SocketError.AlreadyInProgress;           // WSAEALREADY;
        internal const int ENOTSOCK = (int)SocketError.NotSocket;                   // WSAENOTSOCK;
        internal const int EDESTADDRREQ = (int)SocketError.DestinationAddressRequired;  // WSAEDESTADDRREQ;
        internal const int EMSGSIZE = (int)SocketError.MessageSize;                 // WSAEMSGSIZE;
        internal const int EPROTOTYPE = (int)SocketError.ProtocolType;                // WSAEPROTOTYPE;
        internal const int ENOPROTOOPT = (int)SocketError.ProtocolOption;              // WSAENOPROTOOPT;
        internal const int EPROTONOSUPPORT = (int)SocketError.ProtocolNotSupported;        // WSAEPROTONOSUPPORT;
        internal const int ESOCKTNOSUPPORT = (int)SocketError.SocketNotSupported;          // WSAESOCKTNOSUPPORT;
        internal const int EOPNOTSUPP = (int)SocketError.OperationNotSupported;       // WSAEOPNOTSUPP;
        internal const int EPFNOSUPPORT = (int)SocketError.ProtocolFamilyNotSupported;  // WSAEPFNOSUPPORT;
        internal const int EAFNOSUPPORT = (int)SocketError.AddressFamilyNotSupported;   // WSAEAFNOSUPPORT;
        internal const int EADDRINUSE = (int)SocketError.AddressAlreadyInUse;         // WSAEADDRINUSE;
        internal const int EADDRNOTAVAIL = (int)SocketError.AddressNotAvailable;         // WSAEADDRNOTAVAIL;
        internal const int ENETDOWN = (int)SocketError.NetworkDown;                 // WSAENETDOWN;
        internal const int ENETUNREACH = (int)SocketError.NetworkUnreachable;          // WSAENETUNREACH;
        internal const int ENETRESET = (int)SocketError.NetworkReset;                // WSAENETRESET;
        internal const int ECONNABORTED = (int)SocketError.ConnectionAborted;           // WSAECONNABORTED;
        internal const int ECONNRESET = (int)SocketError.ConnectionReset;             // WSAECONNRESET;
        internal const int ENOBUFS = (int)SocketError.NoBufferSpaceAvailable;      // WSAENOBUFS;
        internal const int EISCONN = (int)SocketError.IsConnected;                 // WSAEISCONN;
        internal const int ENOTCONN = (int)SocketError.NotConnected;                // WSAENOTCONN;
        internal const int ESHUTDOWN = (int)SocketError.Shutdown;                    // WSAESHUTDOWN;
        internal const int ETOOMANYREFS = (int)SocketError.TooManyOpenSockets;          // WSAETOOMANYREFS;
        internal const int ETIMEDOUT = (int)SocketError.TimedOut;                    // WSAETIMEDOUT;
        internal const int ECONNREFUSED = (int)SocketError.ConnectionRefused;           // WSAECONNREFUSED;
        internal const int EHOSTDOWN = (int)SocketError.HostDown;                    // WSAEHOSTDOWN;
        internal const int EHOSTUNREACH = (int)SocketError.HostUnreachable;             // WSAEHOSTUNREACH;
        internal const int EPROCLIM = (int)SocketError.ProcessLimit;                // WSAEPROCLIM;

        internal const int ELOOP = 10062; // WSAELOOP;
        internal const int EUSERS = 10068; // WSAEUSERS;
        internal const int EDQUOT = 10069; // WSAEDQUOT;
        internal const int ESTALE = 10070; // WSAESTALE;
        internal const int EREMOTE = 10071; // WSAEREMOTE;
    }
}
