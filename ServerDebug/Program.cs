﻿using System;
using SimpleBlockchain.Net;
using System.Net;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using Newtonsoft.Json;

namespace ServerDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            P2PServer server = new P2PServer(
                                            hashLength: 512,
                                            randomNumberLength: 64,
                                            hostName: Dns.GetHostName(),
                                            port: 8900,
                                            converter: new ByteConverter(),
                                            verifier: new ECDSASignatureVerifier(),
                                            digest: new KeccakDigest(512)
                                            );

            server.OnBlockAccepted += (sender, eventArgs) => Console.WriteLine($"Accepted block:\n{JsonConvert.SerializeObject(eventArgs.Block, Formatting.Indented)}\n");
            server.OnTransactionAccepted += (sender, eventArgs) => Console.WriteLine($"Accepted transaction:\n{JsonConvert.SerializeObject(eventArgs.Transaction, Formatting.Indented)}\n");

            server.Start();

            Console.ReadKey();

            server.Stop();
        }
    }
}
