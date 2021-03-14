using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace ApiRediss.Controllers
{
    public class ValuesController : ApiController
    {
        private static readonly CacheController _cacheController = new CacheController();

        [Route("api/Values/Hola")]
        [HttpPost]
        public async Task<JObject> Hola([FromBody] JObject req)
        {
            JObject res = new JObject();


            if (!req.ContainsKey("TipoDocumento") || !req.ContainsKey("NumeroDocumento"))
            {
                res["Codigo"] = "02";
                res["Respuesta"] = "Parametros incorrectos";
                return res;
            }


            try
            {
                string TipoDocumento = req["TipoDocumento"].ToString();
                string NumeroDocumento = req["NumeroDocumento"].ToString();
                bool cifrar = req.ContainsKey("Cifrado") && bool.Parse(req["Cifrado"].ToString());
                string llave = $"{TipoDocumento}-{NumeroDocumento}";

                var resCache = await _cacheController.ConsultarCache<JObject>(llave, cifrar);

                if (resCache != null)
                {
                    res["Codigo"] = "01";
                    res["Descripcion"] = "Datos Encontrados en cache";
                    res["Cifrado"] = cifrar;
                    res["Datos"] = resCache;
                }
                else
                {

                    JObject hola = new JObject { { "Hola", "manco" } };
                    hola["Data"] = JObject.Parse(@"{'_id':'604e97365d4296b6c329e0bc','index':0,'guid':'3bad6bbd-2d41-4bf3-888e-c7867676768b','isActive':false,'balance':'$2,878.77','picture':'http://placehold.it/32x32','age':20,'eyeColor':'blue','name':'Knowles Russo','gender':'male','company':'TRIPSCH','email':'knowlesrusso@tripsch.com','phone':'+1 (837) 517-2372','address':'162 Elizabeth Place, Davenport, Kansas, 1600','about':'Quis excepteur irure officia eiusmod ullamco laborum reprehenderit pariatur sunt. Adipisicing pariatur commodo consectetur deserunt adipisicing eu Lorem aliquip eu. Duis nulla aute dolor pariatur sint proident velit ut dolore dolor culpa nostrud qui. In nulla duis amet voluptate et ut nostrud eiusmod in incididunt. Ut incididunt voluptate anim dolor esse. Non dolore reprehenderit velit id commodo pariatur minim dolor in qui veniam aute. Consequat ullamco irure nostrud est culpa dolor.\r\n','registered':'2019-09-10T08:52:57 +05:00','latitude':52.394169,'longitude':-79.775837,'tags':['tempor','culpa','tempor','ipsum','esse','nulla','in'],'friends':[{'id':0,'name':'Elvia Humphrey'},{'id':1,'name':'Hazel Booth'},{'id':2,'name':'Abby Palmer'}],'greeting':'Hello, Knowles Russo! You have 8 unread messages.','favoriteFruit':'strawberry'}");
                    res["Codigo"] = "01";
                    res["Descripcion"] = "Datos Encontrados";
                    res["Datos"] = hola;


                    Task.Run(async () => await _cacheController.GuardarCache(llave, hola, cifrar));
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
