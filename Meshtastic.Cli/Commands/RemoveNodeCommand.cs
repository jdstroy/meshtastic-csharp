using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class RemoveNodeCommand : Command
{
    public RemoveNodeCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) :
        base(name, description)
    {
        var nodeNum = new Argument<uint>("nodenum", "Nodenum of the node to remove from the device NodeDB");

        this.SetHandler(async (context, commandContext) =>
            {
                var handler = new RemoveNodeCommandHandler(nodeNum, context, commandContext);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
