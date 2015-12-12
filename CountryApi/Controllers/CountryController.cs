using CountryApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http;

namespace CountryApi.Controllers
{
    public class CountryController : ApiController
    {
        // api/country/name
        public HttpResponseMessage Get(string name)
        {
            HttpResponseMessage response;

            var c = GetCountry(name);

            // serialize mislukt
            if (c == null) response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // leeg object
            if (c.name == null)
            {
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK, c);
            }
            return response;
        }

        public HttpResponseMessage Post([FromBody] Border countryData) {

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.NoContent);

            if (countryData != null)
            {
                try
                {
                    // haal geojson op
                    var p = HttpContext.Current.Server.MapPath("~/geojson");

                    if (p != null)
                    {
                        string[] filePaths = Directory.GetFiles(p);

                        var found = false;
                        var newCountryName = string.Empty;

                        foreach (var f in filePaths)
                        {
                            var oldCountryName = Path.GetFileNameWithoutExtension(f).Substring(0, Path.GetFileNameWithoutExtension(f).IndexOf('.')).ToLower();
                            newCountryName = countryData.features[0].properties.name.ToLower();

                            if (newCountryName.Equals(oldCountryName) && !found)
                            {
                                string newdata = string.Empty;
                                

                                // de-serialize de json naar een object
                                using (StreamReader r = new StreamReader(f))
                                {
                                    string json = r.ReadToEnd();
                                    Border oldCountry = JsonConvert.DeserializeObject<Border>(json);

                                    if (oldCountry != null)
                                    {
                                        var newLatLonCount = countryData.features[0].geometry.coordinates[0].Count();
                                        var oldLatLonCount = oldCountry.features[0].geometry.coordinates[0].Count();

                                        if (newLatLonCount > oldLatLonCount)
                                        {
                                            // nieuw bestand heeft meer lat/lon punten
                                            // dus meer details, sla de nieuwe op
                                            // anders bewaar de oude

                                            newdata = JsonConvert.SerializeObject(countryData);
                                        }
                                        else {
                                            response = Request.CreateResponse(HttpStatusCode.OK, new Feedback("niet genoeg punten", "INFO"));
                                        }
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(newdata)) {

                                    //write string to file
                                    System.IO.File.WriteAllText(f, newdata);

                                    // verwijder uit de cache
                                    RemoveFromCache(newCountryName);
                                    response = Request.CreateResponse(HttpStatusCode.OK, new Feedback("Je data voor " + oldCountryName + " is opgeslagen", "INFO"));
                                }

                                found = true;
                            }
                        }

                        if (!found) { 
                            // geen land gevonden op schijf
                            System.IO.File.WriteAllText(p + @"\" + newCountryName + ".geo.json", JsonConvert.SerializeObject(countryData));
                            response = Request.CreateResponse(HttpStatusCode.OK, new Feedback("Je data " + newCountryName + " is opgeslagen in een nieuw bestand", "INFO"));
                        }
                    }   
                }
                catch (Exception ex)
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, new Feedback(ex.Message, "ERROR"));
                }
            }
            else {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, new Feedback("Geen data", "ERROR"));
            }
            
            return response;
        }

        [HttpGet]
        public HttpResponseMessage NumberOfGeoJsonPoints(string name)
        {
            HttpResponseMessage response;
            var c = GetCountry(name);

            // serialize mislukt
            if (c == null) response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // leeg object
            if (c.name == null)
            {
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.OK, new Feedback("Aantal punten van " + name + ": " + c.geodata.coordinates[0].Count(), "INFO"));
               // response = Request.CreateResponse(HttpStatusCode.OK, new Feedback("Aantal punten van " + name + ": " + c.geodata.geometry.coordinates[0].Count(), "INFO"));
            }
            return response;
        }

        #region Helpers

        private void RemoveFromCache(string country) {
            ObjectCache cache = MemoryCache.Default;
            var c = cache.Get(country) as Country;
            if (c != null) cache.Remove(country);
        }

        // doe een call naar restcoutries.eu
        private Country GetCountry(string name)
        {
            ObjectCache cache = MemoryCache.Default;

            // bestaat deze al in de cache?
            var c = cache.Get(name) as Country;
            if (c != null)
                return c;

            // nee, nog niet gecached
            c = new Country();
            using (var webClient = new System.Net.WebClient())
            {
                try
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;

                    var json = webClient.DownloadString("http://restcountries.eu/rest/v1/name/" + name);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        // json staat in een array.
                        // hebben we niet nodig -> verwijder!
                        if (json.StartsWith("["))
                        {
                            json = json.Substring(1);
                            if (json.EndsWith("]"))
                            {
                                json = json.Substring(0, json.Length - 1);
                            }
                        }

                        c = JsonConvert.DeserializeObject<Country>(json);

                        // haal de geojson op
                        c.geodata = GetBorderData(c.name);

                        // haal de lat/lon op van de hoofdstad
                        if (!string.IsNullOrWhiteSpace(c.capital) && !string.IsNullOrWhiteSpace(c.name))
                        {
                            c.capitalDetails = GetCapitalLatLon(c.capital + " " + c.name);
                        }

                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add(name, c, policy);

                    }
                }
                catch (Exception ex)
                {
                    if (name.Equals("belgium")) { 
                        var c1 = createBelgium();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("belgium", c1, policy);
                        return c1;
                    }

                    if (name.Equals("netherlands") || (name.Equals("netherland"))) 
                    {
                        var c1 = createNetherlands();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("netherlands", c1, policy);
                        return c1;
                    }

                    if (name.Equals("denmark")) 
                    {
                        var c1 = createDenmark();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("denmark", c1, policy);
                        return c1;
                    }

                    if (name.Equals("spain"))
                    {
                        var c1 = createSpain();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("spain", c1, policy);
                        return c1;
                    }

                    if (name.Equals("italy"))
                    {
                        var c1 = createItaly();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("italy", c1, policy);
                        return c1;
                    }

                    if (name.Equals("austria"))
                    {
                        var c1 = createAustria();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("austria", c1, policy);
                        return c1;
                    }

                    if (name.Equals("albania"))
                    {
                        var c1 = createAlbania();
                        // voeg toe aan cache
                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(4) };
                        cache.Add("albania", c1, policy);
                        return c1;
                    }
                }
            };

            return c;
        }

        // details van de hoofdstad (google maps)
        private Capital GetCapitalLatLon(string capitalcountry)
        {
            var capital = new Capital();

            using (var webClient = new System.Net.WebClient())
            {
                try
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;

                    var json = webClient.DownloadString("https://maps.googleapis.com/maps/api/geocode/json?address=" + capitalcountry);

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        capital = JsonConvert.DeserializeObject<Capital>(json);
                    }
                }
                catch (Exception ex) { }
            }

            return capital;
        }

        // lijst van geojson data in MemoryCache
        private List<Border> LoadBorders()
        {
            var borders = new List<Border>();
            try
            {
                // haal geojson op
                var p = HttpContext.Current.Server.MapPath("~/geojson");

                if (p != null)
                {
                    string[] filePaths = Directory.GetFiles(p);

                    foreach (var f in filePaths)
                    {
                        var t = File.ReadAllText(f);
                        if (!string.IsNullOrWhiteSpace(t))
                        {
                            //try
                            //{
                                var b = new Border();
                                b = JsonConvert.DeserializeObject<Border>(t);
                                if (b.type.Equals("FeatureCollection"))
                                {
                                    borders.Add(b);
                                }
                            //}
                            //catch (Exception ex)
                            //{
                           // }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            

            return borders;
        }

        // zoek in geojson de data van een land op
        private Geometry GetBorderData(string country)
       {
            var borders = LoadBorders();
            var geo = new Geometry();

            foreach (var b in borders)
            {
                foreach (var f in b.features)
                {
                    if (f.properties.name.ToLower().Equals(country.ToLower()))
                    {
                        if (f.geometry != null)
                        {
                            geo = f.geometry;
                            break;
                        }
                    }
                }
            }

            return geo;
        }

        //private Feature GetBorderData(string country)
        //{
        //    var borders = LoadBorders();
        //    var feature = new Feature();

        //    foreach (var b in borders)
        //    {
        //        foreach (var f in b.features)
        //        {
        //            if (f.properties.name.ToLower().Equals(country.ToLower()))
        //            {
        //                if (f.geometry != null)
        //                {
        //                    feature = f;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    return feature;
        //}

        #endregion

        #region Fake Countries
        private Country createBelgium() {
            var c = new Country();
            c.name = "Belgium";
            c.capital = "Brussels";
            c.region = "europe";
            c.latlng = new List<double>() { 50.503887, 4.469936 };
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        private Country createNetherlands()
        {
            var c = new Country();
            c.name = "Netherlands";
            c.capital = "Amsterdam";
            c.region = "europe";
            c.latlng = new List<double>() { 52.132633, 5.291266 };
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        private Country createDenmark()
        {
            var c = new Country();
            c.name = "Denmark";
            c.capital = "Copenhagen";
            c.region = "europe";
            c.latlng = new List<double>() { 56.263920, 9.501785 };
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        private Country createSpain()
        {
            var c = new Country();
            c.name = "Spain";
            c.capital = "Madrid";
            c.region = "europe";
            c.latlng = new List<double>() { 40.463667, -3.749220 };
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        private Country createItaly()
        {
            var c = new Country();
            c.name = "Italy";
            c.capital = "Rome";
            c.region = "europe";
            c.latlng = new List<double>() { 41.871940, 12.567380 };
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        private Country createAustria()
        {
            var c = new Country();
            c.name = "Austria";
            c.capital = "Vienna";
            c.region = "europe";
            c.latlng = new List<double>() { 47.516231, 14.550072 };
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        private Country createAlbania()
        {
            var c = new Country();
            c.name = "Albania";
            c.capital = "Tirana";
            c.region = "europe";
            c.latlng = new List<double>() { 41.153332, 20.168331 }; ;
            c.geodata = GetBorderData(c.name);
            c.capitalDetails = GetCapitalLatLon(c.capital + c.name);
            return c;
        }
        #endregion

    }
}
