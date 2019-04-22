using System;
using System.Collections.Generic;

namespace AWSSabreLambda.AirLowFareSearch
{
    public class OTAAirLowFareSearchRQBuilder : IOTAAirLowFareSearchRQBuilder
    {
        private RootObject _rootObject;
        private OTAAirLowFareSearchRQ _request;
        public OTAAirLowFareSearchRQBuilder()
        {
            _rootObject = new RootObject();
            _request = new OTAAirLowFareSearchRQ();
            _request.POS = GetPOS();
            _request.TPA_Extensions = GetTPAExtensions();
            _request.TravelPreferences = GetTravelPreferences();
            _request.OriginDestinationInformation = new List<OriginDestinationInformation>();
            _request.Version = 1.ToString();
        }

        public IOTAAirLowFareSearchRQBuilder WithDestinationLocation(
            string originLocation, 
            string destinationLocation,
            DateTime departure)
        {
            var originDestinationInformation = new OriginDestinationInformation()
            {
                DepartureDateTime = departure,
                OriginLocation = new OriginLocation()
                {
                    LocationCode = originLocation,
                },
                DestinationLocation = new DestinationLocation()
                {
                    LocationCode = destinationLocation,
                },
                RPH = _request.OriginDestinationInformation.Count.ToString(),
            };
            _request.OriginDestinationInformation.Add(originDestinationInformation);

            return this;
        }

        public IOTAAirLowFareSearchRQBuilder WithRequestedSeats(int seatAmount)
        {
            _request.TravelerInfoSummary = new TravelerInfoSummary()
            {
                AirTravelerAvail = new List<AirTravelerAvail>()
                {
                    new AirTravelerAvail()
                    {
                        PassengerTypeQuantity = new List<PassengerTypeQuantity>()
                        {
                            new PassengerTypeQuantity()
                            {
                                Code = "ADT",
                                Quantity = 1, //todo: change to seatAmount
                            }
                        }
                    }
                },
                SeatsRequested = new List<int>()
                {
                    1
                }
            };

            return this;
        }

        public RootObject Build()
        {
            _rootObject.OTA_AirLowFareSearchRQ = _request;
            return _rootObject;
        }

        private POS GetPOS()
        {
            return new POS()
            {
                Source = new List<Source>
                {
                    new Source
                    {
                        PseudoCityCode = "F9CE",
                        RequestorID = new RequestorID
                        {
                            CompanyName = new CompanyName
                            {
                                Code = "TN",
                            },
                            ID = "1",
                            Type = "1",
                        }
                    }
                }
            };
        }

        private TPAExtensions GetTPAExtensions()
        {
            return new TPAExtensions()
            {
                IntelliSellTransaction = new IntelliSellTransaction()
                {
                    RequestType = new RequestType()
                    {
                        Name = "50ITINS",
                    }
                }
            };
        }

        private TravelPreferences GetTravelPreferences()
        {
            return new TravelPreferences()
            {
                TPA_Extensions = new TPAExtensions2()
                {
                    DataSources = new DataSources()
                    {
                        ATPCO = "Enable",
                        LCC = "Disable",
                        NDC = "Disable",
                    },
                    NumTrips = new NumTrips()
                }
            };
        }
    }
}
