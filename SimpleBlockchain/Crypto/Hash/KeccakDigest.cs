﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHA3;

namespace SimpleBlockchain.Crypto.Hash
{
    public class KeccakDigest : IDigest
    {
        private SHA3Managed digest;

        public int Id { get; }

        public int HashLength => digest.HashByteLength;

        public KeccakDigest(int id, int hashLengthInBits)
        {
            Id = id;
            digest = new SHA3Managed(hashLengthInBits);
        }

        public byte[] GetHash(byte[] data) => digest.ComputeHash(data);
    }
}
