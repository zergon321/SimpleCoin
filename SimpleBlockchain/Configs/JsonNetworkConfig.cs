﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.Configs.Parameters;
using Newtonsoft.Json;
using System.IO;

namespace SimpleBlockchain.Configs
{
    public class JsonNetworkConfig : INetworkConfig
    {
        private NetworkParameters parameters;

        public string AddressBookPath => parameters.AddressBookPath;

        public int RandomNumberLength => parameters.RandomNumberLength;
        public int HashLength => parameters.HashLength;

        public JsonNetworkConfig(string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            using (Stream jsonFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (StreamReader reader = new StreamReader(jsonFile))
            using (JsonReader jsonReader = new JsonTextReader(reader))
                parameters = jsonSerializer.Deserialize<NetworkParameters>(jsonReader);
        }
    }
}
