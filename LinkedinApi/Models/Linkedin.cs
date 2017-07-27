using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinkedinApi.Models
{
    /// <summary>
    /// Modelo para armazenar as informações de usuários
    /// </summary>
    public class LinkedinUser
    {
        public string Name { get; set; }
        public string Foto { get; set; }
        public string Cargo { get; set; }
        public string Link { get; set; }
    }

    /// <summary>
    /// Modelo para armazenar as informações de vagas
    /// </summary>
    public class LinkedinVagas
    {
        public string Vaga { get; set; }
        public string Descricao { get; set; }
        public string Local { get; set; }
    }

    /// <summary>
    /// Modelo para armazenar as informações completas do perfil da empresa
    /// </summary>
    public class LinkedinProfile
    {
        public int CodEmpresa { get; set; } = 0;
        public string Nome { get; set; } = "";
        public string Foto { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string[] Especializacoes { get; set; }
        public string Site { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Sede { get; set; } = "";
        public string Tamanho { get; set; } = "";
        public string Ano { get; set; } = "";
        public string Seguidores { get; set; } = "";
        public List<LinkedinVagas> Vagas { get; set; } = new List<LinkedinVagas>();
        public List<LinkedinUser> Usuarios { get; set; } = new List<LinkedinUser>();
    }

    /// <summary>
    /// Modelo para armazenar as informações basicas dos perfis encontrados
    /// </summary>
    public class LinkedinInfo
    {
        public int CodEmpresa { get; set; }
        public string NomeEmpresa { get; set; }
        public string LogoEmpresa { get; set; }
        public string LinkEmpresa { get; set; }
    }

    public class Linkedin
    {
        #region Props

        /// <summary>
        /// Propriedade responsavel por armazenar temporariamente as informações das empresas encontradas na busca do usuário
        /// </summary>
        public static Dictionary<int, LinkedinInfo> DicLinks { get; set; } = new Dictionary<int, LinkedinInfo>();

        #endregion

        #region Methods

        /// <summary>
        /// Método responsável por realizar o login no linkedin
        /// </summary>
        /// <returns>PhantomJSDriver logado</returns>
        private static PhantomJSDriver RealizaLogin()
        {
            var options = new PhantomJSOptions();

            options.AddAdditionalCapability("permissions.default.stylesheet", 2);
            options.AddAdditionalCapability("permissions.default.image", 2);
            options.AddAdditionalCapability("phantomjs.page.settings.userAgent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:25.0) Gecko/20100101 Firefox/35.0");

            var loPhantom = new PhantomJSDriver(options);
            var time = 20;

            loPhantom.Manage().Window.Size = new Size(2000, 2000);
            loPhantom.Navigate().GoToUrl("https://www.linkedin.com/uas/login?goback=&trk=hb_signin");

            var loginForm = loPhantom.FindElementByName("session_key");
            var passForm = loPhantom.FindElementByName("session_password");
            var loButton = loPhantom.FindElementByName("signin");

            loginForm.Clear();
            passForm.Clear();
            loginForm.SendKeys(""); //Login
            passForm.SendKeys(""); //Senha

            loButton.Click();

            var wait = new WebDriverWait(loPhantom, TimeSpan.FromSeconds(time));
            wait.Until(drv => drv.Url.Contains("feed"));

            return loPhantom;
        }

        /// <summary>
        /// Método responsável por recuperar os perfis do linkedin de acordo com o termo informado
        /// </summary>
        /// <param name="parrEmpresa">Array com os termos informados pelo usuário</param>
        /// <returns></returns>
        public static List<LinkedinInfo> GetDataLinkedin(params string[] parrEmpresa)
        {
            var loModel = new List<LinkedinInfo>();
            var loPhantom = RealizaLogin();
            var lListLinks = new List<string>();

            DicLinks = new Dictionary<int, LinkedinInfo>();

            for (var lintCont = 0; lintCont < parrEmpresa.Length; lintCont++)
            {
                loPhantom.Navigate().GoToUrl($"https://www.linkedin.com/search/results/companies/?keywords={parrEmpresa[lintCont]}&origin=GLOBAL_SEARCH_HEADER");

                var larrSearch = loPhantom.FindElement(By.ClassName("results-list")).FindElements(By.ClassName("search-result"));

                for (int lintCont2 = 0; lintCont2 < larrSearch.Count; lintCont2++)
                {
                    try
                    {
                        var info = new LinkedinInfo()
                        {
                            LinkEmpresa = larrSearch[lintCont2].FindElement(By.TagName("a")).GetAttribute("href"),
                            NomeEmpresa = larrSearch[lintCont2].FindElement(By.ClassName("search-result__title")).Text,
                            LogoEmpresa = larrSearch[lintCont2].FindElement(By.ClassName("lazy-image")).GetAttribute("src"),
                        };

                        info.CodEmpresa = GetCodeEmpresa(info.LinkEmpresa);

                        if (!DicLinks.ContainsKey(info.CodEmpresa))
                        {
                            DicLinks.Add(info.CodEmpresa, info);
                            loModel.Add(info);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            loPhantom.Quit();

            return loModel;
        }

        /// <summary>
        /// Recupera as informações da empresa
        /// </summary>
        /// <param name="pintCode">Código da empresa</param>
        public static LinkedinProfile GetProfileLinkedin(int pintCode)
        {
            var model = new LinkedinProfile();

            if (DicLinks != null && DicLinks.ContainsKey(pintCode))
            {
                var loPhantom = RealizaLogin();
                var modelInfo = DicLinks[pintCode];
                var regexObj = new Regex(@"[^\d]");
                var time = 30;

                loPhantom.Navigate().GoToUrl(modelInfo.LinkEmpresa);

                var wait = new WebDriverWait(loPhantom, TimeSpan.FromSeconds(time));
                wait.Until(drv => drv.FindElements(By.Id("org-about-company-module__show-details-btn")).Count > 0);

                //Clica no botão para exibir todas as informações da tela
                var btn = loPhantom.FindElement(By.Id("org-about-company-module__show-details-btn"));
                btn.Click();

                model.CodEmpresa = pintCode;
                model.Nome = GetPropValue("org-top-card-module__name", "text", loPhantom);
                model.Foto = GetPropValue("org-top-card-module__container", "img", loPhantom);
                model.Descricao = GetPropValue("org-about-us-organization-description__text", "text", loPhantom);
                model.Especializacoes = GetPropValue("org-about-company-module__specialities", "text", loPhantom).Split(',');
                model.Sede = GetPropValue("org-about-company-module__headquarters", "text", loPhantom); ;
                model.Tamanho = GetPropValue("org-about-company-module__company-staff-count-range", "text", loPhantom); ;
                model.Site = GetPropValue("org-about-us-company-module__website", "href", loPhantom);
                model.Ano = GetPropValue("org-about-company-module__founded", "text", loPhantom);
                model.Tipo = GetPropValue("org-about-company-module__company-type", "text", loPhantom);
                model.Seguidores = regexObj.Replace(GetPropValue("org-top-card-module__followers-count", "text", loPhantom), "");
                model.Vagas = GetOfficials(loPhantom, pintCode.ToString());
                model.Usuarios = GetPeoples(loPhantom, pintCode.ToString());

                loPhantom.Quit();
            }

            return model;
        }

        /// <summary>
        /// Recupera as informações de acordo com uma determinada classe
        /// </summary>
        /// <param name="pstrClassName">Classe</param>
        /// <param name="pstrProp">Propriedade a ser recuperada</param>
        /// <param name="poPhantom">Objeto phantomjs</param>
        /// <returns>Texto com valor desejado</returns>
        private static string GetPropValue(string pstrClassName, string pstrProp, PhantomJSDriver poPhantom)
        {
            var lstrValue = "";

            try
            {
                switch (pstrProp)
                {
                    case "href":
                        lstrValue = poPhantom.FindElementByClassName(pstrClassName).GetAttribute("href"); //recupera o atributo href
                        break;
                    case "img":
                        lstrValue = poPhantom.FindElementByClassName(pstrClassName).FindElement(By.TagName("img")).GetAttribute("src"); //recupera o caminho da imagem
                        break;
                    default:
                        lstrValue = poPhantom.FindElementByClassName(pstrClassName).Text; //recupera o texto do campo
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return lstrValue;
        }

        /// <summary>
        /// Método retorna somente os numeros de uma URL, utilizado para recuperar o código da empresa
        /// </summary>
        /// <param name="pListUrl">Lista com as URL's encontradas</param>
        private static int GetCodeEmpresa(string pstrUrl)
        {
            var regexObj = new Regex(@"[^\d]");
            var lstrResult = regexObj.Replace(pstrUrl, "");

            return Convert.ToInt32(lstrResult);
        }

        /// <summary>
        /// Método responsável por retornar uma lista com as vagas anunciadas pela empresa
        /// </summary>
        /// <param name="poPhantom">Objeto phantomjsdriver</param>
        /// <param name="pstrCode">Codigo da empresa</param>
        /// <returns></returns>
        private static List<LinkedinVagas> GetOfficials(PhantomJSDriver poPhantom, string pstrCode)
        {
            var loModel = new List<LinkedinVagas>();

            poPhantom.Navigate().GoToUrl($"https://www.linkedin.com/jobs/search/?locationId=OTHERS%2Eworldwide&f_C={pstrCode}");

            if (poPhantom.FindElements(By.ClassName("job-card__link-wrapper")).Count() > 0)
            {
                var jobs = poPhantom.FindElements(By.ClassName("job-card__link-wrapper"));

                for (int lintCont = 0; lintCont < jobs.Count; lintCont++)
                {
                    var model = new LinkedinVagas();

                    model.Vaga = jobs[lintCont].FindElement(By.ClassName("truncate-multiline--last-line-wrapper")).Text;
                    model.Descricao = jobs[lintCont].FindElement(By.ClassName("job-card__description-snippet")).Text;
                    model.Local = jobs[lintCont].FindElement(By.ClassName("job-card__location")).Text;

                    loModel.Add(model);
                }
            }

            return loModel;
        }

        /// <summary>
        /// Método responsável por retornar os funcionarios da empresa selecionada
        /// </summary>
        /// <param name="poPhantom">Objeto phantomjsdriver</param>
        /// <param name="pstrCode">Codigo da empresa</param>
        /// <returns></returns>
        public static List<LinkedinUser> GetPeoples(PhantomJSDriver poPhantom, string pstrCode)
        {
            var pageCount = 5;
            var loModel = new List<LinkedinUser>();
            var url = $"https://www.linkedin.com/search/results/people/?facetCurrentCompany=%5B\"{pstrCode}\"%5D";

            for (int lintCont = 1; lintCont <= pageCount; lintCont++)
            {
                poPhantom.Navigate().GoToUrl($"{url}&page={lintCont}");

                var users = poPhantom.FindElementsByClassName("search-result__wrapper");

                for (int lintCont2 = 0; lintCont2 < users.Count; lintCont2++)
                {
                    var model = new LinkedinUser();

                    model.Name = users[lintCont2].FindElement(By.ClassName("actor-name")).Text;
                    model.Foto = users[lintCont2].FindElement(By.TagName("img")).GetAttribute("src");
                    model.Cargo = users[lintCont2].FindElement(By.ClassName("subline-level-1")).Text;
                    model.Link = users[lintCont2].FindElement(By.ClassName("search-result__result-link")).GetAttribute("href");

                    loModel.Add(model);
                }
            }

            return loModel;
        }

        #endregion
    }
}