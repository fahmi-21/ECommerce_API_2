using ECommerce_API_2.DataAccess;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce_API_2.Utilities.DBInitilization
{
    public class DBInitilizer : IDBInitilizer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public DBInitilizer(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        public async Task Initialize()
        {
            //if (_context.Database.GetPendingMigrations().Any())
            //{
            //    _context.Database.Migrate();
            //}

            if (!await _roleManager.RoleExistsAsync(SD.SUPER_ADMIN_ROLE))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.SUPER_ADMIN_ROLE));
                await _roleManager.CreateAsync(new IdentityRole(SD.ADMIN_ROLE));
                await _roleManager.CreateAsync(new IdentityRole(SD.EMPLOYEE_ROLE));
                await _roleManager.CreateAsync(new IdentityRole(SD.CUSTOMER_ROLE));

                await _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        FName = "Super",
                        LName = "Admin",
                        Email = "superadmin@project.com",
                        EmailConfirmed = true,
                        UserName = "SuperAdmin"
                    }, password: "SuperAdmin1234$"
                );
                await _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        FName = "Admin",
                        LName = "",
                        Email = "admin@project.com",
                        EmailConfirmed = true,
                        UserName = "Admin"
                    }, password: "Admin1234$"
                );
                await _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        FName = "Employee",
                        LName = "1",
                        Email = "employee1@project.com",
                        EmailConfirmed = true,
                        UserName = "Employee1"
                    }, password: "Employee1234$"
                );
                await _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        FName = "Employee",
                        LName = "2",
                        Email = "employee2@project.com",
                        EmailConfirmed = true,
                        UserName = "Employee2"
                    }, password: "Employee1234$"
                );
                var user1 = await _userManager.FindByNameAsync("SuperAdmin");
                var user2 = await _userManager.FindByNameAsync("Admin");
                var user3 = await _userManager.FindByNameAsync("Employee1");
                var user4 = await _userManager.FindByNameAsync("Employee2");

                if (user1 is not null && user2 is not null && user3 is not null && user4 is not null)
                {
                    await _userManager.AddToRoleAsync(user1, SD.SUPER_ADMIN_ROLE);
                    await _userManager.AddToRoleAsync(user2, SD.ADMIN_ROLE);
                    await _userManager.AddToRoleAsync(user3, SD.EMPLOYEE_ROLE);
                    await _userManager.AddToRoleAsync(user4, SD.EMPLOYEE_ROLE);
                }
            }
        }
    }
}
