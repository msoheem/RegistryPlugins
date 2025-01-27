﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Registry.Abstractions;
using RegistryPluginBase.Classes;
using RegistryPluginBase.Interfaces;

namespace RegistryPlugin.USB
{
    public class USB : IRegistryPluginGrid
    {
        private readonly BindingList<ValuesOut> _values;
        public USB()
        {
            _values = new BindingList<ValuesOut>();

            Errors = new List<string>();
        }
        public string InternalGuid => "cb4b10ca-be72-4e02-9dc5-080110bcfd30";

        public List<string> KeyPaths => new List<string>(new[]
        {
            @"ControlSet00*\Enum\USB"
        });

        public string ValueName => null;
        public string AlertMessage { get; private set; }
        public RegistryPluginType.PluginType PluginType => RegistryPluginType.PluginType.Grid;
        public string Author => "Hyun Yi @hyuunnn";
        public string Email => "";
        public string Phone => "000-0000-0000";
        public string PluginName => "USB";

        public string ShortDescription
            => "USB Information";

        public string LongDescription => ShortDescription;

        public double Version => 0.1;
        public List<string> Errors { get; }

        public void ProcessValues(RegistryKey key)
        {
            _values.Clear();
            Errors.Clear();

            foreach (var rd in ProcessKey(key))
            {
                _values.Add(rd);
            }
        }

        public IBindingList Values => _values;

        private string SplitData(string data)
        {
            if (data != null)
            {
                string[] split = data.Split(';');
                return split[split.Length - 1];
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<ValuesOut> ProcessKey(RegistryKey key)
        {
            var l = new List<ValuesOut>();

            foreach (var registryKey in key.SubKeys)
            {
                try 
                {
                    string keyName = registryKey.KeyName;

                    foreach (var subKey in registryKey.SubKeys)
                    {
                        string serialNumber = subKey.KeyName;
                        string parentIdPrefix = subKey.Values.SingleOrDefault(t => t.ValueName == "ParentIdPrefix")?.ValueData;
                        string service = subKey.Values.SingleOrDefault(t => t.ValueName == "Service")?.ValueData;
                        string deviceDesc = SplitData(subKey.Values.SingleOrDefault(t => t.ValueName == "DeviceDesc")?.ValueData);
                        string friendlyName = SplitData(subKey.Values.SingleOrDefault(t => t.ValueName == "FriendlyName")?.ValueData);
                        string locationinformation = subKey.Values.SingleOrDefault(t => t.ValueName == "LocationInformation")?.ValueData;
                        

                        DateTimeOffset? ts = subKey.LastWriteTime;

                        var ff = new ValuesOut(keyName, serialNumber, parentIdPrefix, service, deviceDesc, friendlyName, locationinformation, ts)
                        {
                            BatchValueName = "Multiple",
                            BatchKeyPath = subKey.KeyPath
                        };
                        l.Add(ff);
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add($"Error processing USB key: {ex.Message}");
                }
            }

            if (Errors.Count > 0)
            {
                AlertMessage = "Errors detected. See Errors information in lower right corner of plugin window";
            }

            return l;
        }
    }
}
