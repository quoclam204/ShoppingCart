using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Areas.Admin.Repository;
using ShoppingCart.Models;
using ShoppingCart.Repository;
using System.Runtime.InteropServices;

namespace ShoppingCart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Connection db
            //builder.Services.AddDbContext<DataContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
            //});
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb")));

            // Add Email Sender
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(optiosns =>
            {
                optiosns.IdleTimeout = TimeSpan.FromMinutes(30);
                optiosns.Cookie.HttpOnly = true;
                optiosns.Cookie.IsEssential = true;
            });

            #region Identity
            // Khai báo Identity
            builder.Services.AddIdentity<AppUserModel, IdentityRole>()
                .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
            builder.Services.AddRazorPages();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings. -> khóa tài khoản
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                //options.Lockout.MaxFailedAccessAttempts = 5;
                //options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Mỗi email chỉ được dùng cho 1 tài khoản duy nhất trong hệ thống.
                options.User.RequireUniqueEmail = true;
            });
            #endregion

            var app = builder.Build();

            // Khi xảy ra lỗi hệ thống tự chuyển hướng đến trang 404
            app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            // bộ nhớ tạm để lưu dữ liệu người dùng như: giỏ hàng, trạng thái đăng nhập
            app.UseSession();

            // xử lý đăng nhập (xác thực) và phân quyền.
            app.UseAuthentication(); // Đăng nhập trước 
            app.UseAuthorization(); // Sau đó kiểm tra quyền

            app.MapStaticAssets();

            //Route càng cụ thể → đặt lên trên
            //Route càng chung → đặt xuống dưới
            app.MapControllerRoute(
                name: "Areas",
                pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Custom route category
            app.MapControllerRoute(
                name: "category",
                pattern: "/category/{slug?}",
                defaults: new { controller = "Category", action = "Index" }) // mặc đinh mới
                .WithStaticAssets();

            // Custom route brand
            app.MapControllerRoute(
                name: "brand",
                pattern: "/brand/{slug?}",
                defaults: new { controller = "Brand", action = "Index" })
                .WithStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // seeding data
            var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
            SeedData.SeedingData(context);

            app.Run();
        }
    }
}
