namespace Use_Case_Carte.Components.Route; 
 
public static class Route
{
    public const string Login = "/";

    public const string Dashboard = "/MAG/dashboard";

    public const string Profils = "/MAG/profil";
    public const string CreateProfil = "/MAG/creation-profil";
    public const string ModifierProfil = "/MAG/modification-profil";
    public const string SupprimerProfil = "/MAG/suppression-profil";

    public const string GestionMAG = "/MAG/gestion-mag";
    public const string NouveauMag = "/MAG/nouveau-mag";
    public const string DetailFacturation = "/MAG/detail-facturation";
    public const string SyntheseMag = "/MAG/synthese";

    public const string Roles = "/MAG/roles";
    public const string CreateRoles = "/MAG/roles/creer";
    public const string ModifierRole = "/MAG/roles/modifier";

    public const string Permissions = "/MAG/permissions";
    public const string NouvellePermission = "/MAG/nouvelle-permission";

    public const string Utilisateurs = "/MAG/utilisateurs";
    public const string NouveauUtilisateur = "/MAG/nouveau-utilisateur";
    public const string ModifierUtilisateur = "/MAG/modifier-utilisateur";

     public const string Profil = "MAG/profil"; 
}
