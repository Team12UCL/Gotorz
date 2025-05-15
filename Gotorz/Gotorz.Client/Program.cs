using Gotorz.Client;
using Gotorz.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddSingleton<TravelPackageService>();
builder.Services.AddSingleton<PricingService>();

// Get base address from configuration or use relative path
builder.Services.AddScoped(sp =>
{
    // In production, use relative URLs that will resolve to the same domain
    return new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

await builder.Build().RunAsync();