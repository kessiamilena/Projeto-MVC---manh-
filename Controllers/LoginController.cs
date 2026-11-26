
using Bibliotec_MVC___manha.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotec_MVC___manha.Controllers;

public class LoginController : Controller
{
    // Mostra a visualização da tela de login
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // recebe os dados enviados pelo formulário
    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
        // verifica se os dados recebidos atendem as regras de validação.
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        // mensagem temporária para testar
        ModelState.AddModelError("", "Autenticação ainda não implementada");

        return View(model);
    }
}