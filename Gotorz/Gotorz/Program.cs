using Gotorz.Client.Pages;

using Shared.Models;
using Gotorz.Components;
using Gotorz.Components.Account;
using Gotorz.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Services;
using Microsoft.AspNetCore.SignalR;
using Gotorz.Services;
using Microsoft.AspNetCore.Components;
using Gotorz.Services.Admin;
using Gotorz.Client.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        // Temporarily comment out authentication state that depends on Identity
        builder.Services.AddCascadingAuthenticationState();
        // builder.Services.AddScoped<IdentityUserAccessor>();
        // builder.Services.AddScoped<IdentityRedirectManager>();
        // builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

        // These services likely don't depend on the database
        builder.Services.AddHttpClient("AmadeusClient");
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Services.AddHttpClient<AmadeusAuthService>();

        // Services that might not directly depend on database
        builder.Services.AddScoped<IStripeService, StripeService>();
        builder.Services.AddSingleton<PricingService>();
        builder.Services.AddSingleton<AirportService>();

        // Comment out services that likely depend on database access
        // builder.Services.AddScoped<TravelPackageService>();
        // builder.Services.AddScoped<FlightService>();
        // builder.Services.AddScoped<HotelService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient",
                policy => policy
                    .AllowAnyOrigin() // For production deployment, consider restricting this to your actual domain
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        // Simplified authentication setup without database/identity
        builder.Services.AddAuthentication();

        // Comment out admin services that depend on database
        // builder.Services.AddScoped<RoleManagementService>();
        // builder.Services.AddScoped<AdminUserService>();
        // builder.Services.AddScoped<AdminRoleService>();
        // builder.Services.AddScoped<AdminBookingService>();
        // builder.Services.AddScoped<AdminActivityLogService>();
        // builder.Services.AddScoped<AdminDashboardService>();

        // COMMENTED OUT: Database connection and configuration
        // var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        // builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseSqlServer(connectionString));

        builder.Services.AddControllers();
        // builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // COMMENTED OUT: Identity configuration
        // builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
        //     .AddRoles<IdentityRole>()
        //     .AddEntityFrameworkStores<ApplicationDbContext>()
        //     .AddSignInManager()
        //     .AddDefaultTokenProviders();

        // builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AdditionalUserClaimsPrincipalFactory>();
        // builder.Services.AddScoped<ActivityLogService>();
        // builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        builder.Logging.AddConsole();

        // Comment out cookie authentication configuration
        // builder.Services.ConfigureApplicationCookie(options =>
        // {
        //     options.Events.OnRedirectToLogin = context =>
        //     {
        //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //         return Task.CompletedTask;
        //     };
        //     options.Events.OnRedirectToAccessDenied = context =>
        //     {
        //         context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //         return Task.CompletedTask;
        //     };
        // });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            // Comment out migrations middleware
            // app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors("AllowBlazorClient");

        // Use basic authentication middleware but functionality will be limited
        app.UseAuthentication();

        // USED FOR TESTING SIGNALR
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/chathub"))
            {
                context.Items["__AntiforgerySkipValidation__"] = true;
            }
            await next();
        });
        // TEST STUFF ENDS

        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Gotorz.Client._Imports).Assembly);

        // Comment out identity endpoints as they depend on database
        // app.MapAdditionalIdentityEndpoints();

        app.MapControllers();
        app.MapHub<ChatHub>("/chathub");

        app.Run();
    }
}