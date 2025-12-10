using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace BookStoreAPI.Utilities.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DBInitializer(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager , ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public void Initialize()
        {
            // update-database Automaically
            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }

            if (_roleManager.Roles.IsNullOrEmpty())
            {
                                                                // wait to take the result
                _roleManager.CreateAsync(new(SD.Super_Admin_Role)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new(SD.Admin_Role) ).GetAwaiter().GetResult();
               _roleManager.CreateAsync(new(SD.Employee_Role)).GetAwaiter().GetResult();
               _roleManager.CreateAsync(new(SD.Customer_Role)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new()
                {
                    Name = "SuperAdmin",
                    Email= "SuperAdmin@BookStoreAPI.com",
                    UserName= "SuperAdmin",
                    EmailConfirmed= true,


                },"Admin123$").GetAwaiter().GetResult();
              var user=  _userManager.FindByNameAsync("SuperAdmin").GetAwaiter().GetResult();
                //take the user an add it to this Role
                _userManager.AddToRoleAsync(user!, SD.Super_Admin_Role).GetAwaiter().GetResult();
            }
        }
    }
}
