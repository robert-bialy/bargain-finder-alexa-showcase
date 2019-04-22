using System;

namespace AWSSabreLambda.AirLowFareSearch
{
    public interface IOTAAirLowFareSearchRQBuilder
    {
        IOTAAirLowFareSearchRQBuilder WithDestinationLocation(string originLocation, string destinationLocation, DateTime departure);
        IOTAAirLowFareSearchRQBuilder WithRequestedSeats(int seatAmount);
        RootObject Build();
    }
}
