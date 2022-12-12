﻿using Meshtastic.Cli.Reflection;
using Meshtastic.Protobufs;
using System.Reflection;

namespace Meshtastic.Cli.Parsers;

public class SettingParser
{
    private readonly IEnumerable<string> settings;

    public SettingParser(IEnumerable<string> settings)
	{
        this.settings = settings;
    }

    public SettingParserResult ParseSettings(bool isGetOnly)
    {
        var parsedSettings = new List<ParsedSetting>();
        var validationIssues = new List<string>();

        foreach(var setting in this.settings)
        {
            if (isGetOnly)
            {
                TryParse(parsedSettings, validationIssues, setting, null);
            }
            else // Set settings
            {
                var segments = setting.Split('=', StringSplitOptions.TrimEntries);

                if (segments.Length != 2)
                    validationIssues.Add($"Could not parse setting `{setting}`. Please use the format `mqtt.host=mywebsite.com`");
                else
                    TryParse(parsedSettings, validationIssues, segments[0], segments[1]);
            }
        }
        return new SettingParserResult(parsedSettings, validationIssues);
    }

    private void TryParse(List<ParsedSetting> parsedSettings, List<string> validationIssues, string setting, string? value)
    {
        var segments = setting.Split('.', StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
            validationIssues.Add($"Could not parse setting `{setting}`. Please use the format `mqtt.host`");
        else
        {
            var section = SearchConfigSections(segments.First());
            if (section == null)
                validationIssues.Add($"Could not find section `{segments.First()}` in config or module config");
            else
            {
                var sectionSetting = section.PropertyType.FindPropertyByName(segments[1]);
                if (sectionSetting == null)
                    validationIssues.Add($"Could not find setting `{segments[1]}` under {section.Name}");
                else
                {
                    var parsedValue = value == null ? null : ParseValue(sectionSetting, value!);
                    parsedSettings.Add(new ParsedSetting(section, sectionSetting, parsedValue));
                }
            }
        }
    }

    private static object ParseValue(PropertyInfo setting, string value)
    {
        if (setting.PropertyType == typeof(uint))
            return uint.Parse(value);
        else if (setting.PropertyType == typeof(float))
            return float.Parse(value);
        else if (setting.PropertyType == typeof(bool))
            return bool.Parse(value);
        else if (setting.PropertyType == typeof(string))
            return value;
        else 
            return Enum.Parse(setting.DeclaringType!, value);
    }

    private static PropertyInfo? SearchConfigSections(string section)
    {
        return typeof(LocalConfig).FindPropertyByName(section) ?? typeof(LocalModuleConfig).FindPropertyByName(section);
    }
}

public record SettingParserResult(IEnumerable<ParsedSetting> ParsedSettings, IEnumerable<string> ValidationIssues);
public record ParsedSetting(PropertyInfo Section, PropertyInfo Setting, object? Value);