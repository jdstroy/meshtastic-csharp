using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meshtastic.Cli.Commands
{
    public class TunnelCommand : Command
    {
        public TunnelCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
        {
            this.SetHandler(async (context, commandContext) =>
            {
                var handler = new TunnelCommandHandler(context, commandContext);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, new Option<uint?>("dest") { }, new Option<bool>("select-dest") { }));
        }
    }
}
