using Google.Protobuf;
using Meshtastic.Protobufs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meshtastic.Data.MessageFactories
{
    public class IPv4DatagramFactory
    {
        private readonly DeviceStateContainer container;
        private readonly uint? dest;

        public IPv4DatagramFactory(DeviceStateContainer container, uint? dest = null)
        {
            this.container = container;
            this.dest = dest;
        }

        public MeshPacket CreateIPv4Datagram(byte[] datagram, uint channel = 0)
        {
            return new MeshPacket()
            {
                Channel = channel,
                WantAck = false, // Don't ACK -- handle this in the higher layers
                To = dest ?? 0xffffffff, // Default to broadcast
                Id = (uint)Math.Floor(Random.Shared.Next() * 1e9),
                HopLimit = 1,// container?.GetHopLimitOrDefault() ?? 3,
                Decoded = new Protobufs.Data()
                {
                    Portnum = PortNum.IpTunnelApp,
                    Payload = ByteString.CopyFrom(datagram),
                },
            };
        }
    }
}
