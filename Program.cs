using Bibliotec_MVC___manha.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexaoPadrao")
    )
);

// Configura a autenticação utilizando cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Página para qual o usuário será direcionado caso não esteja autenticado.
        options.LoginPath = "/Login/Index";

        // Página para qual o usuário será direcionado caso não tenha permissão
        options.AccessDeniedPath = "/Login/AcessoNegado";

        // Define o tempo de validade da autenticação
        // inatividade
        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        // Renova a validade da autenticação conforme a atividade do usuário.
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// A ordem é importante -> primeiro identifica o usuário, depois ve se ele tem permissão.
// primeiro UseAuthentication();
// depois -> app.UseAuthorization();

// Identifica o usuário do cookie de autenticação
app.UseAuthentication();

// Verifica se o usuário possui permissão para acessar o recurso solicitado.
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

