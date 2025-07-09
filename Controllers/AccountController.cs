using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

public class AccountController : Controller
{
    [AllowAnonymous] // Permite que usuários não logados acessem a página de login
    public IActionResult Login(string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string pin, string returnUrl = "/")
    {
        // Nosso PIN secreto
        string pinCorreto = "098820";

        if (pin == pinCorreto)
        {
            // A autenticação foi um sucesso. Vamos criar a "identidade" do usuário.
            var claims = new List<Claim>
            {
                // Podemos adicionar informações sobre o usuário aqui.
                // Por enquanto, apenas um nome de usuário genérico.
                new Claim(ClaimTypes.Name, "Usuário Padrão"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // O cookie persiste mesmo se o navegador for fechado
            };

            // Realiza o login, criando o cookie de autenticação
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return LocalRedirect(returnUrl);
        }

        // Se o PIN estiver incorreto, mostra uma mensagem de erro na tela de login
        ViewData["ErrorMessage"] = "PIN inválido. Tente novamente.";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        // Realiza o logout, limpando o cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}