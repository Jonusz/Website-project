using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using TP_PWEB.Data;
using TP_PWEB.Models;
using TP_PWEB.ViewModels;

namespace TP_PWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string? TextoAPesquisar, string? estado, string? categoriaId, string? empresaId, string? ordem)
        {
            var ListaOrdem = new List<SelectListItem> {
                new SelectListItem { Text = "Preço (Ascendente)", Value = "Preço (Ascendente)", Selected = ordem == "Preço (Ascendente)" ? true : false },
                new SelectListItem { Text = "Preço (Descendente)", Value = "Preço (Descendente)", Selected = ordem == "Preço (Descendente)" ? true : false },
                new SelectListItem { Text = "Classificação (Descendente)", Value = "Classificação (Descendente)", Selected = ordem == "Classificação (Descendente)" ? true : false },
                new SelectListItem { Text = "Classificação (Ascendente)", Value = "Classificação (Ascendente)", Selected = ordem == "Classificação (Ascendente)" ? true : false }
            };

            var ListaCategorias = new List<SelectListItem>
            {
                new SelectListItem { Text = "Todas", Value = "-1", Selected = categoriaId == "-1" ? true : false }
            };
            foreach (VehicleCategory cat in _context.VehicleCategory)
            {
                ListaCategorias.Add(new SelectListItem { Text = cat.Name, Value = cat.Id.ToString(), Selected = categoriaId == cat.Id.ToString() ? true : false });
            }

            var ListaEmpresas = new List<SelectListItem>
            {
                new SelectListItem { Text = "Todas", Value = "-1", Selected = empresaId == "-1" ? true : false }
            };
            foreach (Company emp in _context.Company)
            {
                ListaEmpresas.Add(new SelectListItem { Text = emp.Name, Value = emp.Id.ToString(), Selected = empresaId == emp.Id.ToString() ? true : false });
            }

            ViewData["ListaOrdem"] = ListaOrdem;
            ViewData["ListaCategorias"] = ListaCategorias;
            ViewData["ListaEmpresas"] = ListaEmpresas;
            ViewData["TextoPesquisado"] = TextoAPesquisar;

            var veiculos = await _context.Vehicle.Include(v => v.Category).Include(v => v.Company).Where(v => v.Available == true).ToListAsync();

            if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
            {
                veiculos = veiculos.Where(v => v.Brand.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                            || v.Brand.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                            || v.Location.ToUpper().Contains(TextoAPesquisar.ToUpper())).ToList();
            }

            if (categoriaId != null)
            {
                int catId;
                try
                {
                    catId = int.Parse(categoriaId);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Erro a converter '{categoriaId}'");
                    return NotFound();
                }

                if (catId != -1)
                {
                    veiculos = veiculos.Where(v => v.CategoryID == catId).ToList();
                }
            }

            if (empresaId != null)
            {
                int empId;
                try
                {
                    empId = int.Parse(empresaId);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Erro a converter '{empresaId}'");
                    return NotFound();
                }

                if (empId != -1)
                {
                    veiculos = veiculos.Where(v => v.CompanyId == empId).ToList();
                }
            }

            if (ordem == "Preço (Descendente)")
                veiculos = veiculos.OrderByDescending(v => v.Price).ToList();
            else if (ordem == "Classificação (Descendente)")
                veiculos = veiculos.OrderByDescending(v => v.Company.rating).ToList();
            else if (ordem == "Classificação (Ascendente)")
                veiculos = veiculos.OrderBy(v => v.Company.rating).ToList();
            else
                veiculos = veiculos.OrderBy(v => v.Price).ToList();


            return View(veiculos);
        }

        // GET: Veiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vehicle == null)
            {
                return NotFound();
            }

            var veiculo = await _context.Vehicle
                .Include(v => v.Category)
                .Include(v => v.Company)
                .Where(v => v.Available == true)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (veiculo == null)
            {
                return NotFound();
            }

            return View(veiculo);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}