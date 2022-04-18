using System.Text.Json;

var blockchain = new Blockchain.Core.Blockchain();

blockchain.Initialize();

var block = blockchain.CreateBlock("Karl is great!");
blockchain.AddBlock(block);

block = blockchain.CreateBlock("Diggy diggy hole!");
blockchain.AddBlock(block);

block = blockchain.CreateBlock("This could be a transaction or an url to a god damn image!");
blockchain.AddBlock(block);

if(!blockchain.ValidateChain(blockchain.Blocks)) {
    Console.WriteLine("Chain is invalid!");
}

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
options.WriteIndented = true;
var jsonChain = JsonSerializer.Serialize(blockchain.Blocks, options);
Console.WriteLine(jsonChain);
