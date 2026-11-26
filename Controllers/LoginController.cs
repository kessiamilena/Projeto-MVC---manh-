
using System.Runtime.InteropServices.Marshalling;
using System.Security.Claims;
using Bibliotec_MVC___manha.Data;
using Bibliotec_MVC___manha.Models;
using Bibliotec_MVC___manha.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bibliotec_MVC___manha.Controllers;

public class LoginController : Controller
{
    // RECEBENDO DEPENDÊNCIAS (registradas na program)
    // permite consultar e manipular os dados do banco
    private readonly AppDbContext _context;
    // permite gerar e validar o hash das senhas
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public LoginController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // Mostra a visualização da tela de login
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // recebe os dados enviados pelo formulário
    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        // verifica se os dados recebidos atendem as regras de validação.
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        // mensagem temporária para testar
        // ModelState.AddModelError("", "Autenticação ainda não implementada");

        // Busca um usuário pelo email informado e verifica se ele não foi excluido
        // se nao encontrar, retorna null
        Usuario usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.Excluido == false);

        // Verifica se o usuário não foi encontrado
        if(usuario == null)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(model);
        }

        // Verifica se a senha informada corresponde ao hash armazenado.
        // o método recebe 3 parâmetros
        var resultadoSenha = _passwordHasher.VerifyHashedPassword(
            usuario,        // objeto que representa o usuário encontrado no banco.
            usuario.Senha,  // hash da senha armazenada no banco de dados.
            model.Senha     // senha digitada pelo usuário no formulário de login.
        );

        // verifica se a senha está incorreta
        if(resultadoSenha == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(model);
        }

        // verifica se a senha está correta, mas o hash precisa ser atualizado
        if(resultadoSenha == PasswordVerificationResult.Success)
        {
            // gera um novo hash da senha utilizando as configurações de segurança atuais.
            usuario.Senha = _passwordHasher.HashPassword(usuario, model.Senha);

            // Salva o novo hash no banco de dados
            await _context.SaveChangesAsync();
        }

        // Verifica se o usuário está cadastrado como administrador
        // verifica dentro da tabela adm se o usuário possui registro
        // retorna true se existir pelo menos um registro que atende a condição informada.
        bool administrador = await _context.Administrador
            .AnyAsync(a => a.IdAdministrador == usuario.IdUsuario);

        // verifica se o usuário está cadastrado como aluno
        bool aluno = await _context.Aluno.AnyAsync(a => a.IdAluno == usuario.IdUsuario);

        // coloca o tipo do perfil de usuário na variavel tipoUsuario
        string? tipoUsuario = administrador ? "Administrador" : aluno ? "Aluno" : null;

        // Cria uma lista de Claims com as informações do usuário autenticado
        // claims -> são informações do usuário, que permitem identificar e controlar o acesso.
        var claims = new List<Claim>
        {
            // Armazena o identificador(id) do usuário
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),

            // Armazena o nome do usuário
            new Claim(ClaimTypes.Name, usuario.Nome),

            // Armazena o e-mail do usuário
            new Claim(ClaimTypes.Email, usuario.Email),

            // Armazena o tipo de usuário para controle de acesso
            new Claim(ClaimTypes.Role, tipoUsuario)
        };

        // Cria a identidade do usuário utilizando as Claims
        // ClaimsIdentity -> como se fosse uma carteirinha de identificação do usuário dentro do sistema
        // cookies -> informa que estamos utilizando o tipo de autenticação por cookies.
        var identidade = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        // Representa o usuário autenticado
        // classe do asp.net para representar o usuário
        var principal = new ClaimsPrincipal(identidade);

        // Realiza a autenticação e gera o cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return RedirectToAction("Index", "Home");
    }
}