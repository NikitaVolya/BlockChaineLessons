using BlockChaine.Models;



namespace BlockChaine.Services
{
    public class FileService
    {
        private readonly string _chainFilePath = "blockchain.json";

        public void SaveChain(List<Block> chain)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(chain);
            File.WriteAllText(_chainFilePath, json);
        }

        public List<Block> LoadChain()
        {
            if (!File.Exists(_chainFilePath))
                return new List<Block>();

            var json = File.ReadAllText(_chainFilePath);
            var chain = System.Text.Json.JsonSerializer.Deserialize<List<Block>>(json) ?? new List<Block>();

            return chain;
        }
    }
}
