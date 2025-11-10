using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Business.Services;
using ITOVotingApplication.Business.Services.Interfaces;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Data.Context;
using ITOVotingApplication.Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.StaticFiles;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
.AddNewtonsoftJson(options =>
{
	options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Configure API behavior options for better model validation handling
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	// Keep automatic model validation but allow custom handling
	options.SuppressModelStateInvalidFilter = false;
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
builder.Services.AddScoped<ICommitteeService, CommitteeService>();
builder.Services.AddScoped<IUserInvitationService, UserInvitationService>();
builder.Services.AddScoped<ICompanyDocumentTransactionService, CompanyDocumentTransactionService>();

// Communication Services
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITuratelSmsService, TuratelSmsService>();

// HTTP Client for external API calls
builder.Services.AddHttpClient();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "ThisIsMyVerySecureSecretKeyForJWT2024!@#$%";

// Authentication - Dual Support (Cookie + JWT)
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/Auth/Login";
	options.LogoutPath = "/Auth/Logout";
	options.AccessDeniedPath = "/Auth/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromHours(24);
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
		ValidIssuer = jwtSettings["Issuer"] ?? "ITOVotingApp",
		ValidAudience = jwtSettings["Audience"] ?? "ITOVotingAppUsers",
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
		ClockSkew = TimeSpan.Zero
	};

	// JWT Bearer events for API authentication
	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			// Token'� Authorization header'dan al
			var token = context.Request.Headers["Authorization"]
				.FirstOrDefault()?.Split(" ").Last();

			if (!string.IsNullOrEmpty(token))
			{
				context.Token = token;
			}

			return Task.CompletedTask;
		},
		OnAuthenticationFailed = context =>
		{
			if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
			{
				context.Response.Headers.Add("Token-Expired", "true");
			}
			return Task.CompletedTask;
		}
	};
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("UserOnly", policy => policy.RequireRole("User", "Admin"));
	options.AddPolicy("ApiUser", policy =>
	policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
		  .RequireAuthenticatedUser());
});

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
	options.Cookie.Name = ".ITOVoting.Session";
});

// Add HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

// Default static files (wwwroot)
app.UseStaticFiles();

// Additional static files for temp directory with proper configuration
var tempPath = Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "wwwroot", "temp");
if (!Directory.Exists(tempPath))
{
    Directory.CreateDirectory(tempPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(tempPath),
    RequestPath = "/temp",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    {
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" }
    })
});

// Additional static files for Documents directory
var documentsPath = Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "Documents");
if (!Directory.Exists(documentsPath))
{
    Directory.CreateDirectory(documentsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(documentsPath),
    RequestPath = "/Documents",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Giri� yapmam�� kullan�c�lar� Login sayfas�na y�nlendir
app.Use(async (context, next) =>
{
	// E�er kullan�c� authenticate de�ilse ve Login/API endpoint'i de�ilse
	if (!context.User.Identity.IsAuthenticated
		&& !context.Request.Path.StartsWithSegments("/Auth/Login")
		&& !context.Request.Path.StartsWithSegments("/api/auth")
		&& !context.Request.Path.StartsWithSegments("/css")
		&& !context.Request.Path.StartsWithSegments("/js")
		&& !context.Request.Path.StartsWithSegments("/lib")
		&& !context.Request.Path.StartsWithSegments("/images"))
	{
		// Root veya Admin sayfalar�na eri�im deneniyorsa Login'e y�nlendir
		if (context.Request.Path == "/"
			|| context.Request.Path.StartsWithSegments("/Admin")
			|| context.Request.Path.StartsWithSegments("/Dashboard")
			|| context.Request.Path.StartsWithSegments("/Vote"))
		{
			context.Response.Redirect("/Auth/Login");
			return;
		}
	}

	await next();
});

app.MapControllers();

app.MapControllerRoute(
	name: "auth",
	pattern: "Auth/{action=Login}/{id?}",
	defaults: new { controller = "Auth" });

// Admin area routes
app.MapControllerRoute(
	name: "admin",
	pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// Dashboard route
app.MapControllerRoute(
	name: "dashboard",
	pattern: "Dashboard/{action=Index}/{id?}",
	defaults: new { controller = "Dashboard" });

// Vote routes
app.MapControllerRoute(
	name: "vote",
	pattern: "Vote/{action=Index}/{id?}",
	defaults: new { controller = "Vote" });

// Default route - Ana sayfa olarak Auth/Login'e y�nlendir
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapGet("/", context =>
{
	context.Response.Redirect("/Auth/Login");
	return Task.CompletedTask;
});

app.Run();