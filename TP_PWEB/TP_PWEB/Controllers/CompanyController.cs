using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TP_PWEB.Data;
using TP_PWEB.Models;
using TP_PWEB.ViewModels;

namespace TP_PWEB.Controllers
{
    [Authorize (Roles = "Administrator")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Company
        public async Task<IActionResult> Index(string? TextoAPesquisar, string? ordem, string? estado)
        {
            var ListaEmpresas = new List<Company>();

            var ListaOrdem = new List<SelectListItem> {
				new SelectListItem { Text = "Estado", Value = "Estado", Selected = ordem == "Estado" ? true : false },
				new SelectListItem { Text = "Classificação", Value = "Classificação" , Selected = ordem == "Classificação" ? true : false},
                new SelectListItem { Text = "Alfabética", Value = "Alfabética", Selected = ordem == "Alfabética" ? true : false }
            };

            var ListaEstados = new List<SelectListItem> {
                new SelectListItem { Text = "Todas", Value = "" , Selected = estado == "" ? true : false},
                new SelectListItem { Text = "Ativas", Value = true.ToString(), Selected = estado == "True" ? true : false },
                new SelectListItem { Text = "Inativas", Value = false.ToString(), Selected = estado == "False" ? true : false }
            };

            ViewData["ListaOrdem"] = ListaOrdem;
            ViewData["ListaEstados"] = ListaEstados;
            ViewData["TextoPesquisado"] = TextoAPesquisar;

            if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
            {
                ListaEmpresas = await _context.Company.Where(c => c.Name.ToUpper().Contains(TextoAPesquisar.ToUpper())).ToListAsync();
                TextoAPesquisar = " que contêm \"" + TextoAPesquisar + "\"";
            }
            else
                ListaEmpresas = await _context.Company.ToListAsync();

            if (estado != null)
            {
                if (estado == true.ToString())
                    ViewData["Resultado"] = "Lista de Empresas Ativas" + TextoAPesquisar;
                else
                    ViewData["Resultado"] = "Lista de Empresas Inativas" + TextoAPesquisar;

                ListaEmpresas = ListaEmpresas.Where(c => c.Active == Convert.ToBoolean(estado)).ToList();
            }
            else
                ViewData["Resultado"] = "Lista de Empresas" + TextoAPesquisar;

            if (ordem == "Classificação")
                ListaEmpresas = ListaEmpresas.OrderByDescending(e => e.rating).ToList();
            else if (ordem == "Alfabética")
				ListaEmpresas = ListaEmpresas.OrderBy(e => e.Name).ToList();
			else
                ListaEmpresas = ListaEmpresas.OrderByDescending(e => e.Active).ToList();

            return View(ListaEmpresas);
        }

        // GET: Company/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }

            var empresa = await _context.Company.FirstOrDefaultAsync(e => e.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }

            var empresaVM = new CompanyDetailsViewModel();
            var gestores = new List<ApplicationUser>();
            var funcionarios = new List<ApplicationUser>();

			var users = await _userManager.Users.OrderBy(u => u.FirstName).ToListAsync();
			foreach (ApplicationUser user in users)
            {
				if (user.CompanyID == id)
                {
                    if (await _userManager.IsInRoleAsync(user, "Manager"))
                        gestores.Add(user);
                    if (await _userManager.IsInRoleAsync(user, "Worker"))
                        funcionarios.Add(user);
				}
			}

            var total = _context.Vehicle.Where(v => v.CompanyId == id).Count();

			empresaVM.Company = empresa;
            empresaVM.TotalVehicles = total;
            empresaVM.Menagers = gestores;
            empresaVM.Workers = funcionarios;

            return View(empresaVM);
        }

        // GET: Company/Create
        public IActionResult Create()
        {
            ViewBag.ListaErros = new List<string>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyMenagerViewModel empresaVM)
        {
            var novaEmpresa = new Company
            {
                Name = empresaVM.NomeEmpresa,
                Active = empresaVM.AtivoEmpresa
            };

            ModelState.Remove(nameof(novaEmpresa.Workers));
            ModelState.Remove(nameof(novaEmpresa.Vehicles));
            if (ModelState.IsValid)
            {
                _context.Add(novaEmpresa);
                await _context.SaveChangesAsync();

                var gestor = new ApplicationUser
                {
                    UserName = empresaVM.EmailGestor,
                    Email = empresaVM.EmailGestor,
                    FirstName = empresaVM.PrimeiroNomeGestor,
                    LastName = empresaVM.UltimoNomeGestor,
                    Active = empresaVM.AtivoGestor,
                    CompanyID = novaEmpresa.Id,
                    EmailConfirmed = true
                };

                var user = await _userManager.FindByEmailAsync(gestor.Email);
                if (user == null)
                {
                    var result = await _userManager.CreateAsync(gestor, empresaVM.PasswordGestor);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(gestor, "Manager");
                        return RedirectToAction(nameof(Index));
                    }

                    var ListaErros = new List<string>();
                    foreach (var error in result.Errors)
                    {
                        //ModelState.AddModelError(string.Empty, error.Description);
                        ListaErros.Add(error.Description);
                    }
                    ViewBag.ListaErros = ListaErros;
                }
            }

            _context.Remove(novaEmpresa);
            await _context.SaveChangesAsync();

            return View(empresaVM);
        }

        // GET: Company/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }

            var empresa = await _context.Company.FindAsync(id);
            if (empresa == null)
            {
                return NotFound();
            }

            ViewBag.NomeEmpresa = empresa.Name;

            return View(empresa);
        }

        // POST: Company/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Clasyfication,Active")] Company empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(empresa.rating));
            ModelState.Remove(nameof(empresa.Workers));
            ModelState.Remove(nameof(empresa.Vehicles));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empresa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpresaExists(empresa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(empresa);
        }

        // GET: Company/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }

            var empresa = await _context.Company.FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }

            var totalVeiculos = _context.Vehicle.Where(v => v.CompanyId == id).Count();
            if (totalVeiculos > 0)
            {
                ViewBag.PodeApagar = false;
            }
            else
            {
                ViewBag.PodeApagar = true;
            }

            return View(empresa);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Company == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Empresa'  is null.");
            }

            var empresa = await _context.Company.FindAsync(id);
            if (empresa != null)
            {
                var results = await _userManager.Users.Where(u => u.CompanyID == id).ToListAsync();

                foreach (ApplicationUser user in results)
                {
                    user.CompanyID = null;
                }

                _context.Company.Remove(empresa);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpresaExists(int id)
        {
          return _context.Company.Any(e => e.Id == id);
        }
    }
}
