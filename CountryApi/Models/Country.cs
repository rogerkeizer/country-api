using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CountryApi.Models
{
    // convert json to object
    //http://json2csharp.com/

    public class Country
    {
        public string name { get; set; }
        public string capital { get; set; }
        public List<string> altSpellings { get; set; }
       // public string relevance { get; set; }
        public string region { get; set; }
        public string subregion { get; set; }
        public Translations translations { get; set; }
        public int population { get; set; }
        public List<double> latlng { get; set; }
       // public string demonym { get; set; }
       // public double area { get; set; }
       // public double gini { get; set; }
       // public object timezones { get; set; }
        public List<string> borders { get; set; }
        public string nativeName { get; set; }
       // public List<string> callingCodes { get; set; }
        public List<string> topLevelDomain { get; set; }
       // public string alpha2Code { get; set; }
       // public string alpha3Code { get; set; }
        public List<string> currencies { get; set; }
        public List<string> languages { get; set;}
        public Geometry geodata { get; set; }
      //  public Feature geodata { get; set; } TODO: javascript aanpassen zodat een Feature wordt gebruikt ipv een Geometry
        public DateTime cachetime { get; set; }
        public Capital capitalDetails { get; set; }

        public Country() {
            cachetime = DateTime.Now;
            capitalDetails = new Capital();
        }
    }

    public class Properties
    {
        public string name { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<List<List<double>>> coordinates { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public string id { get; set; }
        public Properties properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Border
    {
        public string type { get; set; }
        public List<Feature> features { get; set; }
    }
}