using System;
using System.Collections.Generic;

namespace AWSSabreLambda.AirLowFareSearch
{
    public class DestinationLocation
{
    public string LocationCode { get; set; }
}

public class OriginLocation
{
    public string LocationCode { get; set; }
}

public class OriginDestinationInformation
{
    public DateTime DepartureDateTime { get; set; }
    public DestinationLocation DestinationLocation { get; set; }
    public OriginLocation OriginLocation { get; set; }
    public string RPH { get; set; }
}

public class CompanyName
{
    public string Code { get; set; }
}

public class RequestorID
{
    public CompanyName CompanyName { get; set; }
    public string ID { get; set; }
    public string Type { get; set; }
}

public class Source
{
    public string PseudoCityCode { get; set; }
    public RequestorID RequestorID { get; set; }
}

public class POS
{
    public List<Source> Source { get; set; }
}

public class RequestType
{
    public string Name { get; set; }
}

public class IntelliSellTransaction
{
    public RequestType RequestType { get; set; }
}

public class TPAExtensions
{
    public IntelliSellTransaction IntelliSellTransaction { get; set; }
}

public class DataSources
{
    public string ATPCO { get; set; }
    public string LCC { get; set; }
    public string NDC { get; set; }
}

public class NumTrips
{
}

public class TPAExtensions2
{
    public DataSources DataSources { get; set; }
    public NumTrips NumTrips { get; set; }
}

public class TravelPreferences
{
    public TPAExtensions2 TPA_Extensions { get; set; }
}

public class PassengerTypeQuantity
{
    public string Code { get; set; }
    public int Quantity { get; set; }
}

public class AirTravelerAvail
{
    public List<PassengerTypeQuantity> PassengerTypeQuantity { get; set; }
}

public class TravelerInfoSummary
{
    public List<AirTravelerAvail> AirTravelerAvail { get; set; }
    public List<int> SeatsRequested { get; set; }
}

public class OTAAirLowFareSearchRQ
{
    public List<OriginDestinationInformation> OriginDestinationInformation { get; set; }
    public POS POS { get; set; }
    public TPAExtensions TPA_Extensions { get; set; }
    public TravelPreferences TravelPreferences { get; set; }
    public TravelerInfoSummary TravelerInfoSummary { get; set; }
    public string Version { get; set; }
}

public class RootObject
{
    public OTAAirLowFareSearchRQ OTA_AirLowFareSearchRQ { get; set; }
}
}
