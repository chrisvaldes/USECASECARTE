# TODO - Correction : Pré-sélectionner les permissions lors de la modification d'un rôle

## Objectif
Au clic sur l'icône de modification d'un rôle dans la liste, naviguer vers `/MAG/roles/modifier/{id}` avec le nom du rôle chargé ET les permissions associées pré-cochées dans l'arbre des permissions.

## Étapes

- [x] 1. Analyser le problème
- [x] 2. Obtenir l'approbation du plan
- [x] 3. Modifier `site.js` : Mettre à jour `initPermissionTree` pour accepter et utiliser les IDs pré-sélectionnés
- [x] 4. Tester la compilation (vérifier que le projet build correctement)
- [x] 5. Corriger `UpdateRole.razor.cs` : Comparer les permissions avec `p.Code` au lieu de `p.Id` car l'API retourne des **codes** (ex: "UTILISATEUR") et non des IDs numériques

