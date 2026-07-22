# Plan de correction — Timeout HttpClient 100s

## Problème
Le `HttpClient` côté Blazor Frontend a un timeout par défaut de **100 secondes**. Quand l'API prend plus de temps (traitement de fichiers volumineux, requêtes DB complexes), la requête est annulée avec l'erreur :
> "The request was canceled due to the configured HttpClient.Timeout of 100 seconds elapsing"

## Tâches

### ✅ Étape 1 : Frontend — Augmenter le timeout HttpClient
- Fichier : `Use Case Carte/Use Case Carte/Program.cs`
- ✅ Ajouté `.Timeout = TimeSpan.FromMinutes(10)` au HttpClient
- ✅ Supprimé la double inscription de `PermissionService`

### ✅ Étape 2 : API — Configurer Kestrel timeout
- Fichier : `API/API/Program.cs`
- ✅ Ajouté `KeepAliveTimeout = 15 min`
- ✅ Ajouté `RequestHeadersTimeout = 15 min`

### 🎯 Résultat final
Les deux modifications sont en place. Le loader Blazor (via `toggleOnLoaderAndToast` / `toggleOffLoaderAndToast`) restera actif tant que l'API n'aura pas répondu, car :
1. Le **HttpClient** ne coupera plus la requête avant 10 minutes
2. **Kestrel** ne coupera pas non plus la connexion avant 15 minutes
3. Le loader est géré avec `try/finally` ou `try/catch/finally` → le `toggleOffLoaderAndToast` est toujours exécuté

