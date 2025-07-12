using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ControleDeContasMVC.Contexto;
using ControleDeContasMVC.Repositories;
using ControleDeContasMVC.Services;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 1. Configura��o do Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configura��o do ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

// 3. Configura��o da Autentica��o Externa (Google)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

// 4. Registro dos Reposit�rios e Servi�os (Inje��o de Depend�ncia)
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IContaService, ContaService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Bloco para aplicar migra��es na inicializa��o
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilita a autentica��o e autoriza��o, nesta ordem
app.UseAuthentication();
app.UseAuthorization();

// Rota padr�o apontando para a p�gina Home
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapeia as p�ginas do Identity (Login, etc.)
app.MapRazorPages();

app.Run();