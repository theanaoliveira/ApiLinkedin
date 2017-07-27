using LinkedinApi.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace LinkedinApi.Controllers
{
    [RoutePrefix("api/Linkedin")]
    public class LinkedinController : ApiController
    {
        // GET: Linkedin

        /// <summary>
        /// Retorna as informações basicas das empresas encontradas de acordo com o nome informado pelo usuario
        /// </summary>
        /// <param name="empresa">Termos que serão buscados, podem ser informados varios termos separados por ","
        /// <returns>Lista com as informações basicas dos perfis encontrados</returns>
        [Route("empresa")]
        public List<LinkedinInfo> GetEmpresa(string empresa) => Linkedin.GetDataLinkedin(empresa.Split(','));

        /// <summary>
        /// Retorna um perfil completo de uma empresa especifica
        /// </summary>
        /// <param name="codEmpresa">Código da empresa</param>
        /// <returns>Perfil da empresa</returns>
        [Route("perfil")]
        public LinkedinProfile GetProfile(int codEmpresa) => Linkedin.GetProfileLinkedin(codEmpresa);
    }
}