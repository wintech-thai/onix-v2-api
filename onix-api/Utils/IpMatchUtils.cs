using System.Net;
using System.Net.Sockets;

namespace Its.Onix.Api.Utils
{
    public static class IpMatchUtils
    {
        /// <summary>
        /// Checks whether clientIp matches any entry in a comma-separated list of IPv4/IPv6
        /// addresses or CIDR ranges (e.g. "1.2.3.4,10.0.0.0/8,2001:db8::/32").
        /// </summary>
        public static bool IsIpInList(string? clientIp, string? commaSeparatedList)
        {
            if (string.IsNullOrWhiteSpace(clientIp) || string.IsNullOrWhiteSpace(commaSeparatedList))
            {
                return false;
            }

            if (!IPAddress.TryParse(clientIp.Trim(), out var clientAddress))
            {
                return false;
            }

            clientAddress = Normalize(clientAddress);

            var entries = commaSeparatedList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var entry in entries)
            {
                if (IsMatch(clientAddress, entry))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMatch(IPAddress clientAddress, string entry)
        {
            var parts = entry.Split('/', 2);
            var addressPart = parts[0].Trim();

            if (!IPAddress.TryParse(addressPart, out var entryAddress))
            {
                return false;
            }

            entryAddress = Normalize(entryAddress);

            if (parts.Length == 1)
            {
                return clientAddress.Equals(entryAddress);
            }

            if (!int.TryParse(parts[1].Trim(), out var prefixLength))
            {
                return false;
            }

            return IsInCidrRange(clientAddress, entryAddress, prefixLength);
        }

        // .NET returns IPv4-over-IPv6 sockets (e.g. Connection.RemoteIpAddress) as
        // "::ffff:x.x.x.x". Collapse those down to plain IPv4 so a blacklist entry
        // entered as "x.x.x.x" still matches instead of failing on AddressFamily.
        private static IPAddress Normalize(IPAddress address)
        {
            return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
        }

        private static bool IsInCidrRange(IPAddress address, IPAddress networkAddress, int prefixLength)
        {
            if (address.AddressFamily != networkAddress.AddressFamily)
            {
                return false;
            }

            var addressBytes = address.GetAddressBytes();
            var networkBytes = networkAddress.GetAddressBytes();

            var maxPrefixLength = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
            if (prefixLength < 0 || prefixLength > maxPrefixLength)
            {
                return false;
            }

            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;

            for (var i = 0; i < fullBytes; i++)
            {
                if (addressBytes[i] != networkBytes[i])
                {
                    return false;
                }
            }

            if (remainingBits > 0)
            {
                var mask = (byte)(0xFF << (8 - remainingBits));
                if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
