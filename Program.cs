
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Middlewares;
using UserManagementApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("UsersDb"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// ================= CRUD ENDPOINTS =================

// GET all users
app.MapGet("/users", async (AppDbContext db) =>
    await db.Users.ToListAsync());

// GET user by id
app.MapGet("/users/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(user);
});

// POST create user (with validation)
app.MapPost("/users", async (User user, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(user.Name))
        return Results.BadRequest("Name is required");

    if (string.IsNullOrWhiteSpace(user.Email))
        return Results.BadRequest("Email is required");

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", user);
});

// PUT update user
app.MapPut("/users/{id}", async (int id, User input, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Name = input.Name;
    user.Email = input.Email;

    await db.SaveChangesAsync();
    return Results.Ok(user);
});

// DELETE user
app.MapDelete("/users/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
