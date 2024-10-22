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
    [Authorize (Roles = "Manager")]
    public class WorkerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public WorkerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? TextoAPesquisar, string? estado)
        {
			var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa = null;

            //veryfication manager of company
            if(gestor == null)
            {
                return NotFound();
            }
            else if((empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
				return NotFound();
			}

            var funcionarios = await _userManager.Users.Where(u => u.CompanyID== gestor.CompanyID)
                                                        .OrderBy(u => u.FirstName).ToListAsync();

			var ListaEstados = new List<SelectListItem> {
                new SelectListItem { Text = "Todos", Value = "" , Selected = estado == "" ? true : false},
                new SelectListItem { Text = "Ativos", Value = true.ToString(), Selected = estado == "True" ? true : false },
                new SelectListItem { Text = "Inativos", Value = false.ToString(), Selected = estado == "False" ? true : false }
            };

			ViewData["NomeEmpresa"] = empresa.Name;
			ViewData["ListaEstados"] = ListaEstados;
            ViewData["TextoPesquisado"] = TextoAPesquisar;

            if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
            {
                funcionarios = funcionarios.Where(f => f.FirstName.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                    || f.LastName.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                    || f.Email.ToUpper().Contains(TextoAPesquisar.ToUpper())).ToList();
                TextoAPesquisar = " que contêm \"" + TextoAPesquisar + "\"";
            }

            if (estado != null)
            {
                if (estado == true.ToString())
                    ViewData["Resultado"] = "Lista de Funcionários Ativos" + TextoAPesquisar;
                else
                    ViewData["Resultado"] = "Lista de Funcionários Inativos" + TextoAPesquisar;

                funcionarios = funcionarios.Where(f => f.Active == Convert.ToBoolean(estado)).ToList();
            }
            else
                ViewData["Resultado"] = "Lista de Funcionários" + TextoAPesquisar;

            var temp = new List<ApplicationUser>();
            foreach (ApplicationUser user in funcionarios)
            {
                if (await _userManager.IsInRoleAsync(user, "Worker"))
                {
                    temp.Add(user);
                }
            }
            funcionarios = temp;

            ViewBag.Total = funcionarios.Count;

            var userRolesViewModel = new List<UserManagerViewModel>();
            foreach (ApplicationUser user in funcionarios)
            {
                var utilizadorVM = new UserManagerViewModel();
                utilizadorVM.UserId = user.Id;
                utilizadorVM.UserName = user.UserName;
                utilizadorVM.FirstName = user.FirstName;
                utilizadorVM.LastName = user.LastName;
                utilizadorVM.Active = user.Active;
                utilizadorVM.RolesNames = await GetUserRoles(user);
                userRolesViewModel.Add(utilizadorVM);
            }

            return View(userRolesViewModel);
        }

        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

		public async Task<IActionResult> AtivarInativar(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return View();
			}

            user.Active = !user.Active;

            await _userManager.UpdateAsync(user);

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (userId == null || user == null)
			{
				return NotFound();
			}

			var reservas = _context.Reservation.Where(r => r.DeliveryWorkerId == userId || r.PickUpWorkerID == userId).Count();
			if (reservas > 0)
			{
				ViewBag.PodeApagar = false;
			}
			else
			{
				ViewBag.PodeApagar = true;
			}

			return View(user);
		}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (id != null && user != null)
            {
                await _userManager.DeleteAsync(user);
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            ViewBag.ListaErros = new List<string>();

            var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa;

            if (gestorId == null || gestor == null || (empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
                return NotFound();
            }

            NewWorkerViewModel novoUtilizadorVM = new NewWorkerViewModel();
            novoUtilizadorVM.NomeEmpresa = empresa.Name;

            return View(novoUtilizadorVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewWorkerViewModel utilizadorVM, string cargo)
        {
            var gestorId = _userManager.GetUserId(User);
            var gestor = _context.Users.Find(gestorId);
            Company? empresa;

            if (gestorId == null || gestor == null || (empresa = _context.Company.Find(gestor.CompanyID)) == null)
            {
                return NotFound();
            }

            var novoUser = new ApplicationUser
            {
                UserName = utilizadorVM.Email,
                Email = utilizadorVM.Email,
                FirstName = utilizadorVM.PrimeiroNome,
                LastName = utilizadorVM.UltimoNome,
                Active = utilizadorVM.Ativo,
                CompanyID = empresa.Id,
                EmailConfirmed = true
            };

            var user = await _userManager.FindByEmailAsync(novoUser.Email);
            if (user == null)
            {
                var result = await _userManager.CreateAsync(novoUser, utilizadorVM.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(novoUser, cargo);
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

            utilizadorVM.NomeEmpresa = empresa.Name;

            return View(utilizadorVM);
        }
    }
}
