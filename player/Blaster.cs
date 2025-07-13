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
        var modifiers = new List<ModuleData>();
        
        for (var i = startSlot; NextSlot() == startSlot; i = NextSlot())
        {
            var slot = slots[i];
            var module = slot.GetModule()?.ModuleData;

            if (module == null)
            {
                continue;
            }

            if (module.IsModifier())
            {
                modifiers.Add(module);
                continue;
            }
            
            if (module.IsProjectile())
            {
                ShootProjectile(modifiers, module);
                _lastSlot = i;
                return true;
            }
        }

        return false;
    }

    private void ShootProjectile(List<ModuleData> modifiers, ModuleData projectileModule)
    {
        var projectileBehavior = (ProjectileModuleBehavior)projectileModule.Behavior;
        var projectile = projectileBehavior.CreateProjectile().Projectile.ToNode();

        foreach (var modifier in modifiers)
        {
            var modifierBehavior = (ModifierModuleBehavior)modifier.Behavior;
            modifierBehavior.Apply(projectile);
        }

        var spawnPosition = Player.FindPlayer()?.GetGlobalNosePosition() ?? ShapeGame.Center;
        projectile.GlobalPosition = spawnPosition;

        Delay = 1f;
    }

    private int NextSlot()
    {
        return _lastSlot + 1 >= _inventory.GetSlots().Count ? 0 : _lastSlot + 1;
    }

}