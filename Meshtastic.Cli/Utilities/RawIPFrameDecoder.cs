using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using System.Text;
using System.Threading.Tasks;
using Meshtastic.Cli.CommandHandlers;
using System.Diagnostics;

namespace Meshtastic.Cli.Utilities
{
    internal class RawIPFrameDecoder
    {
        private ILogger logger;

        public RawIPFrameDecoder(ILogger logger)
        {
            this.logger = logger;
        }

        internal DecodedPacketBuffer AllocateBuffer()
        {
            return new DecodedPacketBuffer();
        }

        internal bool Filter(DecodedPacketBuffer processedPacket)
        {
            if (processedPacket.protocolVersion != 4)
            {
                return true;
            }
            switch (processedPacket.ipDatagram.protocol)
            {
                case 0x1: // ICMP
                case 0x2: // IGMP
                case 0x80: // Service-Specific Connection-Oriented Protocol in a Multilink and Connectionless Environment
                    return true;
                default:
                    break;
            }

            if (processedPacket.ipDatagram.Filter())
            {
                return true;
            }

            return false;
        }

        internal void ProcessPacket(ByteString packet, DecodedPacketBuffer receivedBuffer)
        {
            byte protocolVersion = packet.First();
            protocolVersion >>= 4;

            receivedBuffer.protocolVersion = protocolVersion;

            receivedBuffer.packet = packet.ToBase64();

            const int ipPayloadStart = 20;

            if (protocolVersion == 4)
            {
                var protocol = packet.Span[8 + 1];
                var sourceAddress = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(packet.Span.Slice(12)));
                var destinationAddress = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(packet.Span.Slice(16)));
                var payload = packet.Span.ToArray();

                IPDatagram ipDatagram;

                if (protocol == 0x1) // ICMP
                {
                    ipDatagram = new IcmpDatagram
                    {
                        protocol = protocol,
                        sourceAddress = sourceAddress,
                        destinationAddress = destinationAddress,
                        payload = payload,
                        icmpType = packet.Span[ipPayloadStart],
                        icmpCode = packet.Span[ipPayloadStart + 1],
                        icmpChecksum = IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(packet.Span.Slice(ipPayloadStart + 2))),
                    };
                }
                else if (protocol == 0x11) // UDP
                {
                    ipDatagram = new UdpDatagram
                    {
                        protocol = protocol,
                        sourceAddress = sourceAddress,
                        destinationAddress = destinationAddress,
                        payload = payload,
                        sport = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet.Span.Slice(ipPayloadStart))),
                        dport = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet.Span.Slice(ipPayloadStart + 2)))
                    };
                }
                else if (protocol == 0x6) // TCP
                {
                    ipDatagram = new TcpSegment
                    {
                        protocol = protocol,
                        sourceAddress = sourceAddress,
                        destinationAddress = destinationAddress,
                        payload = payload,
                        sport = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet.Span.Slice(ipPayloadStart))),
                        dport = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(packet.Span.Slice(ipPayloadStart + 2)))
                    };
                }
                else
                {
                    ipDatagram = new IPDatagram()
                    {
                        protocol = protocol,
                        sourceAddress = sourceAddress,
                        destinationAddress = destinationAddress,
                        payload = payload,
                    };
                }

                receivedBuffer.ipDatagram = ipDatagram;
            }
        }

        internal void Show(string prefix, DecodedPacketBuffer processedPacket)
        {
            logger.LogTrace($"{prefix}R| {processedPacket.packet}");
            logger.LogTrace($"{prefix}D| af = {processedPacket.protocolVersion}");
            processedPacket.ipDatagram.Show(prefix, logger);
        }
    }

    internal class DecodedPacketBuffer
    {
        internal byte protocolVersion { get; set; }
        internal IPDatagram ipDatagram;
        internal string packet = "";
    }

    internal class IcmpDatagram : IPDatagram
    {
        public byte icmpType { get; set; }
        public byte icmpCode { get; set; }
        public int icmpChecksum { get; set; }

        public override void Show(string prefix, ILogger logger)
        {
            base.Show(prefix, logger);
            logger.LogTrace($"{prefix}D| proto = icmp, type = {icmpType}, code = {icmpCode}");
        }
    }


    public class UdpDatagram : IPDatagram
    {
        public short sport { get; set; }
        public short dport { get; set; }
        public override void Show(string prefix, ILogger logger)
        {
            base.Show(prefix, logger);
            logger.LogTrace($"{prefix}D| proto = udp, sport = {sport & 0xffff}, dport = {dport & 0xffff}");
        }

        public override bool Filter()
        {
            return (dport == 1900 || dport == 5353 || dport == 137 || dport == 135 || dport == 5355);
        }
    }
    public class TcpSegment : IPDatagram
    {
        public short sport { get; set; }
        public short dport { get; set; }

        public override void Show(string prefix, ILogger logger)
        {
            base.Show(prefix, logger);
            logger.LogTrace($"{prefix}D| proto = tcp, sport = {sport & 0xffff}, dport = {dport & 0xffff}");
        }
    }
    public class IPDatagram
    {
        public byte protocol { get; set; }
        public int sourceAddress { get; set; }
        public int destinationAddress { get; set; }

        public byte[] payload = new byte[0];

        public virtual void Show(string prefix, ILogger logger)
        {
            logger.LogTrace(
                $"{prefix}D| src = {sourceAddress.ToString("X8")}, dst = {destinationAddress.ToString("X8")}");
        }

        public virtual bool Filter()
        {
            return false;
        }
    }
}
