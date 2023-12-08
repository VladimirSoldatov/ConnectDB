using System;
using System.Text;
using System.Net;

namespace ConnectDB
{
    class Network
    {
        // To Use:
        // Network.PTRResult = Network.PTRLookup(127.0.0.1);
        // Network.PTRResult.Item1 is boolean true/false (pass/fail) for the test
        // Network.PTRResult.Item2 is the String of the original IP Address
        // Network.PTRResult.Item3 is the String of the Reverse Host for the IP Address
        // Network.PTRResult.Item4 is the String of the Forward IP Address assosciated with the Reverse Host
        // Network.PTRResult.Item5 is the String of the Forward Host the Forward IP Lookup
        //
        //
        // If the Query IP and the Forward IP match, and the Reverse and the Forward host match then the test will pass

        public static Tuple <Boolean, String, String, String, String> PTRResult = null;
 
        public static Tuple<Boolean, String, String, String, String> PTRLookup(String QueryIP)
        {
            Boolean Test = false;
            String ReverseHost = string.Empty;
            String ForwardIP = string.Empty;
            String ForwardHost = string.Empty;
            try
            {
                ReverseHost = Dns.GetHostEntry(QueryIP).HostName;
                ForwardIP = Dns.GetHostEntry(ReverseHost).AddressList[0].MapToIPv4().ToString();
                ForwardHost = Dns.GetHostEntry(ForwardIP).HostName;
            }
            catch (Exception)
            { }

            if (ReverseHost == string.Empty)
                ReverseHost = "No Reverse Host Found" ;
            if (ForwardIP == string.Empty)
                ForwardIP = "No Forward IP Found";
            if (ForwardHost == string.Empty)
                ForwardHost = "No Forward Host Found";
            if (QueryIP == ForwardIP &  ReverseHost == ForwardHost & ForwardHost != "No Forward Host Found")
            Test = true;

            return Tuple.Create(Test, QueryIP, ReverseHost, ForwardIP, ForwardHost);
        }

    }
}
