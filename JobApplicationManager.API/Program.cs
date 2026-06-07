using IdentityService.API.JWT;
using JobApplicationManager.API.Data.Context;
using JobApplicationManager.API.Features.ApplicationEmails.Interfaces;
using JobApplicationManager.API.Features.ApplicationEmails.Services;
using JobApplicationManager.API.Features.Applications.Interfaces;
using JobApplicationManager.API.Features.Applications.Services;
using JobApplicationManager.API.Features.Calendar.Interfaces;
using JobApplicationManager.API.Features.Calendar.Services;
using JobApplicationManager.API.Features.CvDocuments.Interfaces;
using JobApplicationManager.API.Features.CvDocuments.Services;
using JobApplicationManager.API.Features.Notifications.Interfaces;
using JobApplicationManager.API.Features.Notifications.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IApplicationEmailService, ApplicationEmailService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<ICvDocumentService, CvDocumentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token in format: Bearer {token}"
    });

});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
             "https://jobmanagerpro.netlify.app"
             )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
