using CSHLDraft.Hubs;
using CSHLDraft.Components;
using CSHLDraft.Data;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddMudServices();

builder.Services.AddScoped<ICSHLData>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionString__CSHL");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("SET YOUR CONNECTION STRING!\n");
    }
    
    return new CSHLData(connectionString!);
});


builder.Services.AddScoped(provider =>
{
    var url = Environment.GetEnvironmentVariable("CSHL_SUPABASE_URL");
    var key = Environment.GetEnvironmentVariable("CSHL_SUPABASE_KEY");

    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
    {
        throw new InvalidOperationException("SET YOUR SUPABASE AUTH ENV VARIABLES!\n");
    }
            
    return new Supabase.Client(url, key, new SupabaseOptions
    {
        AutoConnectRealtime = true,
        AutoRefreshToken = false,
    });
});

builder.Services.AddScoped<CsvParser>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = new PathString("/login");
            options.AccessDeniedPath = new PathString("/auth/denied");
        });
builder.Services.AddScoped<CustomUserService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<DraftHub>(DraftHub.HubUrl);

app.Run();