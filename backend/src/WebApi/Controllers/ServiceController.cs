using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "Services_All";

        public ServiceController(IAppointmentService appointmentService, IMemoryCache cache)
        {
            _appointmentService = appointmentService;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<ServiceDto>? services))
            {
                services = await _appointmentService.GetServicesAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(CacheKey, services, cacheEntryOptions);
            }

            return Ok(services);
        }
    }
}
