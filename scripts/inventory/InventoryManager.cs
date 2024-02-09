using Godot;
using System;
using Common;
using Inventory;

public partial class InventoryManager : CanvasLayer
{

    [Signal]
    public delegate void InventoryOpenEventHandler();
    
    [Signal]
    public delegate void InventoryCloseEventHandler();
    
    [Node] private PlayerInventory _playerInventory;

    public override void _Ready()
    {
        this.InitAttributes();
        Close();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("inventory"))
        {
            if (IsOpen())
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    public bool IsOpen()
    {
        return Visible;
    }

    private void Open()
    {
        Visible = true;
        foreach (var child in GetChildren())
        {
            child.SetProcess(true);
        }
        EmitSignal(SignalName.InventoryOpen);
    }
    
    private void Close()
    {
        Visible = false;
        foreach (var child in GetChildren())
        {
            child.SetProcess(false);
        }
        EmitSignal(SignalName.InventoryClose);
    }
}
