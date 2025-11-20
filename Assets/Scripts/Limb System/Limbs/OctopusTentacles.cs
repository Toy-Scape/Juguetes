public class OctopusTentacles : Limb
{
    public OctopusTentacles ()
    {
        LimbName = "Tentáculos de Pulpo";

        // Puedes tenr varias pasivas, aquí las agrupamos
        PassiveAbility = new CompositePassiveAbility(
            new WallClimbPassive(),
            new SwimPassive()
        );
    }
}