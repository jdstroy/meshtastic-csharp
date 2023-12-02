using Meshtastic.Data.MessageFactories;
using Microsoft.Extensions.Logging;
using SharpTun.Implementation.Wintun;
using SharpTun.Implementation.Wintun.NativeLink;
using SharpTun.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Meshtastic.Cli.Utilities
{
    internal class TunManager
    {
        readonly ManagedWintunAdapter adapter = SharpTun.Implementation.Wintun.ManagedWintunAdapter.Create("Meshtastic", "WinTun", Guid.Parse("de373def-43f6-484e-a87f-48e591e698fe"));
        readonly ITunSession session;
        private DeviceConnectionContext context;
        private CommandContext commandContext;
        private bool running = true;
        public ILogger Logger;

        public delegate void OnPacketReceive(byte[] packet);

        public event OnPacketReceive ReceiveEvent = packet => { };

        public TunManager(DeviceConnectionContext context, CommandContext commandContext)
        {
            this.context = context;
            this.commandContext = commandContext;
            this.Logger = commandContext.Logger;

            Logger.LogInformation($"TUN starting.");
            ShrarpWinTunLogger.SetLogger(SharpTunLogMessage);
            // Ring size needs to be large
            session = adapter.Start(0x400000);
        }

        internal void SharpTunLogMessage(WINTUN_LOGGER_LEVEL level, UInt64 timestamp, string message)
        {
            Logger.LogInformation($"#TUN - {message}");
        }


        private void PacketLoop()
        {
            while (running)
            {
                Logger.LogTrace("Receiving TUN datagram");
                // write packet to network
                byte[] packet = session.ReceivePacket();
                
                ReceiveEvent(packet);
            }
        }

        public async void Start()
        {
            await Task.Run(PacketLoop);
        }

        internal void SendPacket(byte[] packet)
        {
            session.SendPacket(packet);
        }
    }
}