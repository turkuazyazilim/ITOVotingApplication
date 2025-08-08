// VotingApplication.Web/Program.cs - MVC Configuration
using ITOVotingApplication.Business.Interfaces;
using ITOVotingApplication.Business.Mappings;
using ITOVotingApplication.Business.Services;
using ITOVotingApplication.Core.Interfaces;
using ITOVotingApplication.Data.Context;
using ITOVotingApplication.Data.Repositories;
using ITOVotingApplication.Core.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
	.AddNewtonsoftJson(options =>
	{
		options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	});

// Add Razor Pages for Identity if needed
builder.Services.AddRazorPages();

// Add Session
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// Cookie Authentication for MVC
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.LogoutPath = "/Account/Logout";
		options.AccessDeniedPath = "/Account/AccessDenied";
		options.ExpireTimeSpan = TimeSpan.FromHours(1);
		options.SlidingExpiration = true;
	})
	.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
	{
		var jwtSettings = builder.Configuration.GetSection("JwtSettings");
		var secretKey = jwtSettings["SecretKey"];

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings["Issuer"],
			ValidAudience = jwtSettings["Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
			ClockSkew = TimeSpan.Zero
		};
	});

// Authorization
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("BallotOfficer", policy =>
		policy.RequireRole("Admin", "SandikGorevlisi", "BallotOfficer"));
});

// Database Configuration
builder.Services.AddDbContext<VotingDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
		b => b.MigrationsAssembly("VotingApplication.Data")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Repository and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business Services
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Swagger Configuration (for API endpoints)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Voting Application API",
		Version = "v1",
		Description = "Seçim Uygulamasý API Dokümantasyonu"
	});

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme.",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

// CORS Policy
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		policy =>
		{
			policy.AllowAnyOrigin()
				  .AllowAnyMethod()
				  .AllowAnyHeader();
		});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Voting Application API V1");
		c.RoutePrefix = "swagger";
	});
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapRazorPages();
app.MapControllers(); // For API controllers

// Seed initial data (optional)
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<VotingDbContext>();

	// Ensure database is created
	dbContext.Database.EnsureCreated();

	// Seed roles if not exists
	if (!dbContext.Roles.Any())
	{
		dbContext.Roles.AddRange(
			new Role { RoleDescription = "Admin", IsActive = true },
			new Role { RoleDescription = "SandikGorevlisi", IsActive = true },
			new Role { RoleDescription = "BallotOfficer", IsActive = true },
			new Role { RoleDescription = "User", IsActive = true }
		);
		dbContext.SaveChanges();
	}

	// Seed ballot boxes if not exists
	if (!dbContext.BallotBoxes.Any())
	{
		dbContext.BallotBoxes.AddRange(
			new BallotBox { BallotBoxDescription = "Sandýk 1 - Merkez" },
			new BallotBox { BallotBoxDescription = "Sandýk 2 - Þube" }
		);
		dbContext.SaveChanges();
	}

	// Seed company types if not exists
	if (!dbContext.CompanyTypes.Any())
	{
		dbContext.CompanyTypes.AddRange(
			new ITOVotingApplication.Core.Entities.CompanyType { CompanyTypeCode = "AS", CompanyTypeDescription = "Anonim Þirket" },
			new CompanyType { CompanyTypeCode = "LTD", CompanyTypeDescription = "Limited Þirket" },
			new CompanyType { CompanyTypeCode = "SAHIS", CompanyTypeDescription = "Þahýs Þirketi" },
			new CompanyType { CompanyTypeCode = "KOOP", CompanyTypeDescription = "Kooperatif" }
		);
		dbContext.SaveChanges();
	}

	// Seed committees if not exists
	if (!dbContext.Committees.Any())
	{
		dbContext.Committees.AddRange(
			new Committee { CommitteeDescription = "Yönetim Kurulu" },
			new Committee { CommitteeDescription = "Denetim Kurulu" },
			new Committee { CommitteeDescription = "Disiplin Kurulu" }
		);
		dbContext.SaveChanges();
	}
}

app.Run();