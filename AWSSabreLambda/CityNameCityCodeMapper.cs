using System;
using System.Collections.Generic;
using System.Linq;

namespace AWSSabreLambda
{
    public class CityNameCityCodeMapper
    {
        private static Dictionary<string, string> CityNameCityCodeDictionary = new Dictionary<string, string>()
        {
            {"new york city", "NYC"},
            {"los angeles", "LAX"},
            {"new orleans", "MSY" },
            {"newark-liberty nyc", "EWR"},
            {"philadelphia", "PHL" },
            {"boston", "BOS"},
            {"dallas", "DAL" },
            {"del rio", "DRT" },
            {"mexico city", "MEX" },
            {"charlotte douglas", "CLT" }
        };

        public static string MapToCode(string cityName)
        {
            return CityNameCityCodeDictionary.ContainsKey(cityName) ?
            CityNameCityCodeDictionary[cityName] :
            string.Empty;
        }

        public static string MapToLocation(string cityCode)
        {
            if(!CityNameCityCodeDictionary.ContainsValue(cityCode)) return String.Empty;
            return CityNameCityCodeDictionary.FirstOrDefault(x => x.Value == cityCode).Key;
        }
    }
}
