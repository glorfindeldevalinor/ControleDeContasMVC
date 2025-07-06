// ---------------------------------------------------------------------------------
// ARQUIVO: Program.cs (Configura��o inicial do App MVC)
// ---------------------------------------------------------------------------------
// Adicione a linha 'builder.Services.AddDbContext<AppDbContext>();'
// e a se��o para criar o banco de dados na inicializa��o.

using ControleDeContasMVC.Contexto;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Adiciona servi�os ao cont�iner.
builder.Services.AddControllersWithViews();

// *** ADICIONE ESTA LINHA PARA REGISTRAR O DBCONTEXT ***
builder.Services.AddDbContext<AppDbContext>();


var app = builder.Build();

// *** ADICIONE ESTE BLOCO PARA CRIAR O BD NA INICIALIZA��O ***
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
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