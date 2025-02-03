using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SieveApp.Data;
using SieveApp.Models;
using SieveApp.Services;
using SieveApp;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));
builder.Services.AddScoped<SieveService>();

// Регистрация JwtService
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("Jwt:Key не найден в appsettings.json. Убедитесь, что ключ добавлен.");
}

builder.Services.AddScoped<JwtService>(provider => 
    new JwtService(jwtKey));

// Включение CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Применение миграций и создание базы данных
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated(); // Создаем базу данных, если она не существует
}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("=== User Registration ===");
    Console.Write("Enter username: ");
    var username = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(username))
    {
        Console.WriteLine("Ошибка: Логин не может быть пустым.");
        return;
    }

    if (username.Length < 3)
    {
        Console.WriteLine("Ошибка: Логин должен содержать минимум 3 символа.");
        return;
    }
    if (dbContext.Users.Any(u => u.Username == username))
    {
        Console.WriteLine("Ошибка: Пользователь с таким логином уже существует.");
        return;
    }

    Console.Write("Enter password: ");
    var password = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("Ошибка: Пароль не может быть пустым.");
        return;
    }

    if (password.Length < 6)
    {
        Console.WriteLine("Ошибка: Пароль должен содержать минимум 6 символов.");
        return;
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

    var newUser = new User
    {
        Username = username,
        PasswordHash = passwordHash,
        Token = null // Явно указываем, что Token не задан
    };

    dbContext.Users.Add(newUser);
    dbContext.SaveChanges();

    Console.WriteLine("Пользователь успешно зарегистрирован.");
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("\n=== User Login ===");
    Console.Write("Enter username: ");
    var loginUsername = Console.ReadLine();
    Console.Write("Enter password: ");
    var loginPassword = Console.ReadLine();

    var user = dbContext.Users.SingleOrDefault(u => u.Username == loginUsername);
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginPassword ?? string.Empty, user.PasswordHash))
    {
        Console.WriteLine("Ошибка: Неверный логин или пароль.");
        return;
    }

    // Генерация токена
    var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();
    var token = jwtService.GenerateToken(user.Id);

    // Обновление токена в базе данных (опционально)
    user.Token = token;
    dbContext.SaveChanges();

    Console.WriteLine("Пользователь успешно авторизован.");
    Console.WriteLine($"Токен: {token}");
}

using (var scope = app.Services.CreateScope())
{
    var sieveService = scope.ServiceProvider.GetRequiredService<SieveService>();

    while (true)
    {
        Console.Write("Введите натуральное число N (от 1 до 50): ");
        var input = Console.ReadLine();
        if (int.TryParse(input, out int limit) && limit >= 1 && limit <= 50)
        {
            LimitStorage.CurrentLimit = limit; // Установка предельного числа

            // Получаем простые числа и выводим их в терминал
            var primes = sieveService.SieveOfAtkin(limit);

            Console.WriteLine($"Простые числа до {limit}:");
            foreach (var prime in primes)
            {
                Console.WriteLine(prime);
            }

            break;
        }
        else
        {
            Console.WriteLine("Ошибка: введите корректное число от 1 до 50.");
        }
    }
}

app.UseCors("AllowAll");

app.MapControllers(); // Регистрация маршрутов контроллеров

app.MapGet("/", context =>
{
    context.Response.Redirect($"/api/sieve/prime-numbers-html", permanent: false);
    return Task.CompletedTask;
});

app.Run();

