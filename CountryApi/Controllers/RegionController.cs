using CountryApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;

namespace CountryApi.Controllers
{
    public class RegionController : ApiController
    {
        // api/regions/
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response;
            var r = GetAllRegions();

            // serialize mislukt
            if (r == null) response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // leeg object
            if (r.Count == 0)
            {
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK, r);
            }
            return response;
        }

        // api/countries/
        public HttpResponseMessage GetCountriesByRegion(string name)
        {
            HttpResponseMessage response;
            var r = GetAllCountriesByRegion(name);

            // serialize mislukt
            if (r == null) response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // leeg object
            if (r.Count == 0)
            {
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK, r);  
            }
            return response;
        }

        // api/dropdown/country
        public HttpResponseMessage GetCountriesDropdown(string name)
        {
            HttpResponseMessage response;
            // haal alle countries van een regio op en maak er een dropdown van
            var r = ExtractCountryDropdown(GetAllCountriesByRegion(name));

            // serialize mislukt
            if (r == null) response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // leeg object
            if (r.Count == 0)
            {
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK, r);
            }
            return response;
        }

        #region Helpers

        private List<Region> GetAllRegions()
        {
            ObjectCache cache = MemoryCache.Default;

            // bestaat deze al in de cache?
            var r = cache.Get("regions") as List<Region>;
            if (r != null)
                return r;

            r = new List<Region>();

            r.Add(new Region("Asia","asia"));
            r.Add(new Region("Africa", "africa"));
            r.Add(new Region("Europe", "europe"));
            r.Add(new Region("Americas", "americas"));
            r.Add(new Region("Oceania", "oceania"));

            // voeg toe aan cache
            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
            cache.Add("regions", r, policy);

            return r;
        }
        private List<Country> GetAllCountriesByRegion(string name)
        {

            ObjectCache cache = MemoryCache.Default;

            // bestaat deze al in de cache?
            var c = cache.Get(name) as List<Country>;
            if (c != null)
                return c;

            // nee, nog niet gecached
            c = new List<Country>();
            using (var webClient = new System.Net.WebClient())
            {
                try
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;

                    var json = webClient.DownloadString("http://restcountries.eu/rest/v1/region/" + name);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        c = JsonConvert.DeserializeObject<List<Country>>(json);

                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add(name, c, policy);

                    }
                }
                catch (Exception ex)
                {
                    if (name.Equals("europe")){
                        var country = new Country();
                        //belgium
                        country.name = "Belgium";
                        c.Add(country);
                        //denmark
                        country = new Country();
                        country.name = "Denmark";
                        c.Add(country);
                        // netherlands
                        country = new Country();
                        country.name = "Netherlands";
                        c.Add(country);
                        //spain
                        country = new Country();
                        country.name = "Spain";
                        c.Add(country);
                        //italy
                        country = new Country();
                        country.name = "Italy";
                        c.Add(country);
                        //albania
                        country = new Country();
                        country.name = "Albania";
                        c.Add(country);
                        //austria
                        country = new Country();
                        country.name = "Austria";
                        c.Add(country);

                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add(name, c, policy);
                    }
                    
                }
            };

            return c;
        }
        private List<Dropdown> ExtractCountryDropdown(List<Country> countries)
        {
            var dd = new List<Dropdown>();

            dd.Add(new Dropdown("Maak een keuze", " "));

            if (countries.Count > 0) {
                foreach (var c in countries) {
                    dd.Add(new Dropdown(c.name));
                }
            }

            return dd;
        }

        #endregion
    }
}
