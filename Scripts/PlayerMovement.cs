using Godot;
using System;
using System.Linq;
using Godot.Collections;

public class PlayerMovement : KinematicBody
{
    [Export] public float Gravity = -24.8f;
    [Export] public float MaxSpeed = 2.5f;
    [Export] public float JumpSpeed = 15.0f;
    [Export] public float MaxSlopeAngle = 40.0f;
    [Export] public float MouseSensitivity = 0.05f;
    public Camera _camera;

    private Vector3 _vel = new Vector3();
    private Vector3 _dir = new Vector3();

    private Spatial _rotationHelper;
    private CollisionShape _collisionShape;
    private TerrainGenerator _terrain;
    private InventoryManager _inventory;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _camera = GetNode<Camera>("Camera");
        _rotationHelper = GetNode<Camera>("Camera");
        _collisionShape = GetNode<CollisionShape>("CollisionShape");
        _terrain = GetNode<TerrainGenerator>("../Terrain");
        _inventory = GetNode<InventoryManager>("../Inventory");

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    private Block HighlightedBlock;
    private bool Deleted = false;
    private Vector2 MousePosition = new();
    private Vector3 HighlightedNormal = new();
    
    public override void _Process(float delta)
    {
        var spaceState = GetWorld().DirectSpaceState;
        
        var from = _camera.ProjectRayOrigin(MousePosition);
        var to = from + _camera.ProjectRayNormal(MousePosition) * 10;
        
        //Highlight block being looked at
        var ray = spaceState.IntersectRay(from, to, new Godot.Collections.Array {this});
        if (ray.Count > 0 && ray["collider"] is Block block)
        {
            if (HighlightedBlock != null && HighlightedBlock != block && !Deleted)
            {
                HighlightedBlock.UnHighlight();
            }
                
            HighlightedBlock = block;
            Deleted = false;
            HighlightedBlock.Highlight();
            
            HighlightedNormal = (Vector3)ray["normal"];
        }
        else
        {
            if (HighlightedBlock != null && !Deleted)
            {
                HighlightedBlock.UnHighlight();
            }
            HighlightedBlock = null;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        ProcessInput(delta);
        ProcessMovement(delta);
    }

    private void ProcessInput(float delta)
    {
        //  Walking
        _dir = new Vector3();
        Transform camXform = _camera.GlobalTransform;

        Vector2 inputMovementVector = new Vector2();

        if (Input.IsKeyPressed((int) KeyList.W))
            inputMovementVector.y += MaxSpeed;
        if (Input.IsKeyPressed((int) KeyList.S))
            inputMovementVector.y -= MaxSpeed;
        if (Input.IsKeyPressed((int) KeyList.A))
            inputMovementVector.x -= MaxSpeed;
        if (Input.IsKeyPressed((int) KeyList.D))
            inputMovementVector.x += MaxSpeed;

        inputMovementVector = inputMovementVector.Normalized();
        
        _dir += -camXform.basis.z * inputMovementVector.y;
        _dir += camXform.basis.x * inputMovementVector.x;
        //  Jumping
        if (IsOnFloor())
        {
            if (Input.IsKeyPressed((int) KeyList.Space))
                _vel.y = JumpSpeed;
        }
        //  Capturing/Freeing the cursor
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (Input.GetMouseMode() == Input.MouseMode.Visible)
                Input.SetMouseMode(Input.MouseMode.Captured);
            else
                Input.SetMouseMode(Input.MouseMode.Visible);
        }
    }

    //copied completely from the godot docs
    private void ProcessMovement(float delta)
    {
        _dir.y = 0;
        _dir = _dir.Normalized();

        _vel.y += delta * Gravity;

        Vector3 hvel = _vel;
        hvel.y = 0;

        Vector3 target = _dir;

        target *= MaxSpeed;

        //temporary constant values until I figure out how to remove acceleration
        float accel;
        if (_dir.Dot(hvel) > 0)
            accel = 4.5f;
        else
            accel = 16.0f;

        hvel = hvel.LinearInterpolate(target, accel * delta);
        _vel.x = hvel.x;
        _vel.z = hvel.z;
        _vel = MoveAndSlide(_vel, new Vector3(0, 1, 0), false, 4, Mathf.Deg2Rad(MaxSlopeAngle));
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
            _rotationHelper.RotateX(Mathf.Deg2Rad(mouseEvent.Relative.y * -MouseSensitivity));
            RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * MouseSensitivity));

            Vector3 cameraRot = _rotationHelper.RotationDegrees;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -70, 70);
            _rotationHelper.RotationDegrees = cameraRot;

            MousePosition = mouseEvent.Position;
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (HighlightedBlock != null && mouse.ButtonIndex == (int)ButtonList.Left && mouse.Pressed && !Deleted)
            {
                Deleted = true;
                HighlightedBlock.QueueFree();
                HighlightedBlock = null;
            }
            if (HighlightedBlock != null && mouse.ButtonIndex == (int)ButtonList.Right && mouse.Pressed && !Deleted)
            {
                _terrain.AddBlock(_inventory.Items[_inventory.HotbarSlot].id, HighlightedBlock.X, HighlightedBlock.Y, HighlightedBlock.H + 1);
                GD.Print("added");
            }
        }
    }
}
