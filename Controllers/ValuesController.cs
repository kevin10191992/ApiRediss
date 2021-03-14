using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace ApiRediss.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly IDistributedCache _cacheService;
        private readonly CacheController _cacheController;
        public ValuesController(IDistributedCache cacheService)
        {
            _cacheService = cacheService;
            _cacheController = new CacheController(_cacheService);
        }




        [Route("api/Values/Hola")]
        [HttpGet]
        public async Task<JObject> Hola([FromBody] JObject req)
        {
            JObject res = new JObject();


            if (!req.ContainsKey("TipoDocumento") || !req.ContainsKey("NumeroDocumento"))
            {
                res["Codigo"] = "02";
                res["Respuesta"] = "Parametros incorrectos";
                return res;
            }

            string TipoDocumento = req["TipoDocumento"].ToString();
            string NumeroDocumento = req["NumeroDocumento"].ToString();
            string llave = $"{TipoDocumento}-{NumeroDocumento}";
            try
            {
                var resCache = await _cacheController.ConsultarCache<JObject>(llave);

                if (resCache != null)
                {
                    res["Codigo"] = "01";
                    res["Descripcion"] = "Datos Encontrados en cache";
                    res["Datos"] = resCache;
                }
                else
                {
                    JObject hola = new JObject { { "Hola", "Hola" } };

                    res["Codigo"] = "01";
                    res["Descripcion"] = "Datos Encontrados";
                    res["Datos"] = hola;

                    Task.Run(() => _cacheController.GuardarCache(llave, hola)).Start();
                }
            }
            catch (Exception ex)
            {
                res["Codigo"] = "01";
                res["Descripcion"] = "Ha ocurrido un error";
                res["DetalleError"] = ex.ToString();
            }

            return res;
        }
    }
}
