using System.Linq;
using BlueLine.Skaters;
using Godot;

namespace BlueLine.Management;

public partial class ControlManager : Node
{
    public override void _Ready()
    {
        GameEvents.Instance.NewPuckCarrier += ReactToNewPuckCarrier;
    }

    private void ReactToNewPuckCarrier(Skater skater)
    {
        if (skater.InputDevice is ControllerInput)
            return;

        foreach (PlayerInfo player in MatchStatus.Instance.PlayerControllers.Where(p => p.IsHomeTeam == skater.IsHomeTeam))
        {
            // here I need to replace the aiinput with a controllerinput. 
            ControllerInput input = GetParent().GetNode<ControllerInput>($"ControllerInput{player.DeviceId}");

            if (input != null)
            {
                Skater oldSk8r = MatchStatus.Instance.SpawnedSkaters.First(p => p.InputDevice.DeviceId == input.DeviceId);
                oldSk8r.InputDevice = new AiInput();
                skater.InputDevice = input;
            }

            break;
        }
    }

}