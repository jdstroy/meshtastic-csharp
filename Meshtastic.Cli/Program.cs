﻿using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Handlers;


var portOption = new Option<string>(name: "--port", description: "Target serial port for meshtastic device");
var hostOption = new Option<string>(name: "--host", description: "Target host ip or name for meshtastic device");

var monitorCommand = new Command("monitor", "Serial monitor for meshtastic devices");
monitorCommand.SetHandler(MonitorHandler.Handle,
    new ConnectionBinder(portOption, hostOption), 
    new LoggingBinder());

var infoCommand = new Command("info", "Dump info about the currently connected meshtastic node");
infoCommand.SetHandler(InfoCommandHandler.Handle,
    new ConnectionBinder(portOption, hostOption), 
    new LoggingBinder());

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddGlobalOption(hostOption);
rootCommand.AddCommand(monitorCommand);
rootCommand.AddCommand(infoCommand);

return await AnsiConsole.Status()
    .StartAsync("Connecting...", async ctx =>
    {
        ctx.Status("Connecting...");
        ctx.Spinner(Spinner.Known.Dots);
        ctx.SpinnerStyle(new Style(Meshtastic.Cli.StyleResources.MESHTASTIC_GREEN));

        return await rootCommand.InvokeAsync(args);
    });