using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Spectre.Console;
using System.Reflection;
using Meshtastic.Extensions;
using Google.Protobuf;
using System.Security.Cryptography;
using System.Net.Sockets;
using SharpTun;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Xml.Linq;
using System;
using Meshtastic.Cli.Utilities;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;

namespace Meshtastic.Cli.CommandHandlers;


public class TunnelCommandHandler : DeviceCommandHandler
{
    private readonly TunManager tun;
    private bool suppressChatty = true;

    public TunnelCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext)
    {
        bool debugLoop = false;
        while(debugLoop)
        {
            Task.Delay(10);
        }

        tun = new TunManager(context, commandContext);
    }

    
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        
        Connection.Disconnect();
        return container;
    }

    private void HandleRadioIncoming(FromRadio? fromRadio, DeviceStateContainer container)
    {
        RawIPFrameDecoder decoder = new RawIPFrameDecoder(Logger);
        var receivedBuffer = decoder.AllocateBuffer();
        if (fromRadio!.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Packet)
        {
            var fromRadioDecoded = new
            {
                Packet = fromRadio,
                PortNum = fromRadio.Packet?.Decoded?.Portnum,
                PayloadSize = fromRadio.Packet?.Decoded?.Payload.Length ?? 0,
                ReceivedAt = DateTime.Now,
            };

            if (fromRadio.Packet != null && fromRadio.Packet.Decoded.Portnum == PortNum.IpTunnelApp)
            {
                ByteString decodedPayload = fromRadio.Packet.Decoded.Payload;

                decoder.ProcessPacket(decodedPayload, receivedBuffer);
                decoder.Show("R", receivedBuffer);

                tun.SendPacket(decodedPayload.ToArray());
            };
        }

    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var datagramFactory = new IPv4DatagramFactory(container);
        RawIPFrameDecoder decoder = new RawIPFrameDecoder(Logger);
        var sendBuffer = decoder.AllocateBuffer();

        tun.ReceiveEvent += (packetBytes =>
        {
            if (suppressChatty)
            {
                ByteString packet = ByteString.CopyFrom(packetBytes);

                decoder.ProcessPacket(packet, sendBuffer);
                decoder.Show("S", sendBuffer);

                if (decoder.Filter(sendBuffer))
                {
                    //return;
                }
            }

            var ipv4Datagram = datagramFactory.CreateIPv4Datagram(packetBytes);
            Logger.LogTrace($"Sending IPv4 datagram...");

            Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(ipv4Datagram),
                 (fromRadio, container) =>
                 {
                     HandleRadioIncoming(fromRadio, container);
                     return Task.FromResult(fromRadio != null);
                 });
        });
        
        tun.Start();

        await Connection.ReadFromRadio((fromRadio, container) =>
        {
            HandleRadioIncoming(fromRadio, container);
            return Task.FromResult(false);
        });
    }
}
