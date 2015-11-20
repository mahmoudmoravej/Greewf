using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.IP
{
    public class IpAddressRange
    {
        readonly AddressFamily addressFamily;
        readonly byte[] lowerBytes;
        readonly byte[] upperBytes;

        public IpAddressRange(IPAddress lower, IPAddress upper)
        {
            // Assert that lower.AddressFamily == upper.AddressFamily

            this.addressFamily = lower.AddressFamily;
            this.lowerBytes = lower.GetAddressBytes();
            this.upperBytes = upper.GetAddressBytes();
        }

        public IpAddressRange(string lower, string upper)
            :this(IPAddress.Parse(lower), IPAddress.Parse(upper))
        {
        }

        public bool IsInRange(IPAddress address)
        {
            if (address.AddressFamily != addressFamily)
            {
                return false;
            }

            byte[] addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (int i = 0; i < this.lowerBytes.Length &&
                (lowerBoundary || upperBoundary); i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }

        public bool IsInRange(string address)
        {
            return this.IsInRange(IPAddress.Parse(address));
        }


        private static IpAddressRange _loopbackRange = new IpAddressRange("127.0.0.1", "127.255.255.254");//https://en.wikipedia.org/wiki/Localhost
        private static IpAddressRange _interanet1Range = new IpAddressRange("10.0.0.0", "10.255.255.255");//https://en.wikipedia.org/wiki/Private_network
        private static IpAddressRange _interanet2Range = new IpAddressRange("172.16.0.0", "172.31.255.255");//https://en.wikipedia.org/wiki/Private_network
        private static IpAddressRange _interanet3Range = new IpAddressRange("192.168.0.0", "192.168.255.255");//https://en.wikipedia.org/wiki/Private_network

        public static bool IsInternetIp(string ip)
        {
            var ipAddress = IPAddress.Parse(ip);
            return IsInternetIp(ipAddress);
        }

        public static bool IsInternetIp(IPAddress ip)
        {
            return
                !_loopbackRange.IsInRange(ip) &&
                !_interanet1Range.IsInRange(ip) &&
                !_interanet2Range.IsInRange(ip) &&
                !_interanet3Range.IsInRange(ip);
        }

        public static bool IsLoopbackIp(string ip)
        {
            var ipAddress = IPAddress.Parse(ip);
            return IsLoopbackIp(ip);
        }

        public static bool IsLoopbackIp(IPAddress ip)
        {
            return _loopbackRange.IsInRange(ip);
        }
    }
}
