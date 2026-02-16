using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OutlookPlugin;
using OutlookPlugin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register AppState as a singleton for global state management
builder.Services.AddSingleton<AppState>();

// Register OutlookService for Office.js interop
builder.Services.AddScoped<OutlookService>();

await builder.Build().RunAsync();
