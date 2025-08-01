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
    public class AdmOrderDetailsController : Controller
    {
        private readonly Customerwebsite1Context _context;

        public AdmOrderDetailsController(Customerwebsite1Context context)
        {
            _context = context;
        }

        // GET: AdmOrderDetails
        public async Task<IActionResult> Index()
        {
            var customerwebsite1Context = _context.OrderDetails.Include(o => o.OrderF).Include(o => o.ProductF);
            return View(await customerwebsite1Context.ToListAsync());
        }

        // GET: AdmOrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.OrderF)
                .Include(o => o.ProductF)
                .FirstOrDefaultAsync(m => m.OrderDetailId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // GET: AdmOrderDetails/Create
        public IActionResult Create()
        {
            ViewData["OrderFid"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            ViewData["ProductFid"] = new SelectList(_context.Products, "ProductId", "ProductId");
            return View();
        }

        // POST: AdmOrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderDetailId,OrderFid,ProductFid,Quantity,UnitPrice")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderFid"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderFid);
            ViewData["ProductFid"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetail.ProductFid);
            return View(orderDetail);
        }

        // GET: AdmOrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            ViewData["OrderFid"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderFid);
            ViewData["ProductFid"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetail.ProductFid);
            return View(orderDetail);
        }

        // POST: AdmOrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailId,OrderFid,ProductFid,Quantity,UnitPrice")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.OrderDetailId))
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
            ViewData["OrderFid"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderFid);
            ViewData["ProductFid"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetail.ProductFid);
            return View(orderDetail);
        }

        // GET: AdmOrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.OrderF)
                .Include(o => o.ProductF)
                .FirstOrDefaultAsync(m => m.OrderDetailId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: AdmOrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailId == id);
        }
    }
}
