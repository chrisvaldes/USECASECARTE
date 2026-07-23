# Progression des modifications

## 1️⃣ Token expiré → Redirection vers la page de connexion

### Fichiers modifiés :

| Fichier | Modification |
|---------|-------------|
| **`JwtMessageHandler.cs`** | Vérification expiration token **avant** chaque requête HTTP. Si expiré → suppression token + redirection `/`. Gestion des réponses 401. |
| **`AuthMessageHandler.cs`** | Même logique appliquée pour la cohérence (deuxième handler HTTP). |
| **`JwtAuthenticationStateProvider.cs`** | Vérification token expiré via `IsTokenExpired()` + `RedirectToLoginIfNeeded()` (déjà en place). |
| **`ProtectedPage.cs`** | Vérifie l'existence du token (déjà en place). |

### Comment ça marche :
1. Avant chaque appel API → le handler vérifie si le token est expiré
2. Si expiré → token supprimé, utilisateur redirigé vers `/`
3. Si le serveur répond 401 → même traitement
4. `MainLayout.razor.cs` a déjà un timer qui vérifie l'état d'authentification toutes les 30s

## 2️⃣ Redirection post-connexion basée sur les permissions

### Fichier modifié :

| Fichier | Modification |
|---------|-------------|
| **`Home.razor.cs`** | Nouvelle méthode `GetRedirectUrlFromToken()` qui décode le JWT et redirige selon les permissions. |

### Logique de redirection :
| Permission | Route |
|-----------|-------|
| `UTILISATEUR` | `/MAG/utilisateurs` |
| `TYPEMAG` ou `BKMVTI` | `/MAG/dashboard` |
| `BKMVTI_CONSULTER` | `/MAG/dashboard` |
| Par défaut | `/MAG/dashboard` |

## ✅ Build : Succès

