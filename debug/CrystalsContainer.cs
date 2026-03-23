using Godot;
using System;

public partial class CrystalsContainer : VBoxContainer
{
    public override void _Ready()
    {
        CreateSpawnCrystalsButtons();
        CreateAddRemoveCrystalsButtons();
    }
    
    private void CreateAddRemoveCrystalsButtons()
    {
        void CreateButton(HBoxContainer container, int amount)
        {
            var spawnButton = new Button();
            spawnButton.Text = amount.ToString();
            spawnButton.Pressed += () => CrystalManager.Instance.Crystals += amount;
            container.AddChild(spawnButton);
        }
        
        var addLabel = new Label();
        addLabel.Text = "Crystal Amount";
        addLabel.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(addLabel);
        
        var addContainer = new HBoxContainer();
        CreateButton(addContainer, 1);
        CreateButton(addContainer, 5);
        CreateButton(addContainer, 20);
        CreateButton(addContainer, 50);
        var addCenterContainer = new CenterContainer();
        addCenterContainer.AddChild(addContainer);
        AddChild(addCenterContainer);
        
        var removeContainer = new HBoxContainer();
        CreateButton(removeContainer, -1);
        CreateButton(removeContainer, -5);
        CreateButton(removeContainer, -20);
        CreateButton(removeContainer, -50);
        var removeCenterContainer = new CenterContainer();
        removeCenterContainer.AddChild(removeContainer);
        AddChild(removeCenterContainer);
        
        var setZeroButton = new Button();
        setZeroButton.Text = "Set to 0";
        setZeroButton.Pressed += () => CrystalManager.Instance.Crystals = 0;
        AddChild(setZeroButton);
    }

    private void CreateSpawnCrystalsButtons()
    {
        var label = new Label();
        label.Text = "Spawn Crystals";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(label);
        
        var hBoxContainer = new HBoxContainer();

        void CreateSpawnButton(int amount)
        {
            var spawnButton = new Button();
            spawnButton.Text = amount.ToString();
            spawnButton.Pressed += () => SpawnCrystals(amount);
            hBoxContainer.AddChild(spawnButton);
        }
        
        CreateSpawnButton(1);
        CreateSpawnButton(5);
        CreateSpawnButton(20);
        CreateSpawnButton(50);
            
        var centerContainer = new CenterContainer();
        centerContainer.AddChild(hBoxContainer);
        AddChild(centerContainer);
        
        var clearCrystalsButton = new Button();
        clearCrystalsButton.Text = "Clear";
        clearCrystalsButton.Pressed += () => CrystalManager.Instance.ClearFallingCrystals();
        AddChild(clearCrystalsButton);
    }
    
    private void SpawnCrystals(int amount)
    {
        const float margin = 64;
        var position = new Vector2(
            ShapeGame.PlayableArea.Position.X + margin,
            ShapeGame.PlayableArea.Position.Y + margin
        );
        var size = new Vector2(
            ShapeGame.PlayableArea.Size.X - margin,
            (ShapeGame.PlayableArea.Size.Y - margin) / 2f
        );
        var smallerArea = new Rect2(position, size);
        for (var i = 0; i < amount; i++)
        {
            FallingCrystal.Spawn(smallerArea.RandomPoint());
        }
    }
}
