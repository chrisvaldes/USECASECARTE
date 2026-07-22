using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Use_Case_Carte.Components;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// Enregistrer les services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ProfilService>();
builder.Services.AddScoped<SafeJs>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<NouveauMagService>();
builder.Services.AddScoped<DetailReclamationService>();
builder.Services.AddScoped<TypeMagService>();
builder.Services.AddScoped<DashboardService>(); 
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>(); 
builder.Services.AddScoped<PermissionServiceAuth>(); 
builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5229/"),
});

builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    })
    .AddHubOptions(options =>
    { 
        options.MaximumReceiveMessageSize = 500 * 1024 * 1024; 
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
