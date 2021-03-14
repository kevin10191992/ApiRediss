using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ApiRediss.Controllers
{
    public class CacheController : Controller
    {

        private readonly ConnectionMultiplexer _cacheService;
        public readonly IDatabase _db;
        public static readonly string ESTADO_CIFRADO = "-Cypher";
        public CacheController()
        {
            string conexion = ConfigurationManager.AppSettings["REDIS_URL"].ToString();
            _cacheService = ConnectionMultiplexer.Connect(conexion);
            _db = _cacheService.GetDatabase(0);//la 0 es por defecto
        }


        public async Task<T> ConsultarCache<T>(string llave, bool cifrado)
        {
            T res;
            try
            {
                llave = (cifrado) ? llave + ESTADO_CIFRADO : llave;
                if (await _db.KeyExistsAsync(llave))
                {
                    var objetoCache = await _db.StringGetAsync(llave);
                    res = DeSerializador<T>(objetoCache, cifrado);
                }
                else
                {
                    return default;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        public async Task<bool> GuardarCache<T>(string llave, T Objeto, bool cifrado)
        {
            bool res = false;
            try
            {
                int minutos = int.Parse(ConfigurationManager.AppSettings["MinutosElementoEnCache"]);
                llave = (cifrado) ? llave += ESTADO_CIFRADO : llave;
                await _db.StringSetAsync(llave, Serializador(Objeto, cifrado), TimeSpan.FromMinutes(minutos));
                res = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        public async Task<bool> LimpiarCache(string llave = "", bool BorrarTodo = false)
        {
            bool res = false;
            try
            {
                if (string.IsNullOrEmpty(llave) && !BorrarTodo) return false;

                if (BorrarTodo)
                {
                    var server = _cacheService.GetServer(_cacheService.GetEndPoints(true)[0]);
                    await server.FlushDatabaseAsync(0);
                }
                else
                {
                    await _db.KeyDeleteAsync(llave);
                }

                res = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        private byte[] Serializador<T>(T Objeto, bool cifrado)
        {
            byte[] datos;
            string serializado = Newtonsoft.Json.JsonConvert.SerializeObject(Objeto);
            if (cifrado)
            {
                datos = Encoding.UTF8.GetBytes(serializado);
                datos = Encoding.UTF8.GetBytes(Convert.ToBase64String(datos));
            }
            else
            {
                datos = Encoding.UTF8.GetBytes(serializado);
            }

            return datos;
        }

        private T DeSerializador<T>(byte[] cache, bool cifrado)
        {

            if (cifrado)
            {
                cache = Convert.FromBase64String(Encoding.UTF8.GetString(cache));
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(cache));
        }
    }
}