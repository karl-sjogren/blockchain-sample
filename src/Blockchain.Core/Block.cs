
namespace Blockchain.Core;
public record Block {
    public UInt64 Id { get; init; }
    public string Hash { get; init; }
    public string PreviousHash { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string Data { get; init; }
    public UInt64 Nonce { get; init; }

    internal Block(UInt64 id, string hash, string previousHash, DateTimeOffset timestamp, string data, UInt64 nonce) {
        Id = id;
        Hash = hash;
        PreviousHash = previousHash;
        Timestamp = timestamp;
        Data = data;
        Nonce = nonce;
    }

    public static Block CreateGenesisBlock() => new(
        0,
        "AAAzgp5AwuObmibuUAtZdb9lWWh6rrxJ4ItB+i/DKCU=",
        string.Empty,
        DateTimeOffset.Parse("2022-04-18T00:00:00Z"),
        "Genesis Block",
        187124
    );
}
