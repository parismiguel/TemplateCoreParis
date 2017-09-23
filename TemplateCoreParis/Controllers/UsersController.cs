using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using TemplateCoreParis.Models;
using TemplateCoreParis.Data;
using Microsoft.AspNetCore.Authorization;
using TemplateCoreParis.WebChat;

namespace TemplateCoreParis.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;


        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;

        }

        // GET: Users
        public async Task<IActionResult> Index()
        {

            List<ApplicationUser> model = new List<ApplicationUser>();

            try
            {
                model = await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(model);
            }

            return View(model);
        }
     


        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                applicationUser.UserName = applicationUser.Email;

                var result = await _userManager.CreateAsync(applicationUser);

                return RedirectToAction("Index");
            }

            return View(applicationUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _userManager.FindByIdAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            var _decrypt = Helpers.Helpers.DecryptString(applicationUser.SecretResponse, ChatBotController._keyEncode);

            applicationUser.SecretResponse = _decrypt;

            return View(applicationUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(applicationUser.Id);
                    var _encrypt = Helpers.Helpers.EncryptString(applicationUser.SecretResponse, ChatBotController._keyEncode);

                    user.FirstName = applicationUser.FirstName;
                    user.LastName = applicationUser.LastName;
                    user.Birthday = applicationUser.Birthday;
                    user.DocIdentity = applicationUser.DocIdentity;
                    user.Title = applicationUser.Title;
                    user.EmailConfirmed = applicationUser.EmailConfirmed;
                    user.PhoneNumber = applicationUser.PhoneNumber;
                    user.SecretQuestion = applicationUser.SecretQuestion;
                    user.SecretResponse = _encrypt;

                    //add user to the datacontext (database) and save changes
                    _context.Update(user);

                    await _context.SaveChangesAsync();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return RedirectToAction("Index");
            }

            return View(applicationUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);

            _context.Users.Remove(applicationUser);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private bool ApplicationUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
