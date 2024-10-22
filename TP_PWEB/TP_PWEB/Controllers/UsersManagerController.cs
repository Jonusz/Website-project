using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;
using TP_PWEB.Models;
using TP_PWEB.ViewModels;

namespace TP_PWEB.Controllers
{
    [Authorize (Roles = "Administrator")]
    public class UsersManagerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersManagerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? TextoAPesquisar, string? estado, string? cargo)
        {
			var users = new List<ApplicationUser>();

			var ListaEstados = new List<SelectListItem> {
				new SelectListItem { Text = "Todos", Value = "" , Selected = estado == "" ? true : false},
				new SelectListItem { Text = "Ativos", Value = true.ToString(), Selected = estado == "True" ? true : false },
				new SelectListItem { Text = "Inativos", Value = false.ToString(), Selected = estado == "False" ? true : false }
			};

			var ListaCargos = new List<SelectListItem> {
				new SelectListItem { Text = "Todos", Value = "Todos", Selected = cargo == "Todos" ? true : false },
				new SelectListItem { Text = "Cliente", Value = "Cliente" , Selected = cargo == "Cliente" ? true : false},
				new SelectListItem { Text = "Funcionario", Value = "Funcionario" , Selected = cargo == "Funcionario" ? true : false},
				new SelectListItem { Text = "Gestor", Value = "Gestor" , Selected = cargo == "Gestor" ? true : false},
				new SelectListItem { Text = "Administrador", Value = "Administrador", Selected = cargo == "Administrador" ? true : false }
			};

			ViewData["ListaCargos"] = ListaCargos;
			ViewData["ListaEstados"] = ListaEstados;
			ViewData["TextoPesquisado"] = TextoAPesquisar;

			if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
			{
				users = _userManager.Users.Include(a => a.Company).Where(a => a.FirstName.Contains(TextoAPesquisar) 
														|| a.LastName.Contains(TextoAPesquisar)
														|| a.Email.Contains(TextoAPesquisar)).
														OrderBy(a => a.FirstName).ToList();
				TextoAPesquisar = " que contêm \"" + TextoAPesquisar + "\"";
			}
			else
				users = _userManager.Users.Include(a => a.Company).OrderBy(a => a.Company).ToList();

			if (estado != null)
			{
				if (estado == true.ToString())
					ViewData["Resultado"] = "Lista de Utilizadores Ativos" + TextoAPesquisar;
				else
					ViewData["Resultado"] = "Lista de Utilizadores Inativos" + TextoAPesquisar;

				users = users.Where(c => c.Active == Convert.ToBoolean(estado)).ToList();
			}
			else
				ViewData["Resultado"] = "Lista de Utilizadores" + TextoAPesquisar;

			if (cargo != null && cargo != "Todos")
			{
				var temp = new List<ApplicationUser>();
				foreach (ApplicationUser user in users)
				{
					if (await _userManager.IsInRoleAsync(user, cargo))
					{
						temp.Add(user);
					}
				}
				users = temp;
			}

			ViewBag.Total = users.Count;

			var userRolesViewModel = new List<UserManagerViewModel>();
            foreach (ApplicationUser user in users)
            {
                var utilizadorVM = new UserManagerViewModel();
                utilizadorVM.UserId = user.Id;
                utilizadorVM.UserName = user.UserName;
                utilizadorVM.FirstName = user.FirstName;
                utilizadorVM.LastName = user.LastName;
                utilizadorVM.Active = user.Active;
                utilizadorVM.RolesNames = await GetUserRoles(user);
                if (await _userManager.IsInRoleAsync(user, "Worker") || await _userManager.IsInRoleAsync(user, "Manager"))
                {
					if(user.Company != null) { 
						utilizadorVM.CompanyName = user.Company.Name;
					}
				}
                userRolesViewModel.Add(utilizadorVM);
            }

            return View(userRolesViewModel);
        }

        private async Task<List<string>> GetUserRoles(ApplicationUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }

		public async Task<IActionResult> Edit(string userId)
		{
			ViewBag.userId = userId;

			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return View("Not Found");
			}

			ViewBag.UserName = user.UserName;

			var utilizadorVM = new UserManagerViewModel();
            var listaRoles = new List<RolesData>();

			foreach (var role in _roleManager.Roles)
			{
				var userRolesViewModel = new RolesData
				{
					RoleId = role.Id,
					RoleName = role.Name
				};

				if (await _userManager.IsInRoleAsync(user, role.Name))
					userRolesViewModel.Selected = true;
				else
					userRolesViewModel.Selected = false;

				listaRoles.Add(userRolesViewModel);
			}

            utilizadorVM.FirstName = user.FirstName;
            utilizadorVM.LastName = user.LastName;
            utilizadorVM.Active = user.Active;
            utilizadorVM.ListRoles = listaRoles;

            return View(utilizadorVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserManagerViewModel model, string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return View();
			}

			user.FirstName = model.FirstName;
			user.LastName = model.LastName;
			user.Active = model.Active;

			var roles = await _userManager.GetRolesAsync(user);
			var result = await _userManager.RemoveFromRolesAsync(user, roles);

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot remove user existing roles");
				return View(model);
			}

			result = await _userManager.AddToRolesAsync(user, model.ListRoles.Where(x => x.Selected).Select(y => y.RoleName));

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot add selected roles to user");
				return View(model);
			}

			return RedirectToAction("Index");
		}
    }
}
