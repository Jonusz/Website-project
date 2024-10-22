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
    [Authorize (Roles = "Worker,Manager")]
    public class VehicleFleetManagment : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleFleetManagment(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GestaoFrotaVeiculos
        public async Task<IActionResult> Index(string? TextoAPesquisar, string? estado, string? categoriaId, string? ordem)
        {
            var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa = null;

            //verificar se user existe e pertence a uma empresa 
            if (gestor == null) 
            {
                return NotFound();
            }
            else if ((empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
                return NotFound();
            }

            var ListaOrdem = new List<SelectListItem> {
                new SelectListItem { Text = "Available", Value = "Available", Selected = ordem == "Available" ? true : false },
                new SelectListItem { Text = "Price", Value = "Price", Selected = ordem == "Price" ? true : false },
                new SelectListItem { Text = "Brand", Value = "Brand", Selected = ordem == "Brand" ? true : false },
                new SelectListItem { Text = "Category", Value = "Category" , Selected = ordem == "Category" ? true : false},
                new SelectListItem { Text = "kilometers", Value = "kilometers", Selected = ordem == "kilometers" ? true : false }
            };

            var ListaEstados = new List<SelectListItem> {
                new SelectListItem { Text = "Todos", Value = "" , Selected = estado == "" ? true : false},
                new SelectListItem { Text = "Disponiveis", Value = true.ToString(), Selected = estado == "True" ? true : false },
                new SelectListItem { Text = "Indisponiveis", Value = false.ToString(), Selected = estado == "False" ? true : false }
            };

            var ListaCategorias = new List<SelectListItem>
            {
                new SelectListItem { Text = "Todas", Value = "-1", Selected = categoriaId == "-1" ? true : false }
            };
            foreach (VehicleCategory cat in _context.VehicleCategory)
            {
                ListaCategorias.Add(new SelectListItem { Text = cat.Name, Value = cat.Id.ToString(), Selected = categoriaId == cat.Id.ToString() ? true : false });
            }

            ViewData["NomeEmpresa"] = empresa.Name;
            ViewData["ListaOrdem"] = ListaOrdem;
            ViewData["ListaEstados"] = ListaEstados;
            ViewData["ListaCategorias"] = ListaCategorias;
            ViewData["TextoPesquisado"] = TextoAPesquisar;

            //apenas veiculos da mesma empresa de funcionario/gestor
            var veiculos = await _context.Vehicle.Include(v => v.Category).Where(v => v.CompanyId == gestor.CompanyID).ToListAsync();

            if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
            {
                veiculos = veiculos.Where(v => v.Brand.ToUpper().Contains(TextoAPesquisar.ToUpper()) 
                                            || v.Model.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                            || v.Location.ToUpper().Contains(TextoAPesquisar.ToUpper())).ToList();
                TextoAPesquisar = " que contêm \"" + TextoAPesquisar + "\"";
            }

            if (estado != null)
            {
                if (estado == true.ToString())
                    ViewData["Resultado"] = "Lista de Veículos Disponiveis" + TextoAPesquisar;
                else
                    ViewData["Resultado"] = "Lista de Veículos Indisponiveis" + TextoAPesquisar;

                veiculos = veiculos.Where(v => v.Available == Convert.ToBoolean(estado)).ToList();
            }
            else
                ViewData["Resultado"] = "Lista de Veículos" + TextoAPesquisar;

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

            if (ordem == "Price")
                veiculos = veiculos.OrderBy(v => v.Price).ToList();
            else if (ordem == "Brand")
                veiculos = veiculos.OrderBy(v => v.Brand).ToList();
            else if(ordem == "Category")
                veiculos = veiculos.OrderBy(v => v.CategoryID).ToList();
            else if (ordem == "kilometers")
                veiculos = veiculos.OrderByDescending(v => v.Kms).ToList();
            else
                veiculos = veiculos.OrderByDescending(v => v.Available).ToList();

            return View(veiculos);
        }

        // GET: GestaoFrotaVeiculos/Create
        public IActionResult Create()
        {
            var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa;

            if (gestorId == null || gestor == null || (empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
                return NotFound();
            }

            ViewData["CompanyName"] = empresa.Name;
            ViewData["CategoriaId"] = new SelectList(_context.VehicleCategory, "Id", "Name");

            return View();
        }

        // POST: GestaoFrotaVeiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Brand,Model,Seats,Price,Available,Kms,Location,CompanyId,CategoryID")] Vehicle veiculo)
        {
            var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa;

            if (gestorId == null || gestor == null || (empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(veiculo.Company));
            ModelState.Remove(nameof(veiculo.Category));
            if (ModelState.IsValid && veiculo.Price > 0 && veiculo.Kms > 0)
            {
                veiculo.CompanyId = empresa.Id;
                _context.Add(veiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.VehicleCategory, "Id", "Name", veiculo.CategoryID);
           
            return View(veiculo);
        }

        // GET: GestaoFrotaVeiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vehicle == null)
            {
                return NotFound();
            }

            var veiculo = await _context.Vehicle.FindAsync(id);
            if (veiculo == null)
            {
                return NotFound();
            }

            ViewData["CategoriaId"] = new SelectList(_context.VehicleCategory, "Id", "Name", veiculo.CategoryID);
            
            return View(veiculo);
        }

        // POST: GestaoFrotaVeiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Seats,Price,Available,Kms,Location,CategoryID")] Vehicle veiculo)
        {
            var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa;

            if (gestorId == null || gestor == null || (empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
                return NotFound();
            }
            if (id != veiculo.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(veiculo.Company));
            ModelState.Remove(nameof(veiculo.Category));

            if (ModelState.IsValid)
            {
                try
                {
                    veiculo.CompanyId = empresa.Id;
                    _context.Update(veiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VeiculoExists(veiculo.Id))
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

            ViewData["CategoriaId"] = new SelectList(_context.VehicleCategory, "Id", "Name", veiculo.CategoryID);
            
            return View(veiculo);
        }

        // GET: GestaoFrotaVeiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vehicle == null)
            {
                return NotFound();
            }

            var veiculo = await _context.Vehicle.Include(v => v.Category).Include(v => v.Company).FirstOrDefaultAsync(m => m.Id == id);
            if (veiculo == null)
            {
                return NotFound();
            }

            var reservas = _context.Reservation.Where(r => r.VehicleId == id && (r.ReservationState == 0 || r.ReservationState == 1)).Count();
            if (reservas > 0)
            {
                ViewBag.PodeApagar = false;
            }
            else
            {
                ViewBag.PodeApagar = true;
            }

            return View(veiculo);
        }

        // POST: GestaoFrotaVeiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vehicle == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Veiculo'  is null.");
            }
            var veiculo = await _context.Vehicle.FindAsync(id);
            if (veiculo != null)
            {
                _context.Vehicle.Remove(veiculo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VeiculoExists(int id)
        {
          return _context.Vehicle.Any(e => e.Id == id);
        }
    }
}
