using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CountryApi.Models
{
    public class Region
    {
        public string regionName { get; set; }
        public string normalizedName { get; set; }

        public Region(string name, string normalized){
            regionName = name;
            normalizedName = normalized;
        }
    }

    public class SubRegion
    {
        public string name { get; set; }
        public string capital { get; set; }
        public List<string> altSpellings { get; set; }
        public string relevance { get; set; }
        public string region { get; set; }
        public string subregion { get; set; }
        public Translations translations { get; set; }
        public int population { get; set; }
        public List<double> latlng { get; set; }
        public string demonym { get; set; }
        public int area { get; set; }
        public object gini { get; set; }
        public object timezones { get; set; }
        public List<object> borders { get; set; }
        public string nativeName { get; set; }
        public List<string> callingCodes { get; set; }
        public List<string> topLevelDomain { get; set; }
        public string alpha2Code { get; set; }
        public string alpha3Code { get; set; }
        public List<string> currencies { get; set; }
        public List<string> languages { get; set; }
    }
}