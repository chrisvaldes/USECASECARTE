
namespace Authorization.Domain.Entities
{
    public static class DefaultPermissions
    {
        public static readonly (string Code, string Description)[] All = new[]
        {
            ("GERER_RECLAMATION", "RECLAMATION"),
            ("GERER_MAG", "GERER_MAG"),
            ("CREER_MAG", "CREER_MAG"),
            ("CONSULTER_SYNTHESE", "CONSULTER_SYNTHESE"),

            ("TELECHARGER_MAG", "TELECHARGER_MAG"),
            ("TELECHARGER_CARTE_REGULER", "TELECHARGER_CARTE"), 

            // Ajoutez ici les futures permissions "core" du projet
        };
    }
}