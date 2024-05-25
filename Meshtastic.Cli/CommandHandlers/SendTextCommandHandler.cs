﻿using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class SendTextCommandHandler : DeviceCommandHandler
{
    private readonly string message;

    public SendTextCommandHandler(string message, DeviceConnectionContext context, CommandContext commandContext) :
        base(context, commandContext)
    {
        this.message = message;
    }
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var textMessageFactory = new TextMessageFactory(container);
        var textMessage = textMessageFactory.CreateTextMessagePacket(message);
        Logger.LogInformation($"Sending text message...");

        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(textMessage),
             (fromRadio, container) =>
             {
                 var routingResult = fromRadio.GetPayload<Routing>();
                 if (routingResult != null && fromRadio.Packet.Priority == MeshPacket.Types.Priority.Ack)
                 {
                     if (routingResult.ErrorReason == Routing.Types.Error.None)
                         Logger.LogInformation("Acknowledged");
                     else
                         Logger.LogInformation($"Message delivery failed due to reason: {routingResult.ErrorReason}");

                     return Task.FromResult(true);
                 }

                 return Task.FromResult(fromRadio != null);
             });
    }
}
