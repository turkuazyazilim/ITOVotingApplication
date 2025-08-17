using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Business.Services;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Data.Context;
using ITOVotingApplication.Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Kestrel yapýlandýrmasý - IP üzerinden eriþim için
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//	serverOptions.Listen(System.Net.IPAddress.Any, 5001); // HTTP
//	serverOptions.Listen(System.Net.IPAddress.Any, 7001, listenOptions =>
//	{
//		// HTTPS için sertifika gerekli - development için self-signed sertifika kullanabilirsiniz
//		// listenOptions.UseHttps();
//	});
//});

// Add services to the container.
builder.Services.AddControllersWithViews()
	.AddNewtonsoftJson(options =>
	{
		options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	});

// Database Context
builder.Services.AddDbContext<VotingDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVoteService, VoteService>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Authentication - Dual Support (Cookie + JWT)
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/Account/Login";  // Auth/Login deðil, Account/Login olmalý
	options.LogoutPath = "/Account/Logout";
	options.AccessDeniedPath = "/Account/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
	options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
		ValidAudience = builder.Configuration["JwtSettings:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
	};
});

// Authorization
builder.Services.AddAuthorization();

// CORS Policy for API access
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin()
			   .AllowAnyMethod()
			   .AllowAnyHeader();
	});
});

// Session support
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// Swagger KALDIRILDI
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	// Swagger KALDIRILDI
	// app.UseSwagger();
	// app.UseSwaggerUI();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

// HTTPS redirection'ý kaldýrdýk çünkü hem HTTP hem HTTPS destekliyoruz
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");


app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
	name: "dashboard",
	pattern: "Dashboard/{action=Index}/{id?}",
	defaults: new { controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
	name: "dashboard",
	pattern: "{controller=Account}/{action=Logout}");

app.MapControllers(); // API endpoints için

// Seed Database (Optional)
using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<VotingDbContext>();
	context.Database.EnsureCreated();

	// Seed default admin user if not exists
	//DbInitializer.Initialize(context);
}

app.Run();