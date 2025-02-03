using Microsoft.AspNetCore.Mvc;
using SieveApp.Services;
using SieveApp.Models;

namespace SieveApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SieveController : ControllerBase
    {
        private readonly SieveService _sieveService;

        public SieveController(SieveService sieveService)
        {
            _sieveService = sieveService;
        }

        [HttpPost("get-primes-up-to-n")]
        public IActionResult GetPrimesUpToN([FromBody] PrimeRequest request)
        {
            if (request.Limit <= 0)
            {
                Console.WriteLine("Ошибка: Число должно быть больше 0.");
                return BadRequest("Limit must be greater than 0.");
            }

            var primes = _sieveService.SieveOfAtkin(request.Limit);

            // Вывод простых чисел в терминал
            Console.WriteLine($"Простые числа до {request.Limit}:");
            foreach (var prime in primes)
            {
                Console.WriteLine(prime);
            }

            return Ok(primes);
        }
    }
}
