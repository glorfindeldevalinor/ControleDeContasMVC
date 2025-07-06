// ---------------------------------------------------------------------------------
// ARQUIVO: Program.cs (Configura��o inicial do App MVC)
// ---------------------------------------------------------------------------------
// Adicione a linha 'builder.Services.AddDbContext<AppDbContext>();'
// e a se��o para criar o banco de dados na inicializa��o.

using ControleDeContasMVC.Contexto;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Adiciona servi�os ao cont�iner.
builder.Services.AddControllersWithViews();

// *** ADICIONE ESTA LINHA PARA REGISTRAR O DBCONTEXT ***
builder.Services.AddDbContext<AppDbContext>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Este comando aplica as migra��es pendentes, em vez de apenas criar o banco.
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Contas}/{action=Index}/{id?}");

app.Run();