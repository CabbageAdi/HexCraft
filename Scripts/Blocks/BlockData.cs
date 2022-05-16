using Newtonsoft.Json;

namespace HexCraft;

public class BlockData
{
    [JsonProperty("id")]
    public string Id { get; private set; }
    
    [JsonProperty("texture")]
    public string TexturePath { get; private set; }
    
    [JsonProperty("icon")]
    public string IconPath { get; private set; }
}
