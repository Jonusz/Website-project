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
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: reservation
        [Authorize(Roles = "Client,Worker,Manager")]
        public async Task<IActionResult> Index(string? TextoAPesquisar, DateTime? dataMin, DateTime? dataMax, string? estado, string? categoriaId)
        {
            var userId = _userManager.GetUserId(User);
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return NotFound();
            }

            var reservas = new List<Reservation>();

            // worker/manager
            if (await _userManager.IsInRoleAsync(user, "Worker") || await _userManager.IsInRoleAsync(user, "Manager"))
            {
                reservas = await _context.Reservation.Include(r => r.Client).Include(r => r.PickUpWorker).Include(r => r.PickUpWorker).Include(r => r.Vehicle)
                    .Where(r => r.Vehicle.CompanyId == user.CompanyID).ToListAsync();

                var ListaEstados = new List<SelectListItem> {
                    new SelectListItem { Text = "Todas", Value = "Todas", Selected = estado == "Todas" ? true : false },
                    new SelectListItem { Text = "Rejeitadas", Value = "Rejeitadas", Selected = estado == "Rejeitadas" ? true : false },
                    new SelectListItem { Text = "Pendentes", Value = "Pendentes", Selected = estado == "Pendentes" ? true : false },
                    new SelectListItem { Text = "Aceites", Value = "Aceites", Selected = estado == "Aceites" ? true : false },
                    new SelectListItem { Text = "Encerradas", Value = "Encerradas", Selected = estado == "Encerradas" ? true : false }
                };

                var ListaCategorias = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Todas", Value = "-1", Selected = categoriaId == "-1" ? true : false }
                };
                foreach (VehicleCategory cat in _context.VehicleCategory)
                {
                    ListaCategorias.Add(new SelectListItem { Text = cat.Name, Value = cat.Id.ToString(), Selected = categoriaId == cat.Id.ToString() ? true : false });
                }

                ViewData["ListaEstados"] = ListaEstados;
                ViewData["ListaCategorias"] = ListaCategorias;
                ViewData["TextoPesquisado"] = TextoAPesquisar;

                if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
                {
                    reservas = reservas.Where(r => r.Vehicle.Brand.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Vehicle.Model.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Client.FirstName.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Client.LastName.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Vehicle.Location.ToUpper().Contains(TextoAPesquisar.ToUpper())).ToList();
                }

				if (estado == "Rejeitadas")
					reservas = reservas.Where(r => r.ReservationState == -1).ToList();
                else if (estado == "Pendentes")
					reservas = reservas.Where(r => r.ReservationState == 0).ToList();
                else if (estado == "Aceites")
					reservas = reservas.Where(r => r.ReservationState == 1).ToList();
                else if (estado == "Encerradas")
                    reservas = reservas.Where(r => r.ReservationState == 2).ToList();

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
						reservas = reservas.Where(r => r.Vehicle.CategoryID == catId).ToList();
					}
				}

                if (dataMin != null)
                    reservas = reservas.Where(r => r.PickUpDate >= dataMin).ToList();
                if (dataMax != null)
                    reservas = reservas.Where(r => r.DeliveryDate <= dataMax).ToList();

                if(dataMin != null && dataMax != null)
                    ViewData["Resultado"] = "Lista de reservas desde " + dataMin + " até " + dataMax;
                else if(dataMin != null && dataMax == null)
                    ViewData["Resultado"] = "Lista de reservas desde " + dataMin;
                else if(dataMin == null && dataMax != null)
                    ViewData["Resultado"] = "Lista de reservas até " + dataMax;
            }
			// Client
			else
			{
                reservas = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker).Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Company)
                    .Where(r => r.ClientId == userId).ToListAsync();

                var ListaEstados = new List<SelectListItem> {
                    new SelectListItem { Text = "Todas", Value = "Todas", Selected = estado == "Todas" ? true : false },
                    new SelectListItem { Text = "Rejeitadas", Value = "Rejeitadas", Selected = estado == "Rejeitadas" ? true : false },
                    new SelectListItem { Text = "Pendentes", Value = "Pendentes", Selected = estado == "Pendentes" ? true : false },
                    new SelectListItem { Text = "Aceites", Value = "Aceites", Selected = estado == "Aceites" ? true : false },
                    new SelectListItem { Text = "Encerradas", Value = "Encerradas", Selected = estado == "Encerradas" ? true : false }
                };

                var ListaCategorias = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Todas", Value = "-1", Selected = categoriaId == "-1" ? true : false }
                };
                foreach (VehicleCategory cat in _context.VehicleCategory)
                {
                    ListaCategorias.Add(new SelectListItem { Text = cat.Name, Value = cat.Id.ToString(), Selected = categoriaId == cat.Id.ToString() ? true : false });
                }

                ViewData["ListaEstados"] = ListaEstados;
                ViewData["ListaCategorias"] = ListaCategorias;
                ViewData["TextoPesquisado"] = TextoAPesquisar;

                if (!string.IsNullOrWhiteSpace(TextoAPesquisar))
                {
                    reservas = reservas.Where(r => r.Vehicle.Brand.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Vehicle.Model.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Client.FirstName.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Client.LastName.ToUpper().Contains(TextoAPesquisar.ToUpper())
                                                || r.Vehicle.Location.ToUpper().Contains(TextoAPesquisar.ToUpper())).ToList();
                }

                if (estado == "Rejeitadas")
                    reservas = reservas.Where(r => r.ReservationState == -1).ToList();
                else if (estado == "Pendentes")
                    reservas = reservas.Where(r => r.ReservationState == 0).ToList();
                else if (estado == "Aceites")
                    reservas = reservas.Where(r => r.ReservationState == 1).ToList();
                else if (estado == "Encerradas")
                    reservas = reservas.Where(r => r.ReservationState == 2).ToList();

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
                        reservas = reservas.Where(r => r.Vehicle.CategoryID == catId).ToList();
                    }
                }

                if (dataMin != null)
                    reservas = reservas.Where(r => r.PickUpDate >= dataMin).ToList();
                if (dataMax != null)
                    reservas = reservas.Where(r => r.DeliveryDate <= dataMax).ToList();

                if (dataMin != null && dataMax != null)
                    ViewData["Resultado"] = "Lista de reservas desde " + dataMin + " até " + dataMax;
                else if (dataMin != null && dataMax == null)
                    ViewData["Resultado"] = "Lista de reservas desde " + dataMin;
                else if (dataMin == null && dataMax != null)
                    ViewData["Resultado"] = "Lista de reservas até " + dataMax;
            }

            return View(reservas);
        }

        // GET: reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Reservation == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservation
                .Include(r => r.Client)
                .Include(r => r.DeliveryWorker)
                .Include(r => r.PickUpWorker)
                .Include(r => r.Vehicle)
                .Include(r => r.Vehicle.Category)
                .Include(r => r.Vehicle.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reservation/Create/Id
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(int veiculoId)
        {
            ViewBag.Erro = "";

            var veiculo = await _context.Vehicle.Include(v => v.Category).Include(v => v.Company)
                                                .FirstOrDefaultAsync(v => v.Id == veiculoId);

            if (veiculo == null)
            {
                return NotFound();
            }

            var reserva = new Reservation();
            reserva.PickUpDate = DateTime.Now.AddDays(1);
            reserva.DeliveryDate = DateTime.Now.AddDays(4);
            reserva.Vehicle = veiculo;

            return View(reserva);
        }

        // POST: Reservation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int veiculoId, Reservation reserva)
        {
            var clienteId = _userManager.GetUserId(User);
            var cliente = _context.Users.Find(clienteId);
            var veiculo = await _context.Vehicle.Include(v => v.Category).Include(v => v.Company)
                                                .FirstOrDefaultAsync(v => v.Id == veiculoId);

            if (veiculo == null || cliente == null)
            {
                return NotFound();
            }

            reserva.ClientId = clienteId;
            reserva.Client = cliente;
            reserva.VehicleId = veiculoId;
            reserva.Vehicle = veiculo;

            if(reserva.PickUpDate < DateTime.Now.AddHours(1))
            {
                ViewBag.Erro = "A data de levantamento deve ser pelo menos 1 hora no futuro";
                return View(reserva);
            }
            else if (reserva.DeliveryDate < reserva.PickUpDate)
            {
                ViewBag.Erro = "A data de entrega não deve ser inferior à data de levantamento";
                return View(reserva);
            }
            else if((reserva.DeliveryDate - reserva.PickUpDate).TotalHours < 1)
            {
                ViewBag.Erro = "O tempo total de reserva deve ser superior a 1 hora";
                return View(reserva);
            }

            //verificar se veiculo já está reservado no intervalo de tempo fornecido
            var veiculoReservado1 = _context.Reservation.Where(r => r.VehicleId == veiculoId).Where(r =>
                r.ReservationState == 0 || r.ReservationState == 1).Where(r =>
                (reserva.PickUpDate >= r.PickUpDate && reserva.PickUpDate <= r.DeliveryDate ||
                reserva.DeliveryDate >= r.PickUpDate && reserva.DeliveryDate <= r.DeliveryDate)).Count();

            var veiculoReservado2 = _context.Reservation.Where(r => r.VehicleId == veiculoId).Where(r =>
                r.ReservationState == 0 || r.ReservationState == 1).Where(r =>
                (r.PickUpDate >= reserva.PickUpDate && r.PickUpDate <= reserva.DeliveryDate ||
                r.DeliveryDate >= reserva.PickUpDate && r.DeliveryDate <= reserva.DeliveryDate)).Count();

            if (veiculoReservado1+veiculoReservado2 > 0)
            {
                ViewBag.Erro = "O veículo já se encontra reservado no intervalo de tempo selecionado";
                return View(reserva);
            }

            int totalDias = (int)(reserva.DeliveryDate - reserva.PickUpDate).TotalDays;
            if (totalDias >= 1)
                reserva.Price = (totalDias+1) * reserva.Vehicle.Price;
            else
                reserva.Price = reserva.Vehicle.Price;

            ModelState.Remove(nameof(reserva.DeliveryWorker));
            ModelState.Remove(nameof(reserva.PickUpWorker));

            _context.Add(reserva);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Details", new { id = reserva.Id });
        }

        // GET: Reservation/Accept/5
        [Authorize(Roles = "Worker,Manager")]
        public async Task<IActionResult> Aceitar(int? id)
        {
            if (id == null || _context.Reservation == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker)
                .Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Category)
                .Where(r => r.Id == id).FirstAsync();
            if (reserva == null)
            {
                return NotFound();
            }
           
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Worker,Manager")]
        public async Task<IActionResult> Aceitar(int id, [Bind("Id,PickUpDate,DeliveryDate,Price,Kms,DemageInPickup,DemageInDelivery,Observations,Clasyfication,ReservationState,ClientId,DeliveryWorkerId,PickUpWorkerID,VehicleId")] Reservation reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }

            if (reserva.Observations != null)
                reserva.Observations = "Observações iniciais: " + reserva.Observations;
            reserva.DeliveryWorkerId = _userManager.GetUserId(User);
            reserva.ReservationState = 1; //accepted

            ModelState.Remove(nameof(reserva.DeliveryWorker));
            ModelState.Remove(nameof(reserva.PickUpWorker));
            ModelState.Remove(nameof(reserva.Vehicle));
            ModelState.Remove(nameof(reserva.Client));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.Id))
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

            reserva = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker)
                .Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Category)
                .Where(r => r.Id == id).FirstAsync();

            return View(reserva);
        }

        // GET: reservation/Accepted/5
        [Authorize(Roles = "Worker,Manager")]
        public async Task<IActionResult> Encerrar(int? id)
        {
            if (id == null || _context.Reservation == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker)
                .Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Category)
                .Where(r => r.Id == id).FirstAsync();
            if (reserva == null)
            {
                return NotFound();
            }

            reserva.DemageInDelivery = reserva.DemageInPickup;

            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Worker,Manager")]
        public async Task<IActionResult> Encerrar(int id, string? observacoes, int? classificacao, decimal? kms, [Bind("Id,PickUpDate,DeliveryDate,Price,Kms,DemageInPickup,DemageInDelivery,Observations,Clasyfication,ReservationState,ClientId,DeliveryWorkerId,PickUpWorkerID,VehicleId")] Reservation reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }

            var veiculo = _context.Vehicle.Find(reserva.VehicleId);
            if(veiculo == null)
            {
                return NotFound();
            }

            var empresa = _context.Company.Find(veiculo.CompanyId);
            if (empresa == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(observacoes))
            {
                Console.WriteLine($"\n\nobservacoes: {observacoes}\n\n");
                if (string.IsNullOrWhiteSpace(reserva.Observations))
                    reserva.Observations = "Observações finais: " + observacoes;
                else
                    reserva.Observations = reserva.Observations + "\n Observações finais: " + observacoes;
            }

            if (classificacao == null || kms == null || kms <= veiculo.Kms)
            {
                reserva = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker)
                    .Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Category)
                    .Where(r => r.Id == id).FirstAsync();

                if (classificacao == null)
                {
                    ViewBag.ErroClassificacao = "Deve indicar a classificação";
                    return View(reserva);
                }

                if (kms == null)
                {
                    ViewBag.ErroKms = "Deve indicar o nº de quilómetros do veiculo";
                    return View(reserva);
                }

                if (kms <= veiculo.Kms)
                {
                    ViewBag.ErroKms = "O nº de quilómetros do veículo deve ser superior ao valor anterior da reserva";
                    return View(reserva);
                }
            }
            

            reserva.PickUpWorkerID = _userManager.GetUserId(User);

            reserva.Clasyfication = classificacao;

            if (empresa.rating == null)

                empresa.rating = reserva.Clasyfication;
            else
                empresa.rating = ((empresa.rating * empresa.NumberOfRatings) + classificacao) / (empresa.NumberOfRatings + 1);

            empresa.NumberOfRatings += 1;

            reserva.Kms = kms - veiculo.Kms;
            veiculo.Kms = (decimal)kms;

            reserva.ReservationState = 2; //closed

            ModelState.Remove(nameof(reserva.DeliveryWorker));
            ModelState.Remove(nameof(reserva.PickUpWorker));
            ModelState.Remove(nameof(reserva.Vehicle));
            ModelState.Remove(nameof(reserva.Client));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(veiculo);
                    await _context.SaveChangesAsync();

                    _context.Update(empresa);
                    await _context.SaveChangesAsync();

                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.Id))
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

            reserva = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker)
                .Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Category)
                .Where(r => r.Id == id).FirstAsync();

            return View(reserva);
        }

        public async Task<IActionResult> Rejeitar(int id)
        {
            Reservation reserva = await _context.Reservation.Include(r => r.Client).Include(r => r.DeliveryWorker)
                .Include(r => r.PickUpWorker).Include(r => r.Vehicle).Include(r => r.Vehicle.Category)
                .Where(r => r.Id == id).FirstAsync();

            if (reserva == null)
            {
                return View();
            }

            reserva.ReservationState = -1;

            _context.Update(reserva);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
          return _context.Reservation.Any(e => e.Id == id);
        }
    }
}
