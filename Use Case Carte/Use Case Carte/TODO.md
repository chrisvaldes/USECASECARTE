# Plan de correction — Sidebar (Aside) avec permissions + UI fonctionnelle

## Problème
- **Avec** `@rendermode` : Permissions OK ✅ mais sidebar figée ❌
- **Sans** `@rendermode` : Sidebar active ✅ mais permissions vides ❌

## Cause
Quand `@rendermode` est actif, Blazor remplace le DOM de la sidebar après le rendu initial → les événements JS (clics, SimpleBar) sont perdus.

## ✅ Correction appliquée

### Problème : Onglet actif invisible dans la sidebar
**Cause** : `reinitUi()` dans `blazor-init.js` appelait `setNavActive()`, une fonction qui **écrase** les classes `active` que Blazor applique via `NavLink.ActiveClass`.

### Correction : `wwwroot/js/blazor-init.js`
- **Supprimé** l'appel à `setNavActive()` dans `reinitUi()` 
- Blazor gère déjà correctement l'onglet actif via `NavLink` avec `ActiveClass="active"`
- La fonction `setNavActive()` n'est plus nécessaire et interfère avec Blazor

### Résultat final
- ✅ Permissions chargées et affichées
- ✅ Sidebar interactive (SimpleBar, clics, dropdowns)
- ✅ Onglet actif visible (classe `active` conservée par Blazor)

