using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using e_shop.Models;

namespace e_shop.Controllers
{
    public class AdmOrdersController : Controller
    {
        private readonly Customerwebsite1Context _context;

        public AdmOrdersController(Customerwebsite1Context context)
        {
            _context = context;
        }

        // GET: AdmOrders
        public async Task<IActionResult> Index()
        {
            var customerwebsite1Context = _context.Orders.Include(o => o.CustomerF);
            return View(await customerwebsite1Context.ToListAsync());
        }

        // GET: AdmOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.CustomerF)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: AdmOrders/Create
        public IActionResult Create()
        {
            ViewData["CustomerFid"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            return View();
        }

        // POST: AdmOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerFid,OrderDate,Time,TotalAmount,Details,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerFid"] = new SelectList(_context.Customers, "CustomerId", "FullName", order.CustomerFid);
            return View(order);
        }

        // GET: AdmOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerFid"] = new SelectList(_context.Customers, "CustomerId", "FullName", order.CustomerFid);
            return View(order);
        }

        // POST: AdmOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerFid,OrderDate,Time,TotalAmount,Details,Status")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerFid"] = new SelectList(_context.Customers, "CustomerId", "FullName", order.CustomerFid);
            return View(order);
        }

        // GET: AdmOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.CustomerF)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: AdmOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Load the order including its details
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                // First delete related OrderDetails
                _context.OrderDetails.RemoveRange(order.OrderDetails);

                // Then delete the order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
