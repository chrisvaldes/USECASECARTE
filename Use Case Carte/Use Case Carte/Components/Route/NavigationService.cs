using System;
using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Components.Route;

public class NavigationService
{
    private readonly NavigationManager _navigation;

    public NavigationService(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    public void GoLogin()
    {
        _navigation.NavigateTo(Route.Login);
    }

    public void GoProfil()
    {
        _navigation.NavigateTo(Route.Profil);
    }

    public void GoCreerRole()
    {
        _navigation.NavigateTo(Route.CreateRoles);
    }

    public void GoListeUtilisateur()
    {
        _navigation.NavigateTo(Route.Utilisateurs);
    }

    public void GoNouveauUtilisateur()
    {
        _navigation.NavigateTo(Route.NouveauUtilisateur);
    }

    public void GoModifierUtilisateur(UserDto userDto)
    {
        _navigation.NavigateTo($"{Route.ModifierUtilisateur}/{userDto.Id}");
    }

    public void GoCreerProfil()
    {
        _navigation.NavigateTo(Route.CreateProfil);
    }

    public void GoModifierProfil(ProfilModel profilModel)
    {
        _navigation.NavigateTo($"{Route.ModifierProfil}/{profilModel.Id}");
    }

    public void GoSupprimerProfil()
    {
        _navigation.NavigateTo(Route.SupprimerProfil);
    }

    public void GoDashboard()
    {
        _navigation.NavigateTo(Route.Dashboard);
    }

    public void GoGestionMAG()
    {
        _navigation.NavigateTo(Route.GestionMAG);
    }

    public void GoNouveauMAG()
    {
        _navigation.NavigateTo(Route.NouveauMag);
    }

    public void DetailFacturation()
    {
        _navigation.NavigateTo(Route.DetailFacturation);
    }

    public void GoSyntheseMag(TypeMag typeMag)
    {
        _navigation.NavigateTo($"{Route.SyntheseMag}/{typeMag.Id}");
    }

    public void GoRole()
    {
        _navigation.NavigateTo(Route.Roles);
    }

    public void GoPermission()
    {
        _navigation.NavigateTo(Route.Permissions);
    }

    public void GoNouvellePermission()
    {
        _navigation.NavigateTo(Route.NouvellePermission);
    }

    public void GoModifierRole(string roleId)
    {
        _navigation.NavigateTo($"{Route.ModifierRole}/{roleId}");
    }
}
