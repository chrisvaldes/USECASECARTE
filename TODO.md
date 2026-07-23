# TODO - Redirection vers la page de connexion quand le token expire

## Étapes

- [x] Analyser le code existant
- [x] Obtenir l'approbation du plan
- [x] **Étape 1 : Modifier `JwtMessageHandler.cs`** - Ajouter la vérification d'expiration du token avant chaque requête + gestion des 401 ✅
- [x] **Étape 2 : Modifier `AuthMessageHandler.cs`** - Appliquer la même logique pour la cohérence ✅
- [x] **Étape 3 : Modifier `ProtectedPage.cs`** - Vérifier l'expiration du token (pas seulement son existence) ✅
- [x] **Étape 4 : Modifier `BaseApiService.cs`** - Ajouter la vérification d'expiration du token dans AddAuthHeader() utilisé par tous les services ✅
- [x] **Étape 5 : Mettre à jour les constructeurs de tous les services héritant de `BaseApiService`** pour inclure `NavigationManager` ✅
- [x] **Étape 6 : Tester la compilation** - Vérifier que le projet compile sans erreur ✅

