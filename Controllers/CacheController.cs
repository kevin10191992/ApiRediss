using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ApiRediss.Controllers
{
    public class CacheController : Controller
    {
        private readonly IDistributedCache _cacheService;

        public CacheController(IDistributedCache cacheService)
        {
            _cacheService = cacheService;
        }


        public async Task<T> ConsultarCache<T>(string llave)
        {
            T res;
            try
            {
                res = DeSerializador<T>(await _cacheService.GetAsync(llave));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        public async Task<bool> GuardarCache<T>(string llave, T Objeto)
        {
            bool res = false;
            try
            {
                int minutos = int.Parse(ConfigurationManager.AppSettings["MinutosElementoEnCache"]);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(minutos)
                };
                await _cacheService.SetAsync(llave, Serializador(Objeto), options);
                res = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        public async Task<bool> LimpiarCache<T>(string llave = "", bool BorrarTodo = false)
        {
            bool res = false;
            try
            {
                if (string.IsNullOrEmpty(llave) && !BorrarTodo) return false;

                await _cacheService.RemoveAsync(llave);
                res = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return res;
        }

        private byte[] Serializador<T>(T Objeto)
        {
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(Objeto);
        }

        private T DeSerializador<T>(byte[] cache)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(cache);
        }
    }
}