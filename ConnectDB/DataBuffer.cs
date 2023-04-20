using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectDB
{
    class DataBuffer
    {
        public static string userName;
        public static string userPassword;
        static DataBuffer()
        {
            userName = String.Empty;
            userPassword = String.Empty;
        }
    }
}
