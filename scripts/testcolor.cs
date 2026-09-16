using Godot;

public partial class testcolor : CharacterBody3D
{
    private MeshInstance3D _head;
    private MeshInstance3D _torso;
    private MeshInstance3D _arms;
    private MeshInstance3D _gloves;
    private MeshInstance3D _pants;
    private MeshInstance3D _socks;

    private enum HeadSlots
    {
        Skin = 0
    }

    private enum JerseySlots
    {
        Base = 0,
        Stripe
    }

    public override void _Ready()
    {
        _head = GetNode<MeshInstance3D>("Model/Head");
        _torso = GetNode<MeshInstance3D>("Model/Torso");
        _arms = GetNode<MeshInstance3D>("Model/Arms");
        _gloves = GetNode<MeshInstance3D>("Model/Gloves");
        _pants = GetNode<MeshInstance3D>("Model/Pants");
        _socks = GetNode<MeshInstance3D>("Model/Legs");
    }

    public void ApplyTeamColors(Color primary, Color secondary, Color skin)
    {
        ApplyColorToSlot(_head, (int)HeadSlots.Skin, skin);

        ApplyColorToSlot(_torso, (int)JerseySlots.Base, primary);
        ApplyColorToSlot(_torso, (int)JerseySlots.Stripe, secondary);
        
        ApplyColorToSlot(_arms, (int)JerseySlots.Base, primary);
        ApplyColorToSlot(_arms, (int)JerseySlots.Stripe, secondary);
        
        ApplyColorToSlot(_gloves, (int)JerseySlots.Base, primary);
        ApplyColorToSlot(_gloves, (int)JerseySlots.Stripe, secondary);
        
        ApplyColorToSlot(_pants, (int)JerseySlots.Base, primary);
        ApplyColorToSlot(_pants, (int)JerseySlots.Stripe, secondary);
        
        ApplyColorToSlot(_socks, (int)JerseySlots.Base, primary);
        ApplyColorToSlot(_socks, (int)JerseySlots.Stripe, secondary);
    }

    private void ApplyColorToSlot(MeshInstance3D meshInstance, int surfaceIndex, Color color)
    {
        StandardMaterial3D material = meshInstance.GetActiveMaterial(surfaceIndex) as StandardMaterial3D;
        StandardMaterial3D instanceMaterial = (StandardMaterial3D)material.Duplicate();
        instanceMaterial.AlbedoColor = color;
        meshInstance.SetSurfaceOverrideMaterial(surfaceIndex, instanceMaterial);
    }
}
