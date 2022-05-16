using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using HexCraft;

public class TerrainGenerator : Spatial
{
    [Export] public PackedScene Block;

    [Export] public int Octaves = 2;
    [Export] public int Lacunarity = 2;
    [Export] public float Persistence = 0.5f;
    [Export] public float HeightScale = 15;
    [Export] public float SampleSize = 1.5f;

    public static Dictionary<string, BlockData> BlocksData = new();
    public static Dictionary<string, Sprite> BlockIcons = new();
    public static Dictionary<string, Material> BlockTextures = new();

    private float Root3;
    private OpenSimplexNoise NoiseMap;
    private int Seed;

    private InventoryManager _inventory;

    public override void _Ready()
    {
        Root3 = Mathf.Sqrt(3);

        Seed = new Random().Next(0, 100000);
        NoiseMap = new OpenSimplexNoise();
        NoiseMap.Seed = Seed;

        NoiseMap.Octaves = Octaves;
        NoiseMap.Lacunarity = Lacunarity;
        NoiseMap.Persistence = Persistence;

        //get all block data from blocks_data.tres json file
        var file = new File();
        file.Open("res://blocks_data.tres", File.ModeFlags.Read);
        var blockDataJson = new StringBuilder();
        while (!file.EofReached())
        {
            blockDataJson.Append(file.GetLine());
        }
        var blocks = JsonConvert.DeserializeObject<List<BlockData>>(blockDataJson.ToString());
        BlocksData = blocks.ToDictionary(b => b.Id, b => b);

        foreach (var block in BlocksData.Values)
        {
            //block texture
            var text = GD.Load(block.TexturePath) as Texture;
            var material = new SpatialMaterial();
            material.AlbedoTexture = text;
            BlockTextures.Add(block.Id, material);
            
            //icons
            var icon = GD.Load(block.IconPath) as Texture;
            var sprite = new Sprite();
            sprite.Texture = icon;
            BlockIcons.Add(block.Id, sprite);
        }

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                var sampleX = x * SampleSize;
                var sampleY = y * SampleSize;

                var h = Mathf.RoundToInt(NoiseMap.GetNoise2d(sampleX, sampleY) * HeightScale);

                AddBlock("grass", x, y, h);
                
                h--;
                for (; h >= -HeightScale; h--)
                {
                    AddBlock("dirt", x, y, h);
                }
            }
        }

        _inventory = GetNode<InventoryManager>("../Inventory");
        _inventory.Init();
    }

    public void AddBlock(string id, int x, int y, int h)
    {
        var position = new Vector3(
            (y % 2 == 0) ? (x * Root3) : (x * Root3 + Root3 / 2), //offset position for odd numbered rows
            h * 2, //double because block is 2 units tall
            y * 1.5f //offset
            );
        
        
        var block = (Block)Block.Instance();
        block.Translation = position;
        block.Id = id;
        block.X = x;
        block.Y = y;
        block.H = h;
        this.AddChild(block);
    }
}
