﻿using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Security.Cryptography;
using SHA3;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;
using Newtonsoft.Json;
using SimpleBlockchain.Net.EventArgs;

namespace SimpleBlockchain.Net
{
    class P2PServer : WebSocketBehavior
    {
        private WebSocketServer server;
        private SHA3Managed digest;

        private byte[] hashToVerify;

        public event EventHandler<BlockAcceptEventArgs> OnBlockAccepted;
        public event EventHandler<TransactionAcceptEventArgs> OnTransactionAccepted;

        public ServerState ServerState { get; private set; }

        public int RandomNumberLength { get; set; }
        public int HashLength { get; set; }

        public string HostName { get; set; }
        public int Port { get; set; }

        public IByteConverter Converter { get; set; }
        public ISignatureVerifier Verifier { get; set; }

        public P2PServer()
        {
            ServerState = ServerState.Idle;
            digest = new SHA3Managed(HashLength);
        }

        private void requireAuth()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[RandomNumberLength];
            byte[] randomNumberHash;

            rng.GetBytes(randomNumber);
            randomNumberHash = digest.ComputeHash(randomNumber);
            hashToVerify = randomNumberHash;

            string message = Commands.ServerAuthRequest + " " + Converter.ConvertToString(randomNumberHash);

            Send(message);
        }

        private void sendAuthSuccessful()
        {
            string message = Commands.ServerAuthSuccessfulResponse;

            Send(message);
        }

        private void sendAuthFailure()
        {
            string message = Commands.ServerAuthFailureResponse;

            Send(message);
        }

        private void sendServerBusy()
        {
            string message = Commands.ServerBusyResponse;

            Send(message);
        }

        private void sendProtocolViolation()
        {
            string message = Commands.ServerProtocolViolationResponse;

            Send(message);
        }

        public void Start()
        {
            server = new WebSocketServer("ws://" + HostName + $":{Port}");

            server.AddWebSocketService<P2PServer>("/simplecoin");
            server.Start();
        }

        public void Stop() => server.Stop();

        protected override void OnMessage(MessageEventArgs e)
        {
            string message = e.Data;
            string[] words = message.Split(new char[] { ' ' });
            
            switch (words[0])
            {
                case Commands.ClientHello when ServerState == ServerState.Idle:
                    requireAuth();
                    ServerState = ServerState.WaitingAuth;

                    break;

                case Commands.ClientAuthResponse when ServerState == ServerState.WaitingAuth:
                    byte[] publicKey = Converter.ConvertToByteArray(words[2]);
                    byte[] signature = Converter.ConvertToByteArray(words[4]);

                    bool verified = Verifier.VerifyHash(publicKey, hashToVerify, signature);

                    if (verified)
                    {
                        sendAuthSuccessful();
                        ServerState = ServerState.Busy;
                    }
                    else
                    {
                        sendAuthFailure();
                        ServerState = ServerState.Idle;
                    }

                    break;

                case Commands.ClientAcceptBlockRequest when ServerState == ServerState.Busy:
                    string blockJson = words[1];
                    Block block = JsonConvert.DeserializeObject<Block>(blockJson);
                    BlockAcceptEventArgs blockEventArgs = new BlockAcceptEventArgs(block);

                    OnBlockAccepted?.Invoke(this, blockEventArgs);

                    break;

                case Commands.ClientAcceptTransactionRequest when ServerState == ServerState.Busy:
                    string transactionJson = words[1];
                    Transaction transaction = JsonConvert.DeserializeObject<Transaction>(transactionJson);
                    TransactionAcceptEventArgs transactionEventArgs = new TransactionAcceptEventArgs(transaction);

                    OnTransactionAccepted?.Invoke(this, transactionEventArgs);

                    break;

                case Commands.ClientQuitRequest when ServerState == ServerState.Busy:
                    ServerState = ServerState.Idle;

                    break;

                default:
                    if (ServerState == ServerState.Busy || ServerState == ServerState.WaitingAuth)
                        sendServerBusy();
                    else
                        sendProtocolViolation();

                    break;

            }
        }
    }
}
