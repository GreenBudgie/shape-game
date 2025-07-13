using System.Collections.Generic;

public partial class Blaster : Node
{

    private BlasterInventory _inventory = null!;
    private int _lastSlot;

    public float Delay { get; private set; }

    public override void _Ready()
    {
        _inventory = InventoryManager.Instance.LeftBlasterInventory;
    }

    public override void _Process(double delta)
    {
        if (Delay > 0)
        {
            Delay -= (float)delta;
            return;
        }
    }

    public bool Trigger()
    {
        if (Delay > 0)
        {
            return false;
        }

        var slots = _inventory.GetSlots();
        var startSlot = _lastSlot;
        var modifiers = new List<ModifierModule>();
        for (var i = startSlot; NextSlot() != startSlot; i = NextSlot())
        {
            _lastSlot = i;
            var slot = slots[i];
            var module = slot.GetModule()?.Module;

            if (module == null)
            {
                continue;
            }

            if (module is ModifierModule modifierModule)
            {
                modifiers.Add(modifierModule);
                continue;
            }

            if (module is ProjectileModule projectileModule)
            {
                ShootProjectile(modifiers, projectileModule);
                return true;
            }
        }

        return false;
    }

    private void ShootProjectile(List<ModifierModule> modifiers, ProjectileModule projectileModule)
    {
        var projectile = projectileModule.CreateProjectile().ToNode();

        foreach (var modifier in modifiers)
        {
            modifier.Apply(projectile);
        }

        var spawnPosition = Player.FindPlayer()?.GetGlobalNosePosition() ?? ShapeGame.Center;
        ShapeGame.Instance.AddChild(projectile);
        projectile.GlobalPosition = spawnPosition;

        Delay = 1f;
    }

    private int NextSlot()
    {
        return _lastSlot + 1 >= _inventory.GetSlots().Count ? 0 : _lastSlot + 1;
    }

}