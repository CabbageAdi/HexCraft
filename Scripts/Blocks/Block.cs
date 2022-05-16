using Godot;
using System;

public class Block : StaticBody
{
    [Export] public string Id;

    public int X;
    public int Y;
    public int H;

    private MeshInstance Mesh;
    private bool Highlighted = false;

    public override void _Ready()
    {
        Mesh = this.GetNode<MeshInstance>("Mesh");
        Mesh.MaterialOverride = TerrainGenerator.BlockTextures[Id];
    }

    public void Highlight()
    {
        if (Highlighted) return;
        var material = (SpatialMaterial)Mesh.MaterialOverride.Duplicate();
        material.EmissionEnabled = true;
        material.Emission = Colors.White;
        material.EmissionEnergy = 0.1f;
        Mesh.MaterialOverride = material;
        Highlighted = true;
    }

    public void UnHighlight()
    {
        Mesh.MaterialOverride = TerrainGenerator.BlockTextures[Id];
        Highlighted = false;
    }
}
