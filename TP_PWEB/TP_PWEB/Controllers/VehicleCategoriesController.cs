using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class VehicleCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VehicleCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.VehicleCategory.ToListAsync();

            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategoria(string categoryName)
        {
            if (categoryName != null)
            {
                VehicleCategory categoria = new VehicleCategory();
                categoria.Name = categoryName;

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Index");
        }

        // GET: VehicleCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.VehicleCategory == null)
            {
                return NotFound();
            }

            var categoriaVeiculo = await _context.VehicleCategory.FindAsync(id);
            if (categoriaVeiculo == null)
            {
                return NotFound();
            }

            ViewBag.NomeCategoria = categoriaVeiculo.Name;

            return View(categoriaVeiculo);
        }

        // POST: VehicleCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] VehicleCategory categoriaVeiculo)
        {
            if (id != categoriaVeiculo.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(categoriaVeiculo.Vehicles));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoriaVeiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaVeiculoExists(categoriaVeiculo.Id))
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
            return View(categoriaVeiculo);
        }

        // GET: VehicleDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.VehicleCategory == null)
            {
                return NotFound();
            }

            var categoriaVeiculo = await _context.VehicleCategory
				.FirstOrDefaultAsync(m => m.Id == id);
            if (categoriaVeiculo == null)
            {
                return NotFound();
            }

            return View(categoriaVeiculo);
        }

        // POST: VehicleDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.VehicleCategory == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CategoriaVeiculo'  is null.");
            }
            var categoriaVeiculo = await _context.VehicleCategory.FindAsync(id);
            if (categoriaVeiculo != null)
            {
                _context.VehicleCategory.Remove(categoriaVeiculo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaVeiculoExists(int id)
        {
          return _context.VehicleCategory.Any(e => e.Id == id);
        }
    }
}
