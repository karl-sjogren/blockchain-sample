using System.Security.Cryptography;
using System.Text;

namespace Blockchain.Core;

public class Blockchain {
    public IList<Block> Blocks { get; } = new List<Block>();

    public readonly string DIFFICULTY_PREFIX = "00";

    public void Initialize() {
        var genesisBlock = Block.CreateGenesisBlock();

        Blocks.Add(genesisBlock);
    }

    public void AddBlock(Block block) {
        if(Blocks.Count == 0) {
            throw new InvalidOperationException("Can't add block to empty blockchain. Genesis block must be added first.");
        }

        var lastBlock = Blocks.Last();
        if(!ValidateBlock(block, lastBlock, DIFFICULTY_PREFIX)) {
            throw new InvalidOperationException("Block is invalid.");
        }

        Blocks.Add(block);
    }

    private static bool ValidateBlock(Block block, Block lastBlock, string difficultyPrefix) {
        if(block.PreviousHash != lastBlock.Hash) {
            return false;
        }

        if(block.Id != lastBlock.Id + 1) {
            return false;
        }

        if(!TryDecodeHash(block.Hash, out var buffer)) {
            return false;
        }

        if(!ToBinaryRepresentation(buffer).StartsWith(difficultyPrefix)) {
            return false;
        }

        var recalculatedBuffer = CalculateHash(block.Id, block.PreviousHash, block.Timestamp, block.Data, block.Nonce);
        if(!recalculatedBuffer.SequenceEqual(buffer)) {
            return false;
        }

        return true;
    }

    public Block CreateBlock(string data) {
        var lastBlock = Blocks.Last();

        return CreateBlock(lastBlock.Id + 1, lastBlock.Hash, data);
    }

    internal Block CreateBlock(UInt64 id, string previousHash, string data) {
        var now = DateTimeOffset.UtcNow;
        var (nonce, hash) = MineBlock(id, now, previousHash, data, DIFFICULTY_PREFIX);

        return new Block(
            id,
            hash,
            previousHash,
            now,
            data,
            nonce
        );
    }

    public static (UInt64 nonce, string hash) MineBlock(UInt64 id, DateTimeOffset timestamp, string previousHash, string data, string difficultyPrefix) {
        UInt64 nonce = 0;

        while(true) {
            var buffer = CalculateHash(id, previousHash, timestamp, data, nonce);
            if(ToBinaryRepresentation(buffer).StartsWith(difficultyPrefix)) {
                return (nonce, EncodeHash(buffer));
            }

            nonce++;
        }
    }

    public bool ValidateChain(IEnumerable<Block> chain) {
        foreach(var block in chain) {
            if(block.Id == 0) {
                continue;
            }

            var previousBlock = chain.First(x => x.Id == block.Id - 1);

            if(!ValidateBlock(block, previousBlock, DIFFICULTY_PREFIX)) {
                return false;
            }
        }

        return true;
    }

    public IEnumerable<Block> SelectChain(IEnumerable<Block> local, IEnumerable<Block> remote) {
        var localChainIsValid = ValidateChain(local);
        var remoteChainIsValid = ValidateChain(remote);

        if(localChainIsValid && remoteChainIsValid) {
            return local.Count() >= remote.Count() ? local : remote;
        } else if(localChainIsValid) {
            return local;
        } else if(remoteChainIsValid) {
            return remote;
        }

        throw new InvalidOperationException("Both passed chains are invalid.");
    }

    private static bool TryDecodeHash(string hash, out byte[] buffer) {
        try {
            buffer = DecodeHash(hash);
        } catch(Exception) {
            buffer = Array.Empty<byte>();
            return false;
        }

        return true;
    }

    private static byte[] DecodeHash(string hash) {
        var buffer = Convert.FromBase64String(hash);

        return buffer;
    }

    public static byte[] CalculateHash(UInt64 id, string previousHash, DateTimeOffset timestamp, string data, UInt64 nonce) {
        using var hasher = SHA256.Create();

        var buffer = hasher.ComputeHash(Encoding.UTF8.GetBytes($"{id}|{previousHash}|{timestamp:u}|{data}|{nonce}"));

        return buffer;
    }

    public static string EncodeHash(byte[] buffer) {
        var hash = Convert.ToBase64String(buffer);

        return hash;
    }

    private static string ToBinaryRepresentation(byte[] buffer) {
        var builder = new StringBuilder();
        foreach(var @byte in buffer) {
            builder.Append(Convert.ToString(@byte, 2)); //.PadLeft(8, '0'));
        }

        return builder.ToString();
    }

    private static string ToBinaryRepresentation(Int64 value) {
        var binaryRepresentation = Convert.ToString(value, 2);
        return binaryRepresentation;
    }
}
