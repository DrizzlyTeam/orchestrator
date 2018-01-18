using System;
using System.Threading.Tasks;
using Drizzly.Commons;
using Drizzly.Orchestrator.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ResponseStatus = Drizzly.Commons.ResponseStatus;

namespace Drizzly.Orchestrator.Controllers
{
    [Route("api")]
    public class ValuesController : Controller
    {
        private readonly ServicesConfig _configuration;

        public ValuesController(IOptions<ServicesConfig> configuration)
        {
            _configuration = configuration.Value;
        }

        [HttpGet("{service}/{value}")]
        public async Task<IActionResult> Get(string service, string value)
        {
            service = service.ToLower();
            value = value.ToLower();
            if (!_configuration.EndPoints.ContainsKey(service))
            {
                return JsonResultExtensions.CreateEmptyResult(ResponseStatus.Error,
                    $"No endpoint found for service {service}");
            }

            var client = new RestClient(_configuration.EndPoints[service]);
            
            var request = new RestRequest(value, Method.GET);

            if (Request.HasFormContentType)
            {
                foreach (var key in Request.Form.Keys)
                {
                    request.AddParameter(key, Request.Form[key]);
                }
            }

            var response = await client.ExecuteTaskAsync(request);

            if (!response.IsSuccessful)
            {
                return JsonResultExtensions.CreateEmptyResult(ResponseStatus.Error,
                    $"Query wasn't succesful, please try again later of contact Drizzly team. " +
                    $"Service: {service}, Value: {value}");
            }

            dynamic t = JObject.Parse(response.Content);
            Console.WriteLine(t.Status);
            return Json(JsonConvert.DeserializeObject(response.Content));
        }
    }
}