using Gotorz.Client.Services;
using Gotorz.Components.Account;
using Gotorz.Components;
using Gotorz.Data;
using Gotorz.Services.Admin;
using Gotorz.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Services;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

        builder.Services.AddHttpClient("AmadeusClient");
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.Services.AddHttpClient<AmadeusAuthService>();

        builder.Services.AddScoped<IStripeService, StripeService>();
        builder.Services.AddSingleton<PricingService>();
        builder.Services.AddScoped<TravelPackageService>();
        builder.Services.AddScoped<FlightService>();
        builder.Services.AddScoped<HotelService>();
        builder.Services.AddSingleton<AirportService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient",
                policy => policy
                    .WithOrigins("https://gotorz20250514095933.azurewebsites.net")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();


        builder.Services.AddScoped<RoleManagementService>();

        builder.Services.AddScoped<AdminUserService>();
        builder.Services.AddScoped<AdminRoleService>();
        builder.Services.AddScoped<AdminBookingService>();
        builder.Services.AddScoped<AdminActivityLogService>();
        builder.Services.AddScoped<AdminDashboardService>();



        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddControllers();
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AdditionalUserClaimsPrincipalFactory>();

        builder.Services.AddScoped<ActivityLogService>();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        builder.Logging.AddConsole();

        //builder.Services.ConfigureApplicationCookie(options =>
        //{
        //    options.Events.OnRedirectToLogin = context =>
        //    {
        //        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //        return Task.CompletedTask;
        //    };
        //    options.Events.OnRedirectToAccessDenied = context =>
        //    {
        //        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //        return Task.CompletedTask;
        //    };
        //});

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
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

        app.MapAdditionalIdentityEndpoints();

        app.MapControllers();
        app.MapHub<ChatHub>("/chathub");

        app.Run();
    }
}
