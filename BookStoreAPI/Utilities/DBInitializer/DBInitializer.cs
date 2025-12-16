using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookStoreAPI.Utilities.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DBInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task InitializeAsync()
        {
            // Apply pending migrations
            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }

            // Seed roles and super admin
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Super_Admin_Role));
                await _roleManager.CreateAsync(new IdentityRole(SD.Admin_Role));
                await _roleManager.CreateAsync(new IdentityRole(SD.Employee_Role));
                await _roleManager.CreateAsync(new IdentityRole(SD.Customer_Role));

                var superAdmin = new ApplicationUser
                {
                    Name = "SuperAdmin",
                    Email = "SuperAdmin@BookStoreAPI.com",
                    UserName = "SuperAdmin",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(superAdmin, "Admin123$");
                await _userManager.AddToRoleAsync(superAdmin, SD.Super_Admin_Role);
            }
        }
    }
}
