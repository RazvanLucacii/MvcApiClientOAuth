using Microsoft.AspNetCore.Mvc;
using MvcApiClientOAuth.Filters;
using MvcApiClientOAuth.Models;
using MvcApiClientOAuth.Services;

namespace MvcApiClientOAuth.Controllers
{
    public class EmpleadosController : Controller
    {
        private ServiceApiEmpleados service;
        
        public EmpleadosController(ServiceApiEmpleados service)
        {
            this.service = service;
        }

        [AuthorizeEmpleados]
        public async Task<IActionResult> Index()
        {
            List<Empleado> empleados = await this.service.GetEmpleadosAsync();
            return View(empleados);
        }

        [AuthorizeEmpleados]
        public async Task<IActionResult> Details(int id)
        {
            string token = HttpContext.Session.GetString("TOKEN");
            if(token == null)
            {
                ViewData["MENSAJE"] = "Debe validarse en Login";
                return View();
            }
            Empleado empleado = await this.service.GetEmpleadoAsync(id, token);
            return View(empleado);
        }
    }
}
