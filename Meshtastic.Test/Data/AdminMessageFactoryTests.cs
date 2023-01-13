﻿using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class AdminMessageFactoryTests
{
    private Fixture fixture;
    private DeviceStateContainer deviceStateContainer;
    private AdminMessageFactory factory;

    [SetUp]
    public void Setup()
    {
        fixture = new Fixture();
        deviceStateContainer = new DeviceStateContainer();
        deviceStateContainer.MyNodeInfo.MyNodeNum = 100;
        deviceStateContainer.LocalConfig = new LocalConfig
        {
            Lora = new LoRaConfig()
            {
                HopLimit = 3,
            }
        };
        factory = new AdminMessageFactory(deviceStateContainer);
    }

    [Test]
    public void CreateBeginEditSettingsMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateBeginEditSettingsMessage();
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateCommitEditSettingsMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateCommitEditSettingsMessage();
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateRebootMessage_Should_ReturnValidAdminMessage()
    {
        int seconds = 0;
        bool isOta = false;

        var result = factory.CreateRebootMessage(seconds,isOta);
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateSetConfigMessage_Should_ReturnValidAdminMessage()
    {
        var instance = new NetworkConfig();

        var result = factory.CreateSetConfigMessage(instance);
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateSetConfigMessage_Should_ThrowArgumentException_GivenNonConfigInstance()
    {
        var instance = new Telemetry();

        var action = () => factory.CreateSetConfigMessage(instance);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateSetModuleConfigMessage_Should_ReturnValidAdminMessage()
    {
        var instance = new SerialConfig();

        var result = factory.CreateSetModuleConfigMessage(instance);
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateSetModuleConfigMessage_Should_ThrowArgumentException_GivenNonModuleConfigInstance()
    {
        var instance = new Telemetry();

        var action = () => factory.CreateSetModuleConfigMessage(instance);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateSetChannelMessage_Should_ReturnValidAdminMessage()
    {
        var channel = new Channel();

        var result = factory.CreateSetChannelMessage(channel);
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateGetMetadataMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateGetMetadataMessage();
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateFactoryResetMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateFactoryResetMessage();
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateNodeDbResetMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateNodeDbResetMessage();
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateSetCannedMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateSetCannedMessage(String.Empty);
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }

    [Test]
    public void CreateGetCannedMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateGetCannedMessage();
        result.Decoded.Portnum.Should().Be(PortNum.AdminApp);
    }
}
