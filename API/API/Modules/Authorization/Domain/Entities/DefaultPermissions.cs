
namespace Authorization.Domain.Entities
{
    public static class DefaultPermissions
    {
        public static readonly (string Code, string Description)[] All = new[]
        {
            ("GERER_RECLAMATION", "Créer un utilisateur"),
            ("GERER_MAG", "Modifier un utilisateur"),
            ("CREER_MAG", "Supprimer un utilisateur"),
            ("CONSULTER_SYNTHESE", "Consulter les utilisateurs"),

            ("TELECHARGER_MAG", "Créer un rôle"),
            ("TELECHARGER_CARTE_REGULER", "Modifier un rôle"), 

            // Ajoutez ici les futures permissions "core" du projet
        };
    }
}