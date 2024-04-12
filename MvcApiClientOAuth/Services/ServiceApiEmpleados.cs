using MvcApiClientOAuth.Filters;
using MvcApiClientOAuth.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MvcApiClientOAuth.Services
{
    public class ServiceApiEmpleados
    {
        private string UriApiEmpleados;
        private MediaTypeWithQualityHeaderValue header;

        private IHttpContextAccessor httpContextAccessor;



        public ServiceApiEmpleados(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.UriApiEmpleados = configuration.GetValue<string>("ApiUrls:ApiEmpleados");
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {
            using(HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UriApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                LoginModel model = new LoginModel
                {
                    UserName = username, Password = password
                };
                string jsonData = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UriApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.GetAsync(request);
                if(response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UriApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            string request = "api/empleados";
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request);
            return empleados;
        }

        public async Task<Empleado> GetEmpleadoAsync(int idEmpleado)
        {
            string token =
                this.httpContextAccessor
                .HttpContext.User
                .FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/" + idEmpleado;
            Empleado empleado = await this.CallApiAsync<Empleado>(request);
            return empleado;
        }

        public async Task<Empleado> GetPerfilEmpleadoAsync()
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/perfilempleado";
            Empleado empleado = await this.CallApiAsync<Empleado>(request, token);
            return empleado;
        }

        public async Task<List<Empleado>> GetCompisTrabajoAsync()
        {
            string token = this.httpContextAccessor.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/compiscurro";
            List<Empleado> compis = await this.CallApiAsync<List<Empleado>>(request, token);
            return compis;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            string request = "api/empleados/oficios";
            List<string> oficios = await this.CallApiAsync<List<string>>(request);
            return oficios;
        }

        private string TransformCollectionToQuery(List<string> collection)
        {
            string result = "";
            foreach(string elem in collection)
            {
                result += "oficio=" + elem + "&";
            }
            result = result.TrimEnd('&');
            return result;
        }

        public async Task<List<Empleado>> GetEmpleadosOficiosAsync(List<string> oficios)
        {
            string request = "api/empleados/empleadosoficios";
            string data = this.TransformCollectionToQuery(oficios);
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request + "?" + data);
            return empleados;
        }

        public async Task UpdateEmpleadosOficiosAsync(int incremento, List<string> oficios)
        {
            string request = "api/empleados/incrementarsalariosoficios/" + incremento;
            string data = this.TransformCollectionToQuery(oficios);
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UriApiEmpleados);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.PutAsync(request + "?" + data, null);
            }
        }

    }
}
