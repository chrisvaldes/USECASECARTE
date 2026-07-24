# TODO - Sécurité : Redirection token expiré + Contrôle d'accès par page

## Étapes réalisées

- [x] Analyser le code existant
- [x] Obtenir l'approbation du plan
- [x] **Étape 1 : Modifier `JwtMessageHandler.cs`** - Ajouter vérification expiration token avant requête + gestion 401
- [x] **Étape 2 : Modifier `AuthMessageHandler.cs`** - Même logique pour cohérence
- [x] **Étape 3 : Créer page `/acces-refuse` (403)** - Page d'accès refusé + code-behind
- [x] **Étape 4 : Étendre `ProtectedPage.cs`** - Ajouter `RequiredPermissions` + vérification
- [x] **Étape 5 : Ajouter `GoToAccessDenied()` dans `NavigationService`**
- [x] **Étape 6 : Modifier toutes les pages** pour hériter de `ProtectedPageBase` avec leurs permissions

### Pages modifiées avec permissions :

| Page | Permission requise |
|------|-------------------|
| `Dashboard.razor.cs` | BKMVTI_CONSULTER |
| `Profil.razor.cs` | UTILISATEUR |
| `ListProfil.razor.cs` | UTILISATEUR |
| `CreateProfil.razor.cs` | UTILISATEUR |
| `UpdateProfil.razor.cs` | UTILISATEUR |
| `ListeMag.razor.cs` | TYPEMAG, BKMVTI, BKMVTI_CONSULTER |
| `TraiterMag.razor.cs` | TYPEMAG, BKMVTI |
| `ListRoles.razor.cs` | UTILISATEUR |
| `CreateRole.razor.cs` | UTILISATEUR |
| `UpdateRole.razor.cs` | UTILISATEUR |
| `ListeUtilisateur.razor.cs` | UTILISATEUR |
| `NouveauUtilisateur.razor.cs` | UTILISATEUR |
| `UpdateUtilisateur.razor.cs` | UTILISATEUR |
| `Synthese.razor.cs` | BKMVTI_CONSULTER, TYPEMAG |
| `DetailReclamation.razor.cs` | BKMVTI_CONSULTER, TYPEMAG |

- [x] **Étape 7 : Supprimer `@inherits PermissionComponentBase`** du `TraiterMag.razor` (conflit résolu)

