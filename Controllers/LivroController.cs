
using Bibliotec_MVC___manha.Data;
using Bibliotec_MVC___manha.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bibliotec_MVC___manha.Controllers;

// Permite o acesso apenas aos usuários autenticados (adm e aluno)
[Authorize(Roles = "Administrador, Aluno")]
public class LivroController : Controller
{
    private readonly AppDbContext _context;

    public LivroController(AppDbContext context)
    {
        _context = context;
    }

    // Lista os livros cadastrados no banco de dados.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        List<Livro> livros = await _context.Livro
            .Include(l => l.IdAutorNavigation)
            .Include(l => l.IdStatusNavigation)
            .Include(l => l.IdCategoria)
            .OrderBy(l => l.Titulo)
            .ToListAsync();
        
        if(User.IsInRole("Administrador"))
        {
            // retorna a view de listagem de adm enviando a lista de livros
            return View("ListagemAdministrador", livros);
        }

        // retorna a view de listagem de aluno enviando a lista de livros
        return View("ListagemAluno", livros);
    }
}