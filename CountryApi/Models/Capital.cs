using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CountryApi.Models
{
    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }

    public class LatLon
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Bounds
    {
        public LatLon northeast { get; set; }
        public LatLon southwest { get; set; }
    }

    public class Viewport
    {
        public LatLon northeast { get; set; }
        public LatLon southwest { get; set; }
    }

    public class Location
    {
        public Bounds bounds { get; set; }
        public LatLon location { get; set; }
        public string location_type { get; set; }
        public Viewport viewport { get; set; }
    }

    public class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Location geometry { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class Capital
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }
}