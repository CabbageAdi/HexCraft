using System.Collections.Generic;
using System.Linq;
using Godot;

public class InventoryManager : Node
{
    [Export] public Texture SlotIcon;
    [Export] public int HotbarSlots;
    [Export] public Vector2 Dimensions;

    //0 to HotbarSlots - 1 is hotbar items, next is top left corner of internal inventory
    public Dictionary<int, (string id, int quantity)> Items;
    public int HotbarSlot = 0;

    private List<Sprite> _slotSprites = new();
    private List<Sprite> _itemSprites = new();
    
        
    private Container _uiNode;

    public void Init()
    {
        _uiNode = GetNode<Container>("../UI");
        Items = new((int)(HotbarSlots + Dimensions.x * Dimensions.y));

        for (int i = 0; i < HotbarSlots; i++)
        {
            var sprite = new Sprite();
            sprite.Texture = SlotIcon;
            sprite.Position = new Vector2(50 + i * 100, 550);
            sprite.Modulate = Colors.Black;
            
            var itemSprite = new Sprite();
            itemSprite.Position = sprite.Position;

            _uiNode.AddChild(sprite);
            _uiNode.AddChild(itemSprite);
            _slotSprites.Add(sprite);
            _itemSprites.Add(itemSprite);
        }
        
        //temporary fill
        for (int i = 0; i < HotbarSlots && i < TerrainGenerator.BlocksData.Count; i++)
        {
            SetItem(i, TerrainGenerator.BlocksData.ElementAt(i).Key, 1);
        }
    }

    public void SetItem(int position, string id, int quantity)
    {
        Items[position] = (id, quantity);
        var block = TerrainGenerator.BlockIcons[id];
        _itemSprites[position].Texture = block.Texture;
    }

    public override void _Process(float delta)
    {
        bool set = false;
        for (int i = 0; i < HotbarSlots; i++)
        {
            if (Input.IsKeyPressed((int) KeyList.Key1 + i) && !set)
            {
                HotbarSlot = i;
                set = true;
            }

            if (HotbarSlot == i)
            {
                _slotSprites[i].Modulate = Colors.White;
            }
            else
            {
                _slotSprites[i].Modulate = Colors.Black;
            }
        }
    }
}