using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using AWSSabreLambda.AirLowFareSearch;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSSabreLambda
{
    public class Function
    {
        private HttpClient _httpClient;
        private const string INVOCATION_NAME = "Schedule Info";

        //sample usage: alexa ask schedule info from new york city to los angeles

        public Function()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", @"T1RLAQIZCDkGcoxddjfXIqaxkDgmuJqycxDuqN0smXUY/jSRwV0L0r/4AADAkh9BFeDlf3q0B9Wg8H+IWmp311Fg4Q+kVX+GFy0EI3Ipi0K+OfrTn3rTtsE0IooFEqzMMoGBJYVPsOg6sfQBlg7hyKPYgREHofJCY2vXRAFtDPElPAN90SM/nj4wmy6mTXoeCkx6zVXtuGyInmMeqtRRpyE//n+xHVIXbNodnbPFArJrq2S7OpdW0vCIcQVwGYAlWbSX4kOBx0T+5NLt4jf/AfsLXS9mseTRz7D0eLDao6e8mEMLTpuvunXDFPk4");
        }
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var requestType = input.GetRequestType();
            if (requestType == typeof(IntentRequest))
            {
                if (input.Request is IntentRequest intentRequest)
                {
                    if (intentRequest.Intent.Slots.ContainsKey("Origin") && intentRequest.Intent.Slots.ContainsKey("Destiny"))
                    {
                        var originCode = CityNameCityCodeMapper.MapToCode(intentRequest.Intent.Slots["Origin"].Value.ToLower());
                        var destinyCode = CityNameCityCodeMapper.MapToCode(intentRequest.Intent.Slots["Destiny"].Value.ToLower());
                        DateTime? date = intentRequest.Intent.Slots.ContainsKey("Date")
                            ? DateTime.Parse(intentRequest.Intent.Slots["Date"].Value)
                            : (DateTime?)null;

                        var response = await GetItineraryInfo(originCode, destinyCode, date, context);
                        return MakeSkillResponse(response, false);
                    }
                }
                else
                {
                    context.Logger.LogLine($"The country was not understood.");
                    return MakeSkillResponse("I'm sorry, but I didn't understand the country you were asking for. Please ask again.", false);
                }

        }
            return MakeSkillResponse(
                $"I don't know how to handle this intent. Please say something like Alexa, ask {INVOCATION_NAME} from New York City to Los Angeles.",
                true);
        }

        private SkillResponse MakeSkillResponse(
            string outputSpeech,
            bool shouldEndSession,
            string repromptText = "Just say, from New York City to Los Angeles. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech() { Text = repromptText } };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }

        private async Task<string> GetItineraryInfo(string originCode, string destinyCode, DateTime? date, ILambdaContext context)
        {
            var message = new StringBuilder();
            var response = await GetResultsForIteinaryInfo(originCode, destinyCode, date ?? DateTime.Now, context);
            var itineraryCount = response?.statistics?.itineraryCount ?? 0;
            if (itineraryCount == 0) return "Sorry, I wasn't able to find any itinerary";
            message.AppendLine($"I've managed to find {itineraryCount} itineraries.");

            var (schedule, pricingInformation) = GetCheapestFlightInfo(response?.itineraryGroups.First().itineraries, response?.legDescs, response?.scheduleDescs, originCode, destinyCode);
            message.Append(GetScheduleMessage(schedule));
            message.Append(GetPricingMessage(pricingInformation));

            return  message.ToString();
        }

        private string GetPricingMessage(PricingInformation pricingInformation)
        {
            var totalFare = pricingInformation.fare.totalFare;
            return $"The cheapest price I could find is {totalFare.totalPrice} {totalFare.currency}.";
        }

        private string GetScheduleMessage(List<ScheduleDesc> schedule)
        {
            var message = new StringBuilder();
            if (schedule.Count == 1)
            {
                var flight = schedule.Single();
                DateTime time = DateTime.Parse(flight.departure.time);
                var depart = !string.IsNullOrEmpty(CityNameCityCodeMapper.MapToLocation(flight.departure.airport))
                    ? CityNameCityCodeMapper.MapToLocation(flight.departure.airport)
                    : flight.departure.airport;
                var arrival = !string.IsNullOrEmpty(CityNameCityCodeMapper.MapToLocation(flight.arrival.airport))
                    ? CityNameCityCodeMapper.MapToLocation(flight.arrival.airport)
                    : flight.arrival.airport;
                message.AppendLine($"It's direct flight from {depart} to {arrival} at {time.TimeOfDay}");
            }
            else
            {
                foreach (var flight in schedule)
                {
                    DateTime time = DateTime.Parse(flight.departure.time);
                    var depart = !string.IsNullOrEmpty(CityNameCityCodeMapper.MapToLocation(flight.departure.airport))
                        ? CityNameCityCodeMapper.MapToLocation(flight.departure.airport)
                        : flight.departure.airport;
                    var arrival = !string.IsNullOrEmpty(CityNameCityCodeMapper.MapToLocation(flight.arrival.airport))
                        ? CityNameCityCodeMapper.MapToLocation(flight.arrival.airport)
                        : flight.arrival.airport;
                    message.AppendLine($"Take {depart} to {arrival} at {time.TimeOfDay}.");
                }
            }

            return message.ToString();
        }

        private KeyValuePair<List<ScheduleDesc>, PricingInformation> GetCheapestFlightInfo(List<Itinerary> itineraries, List<LegDesc> legDescriptions, List<ScheduleDesc> scheduleDescs, string originCode, string destinyCode)
        {
            if (legDescriptions == null || scheduleDescs == null) return new KeyValuePair<List<ScheduleDesc>, PricingInformation>();
            var itinerariesSortedByPrice = itineraries.OrderBy(it => it.pricingInformation.Single().fare.totalFare.totalPrice).ToArray();
            var flightSchedule = GetFlightSchedule(itinerariesSortedByPrice.First().legs.Single(), legDescriptions, scheduleDescs);

            return new KeyValuePair<List<ScheduleDesc>, PricingInformation>(flightSchedule,
                itinerariesSortedByPrice.First().pricingInformation.Single());
        }

        private List<ScheduleDesc> GetFlightSchedule(Leg leg, List<LegDesc> legDescriptions, List<ScheduleDesc> scheduleDescs)
        {
            var listOfschedules = legDescriptions.Find(legDesc => leg.@ref == legDesc.id)
                .schedules.Select(sch => scheduleDescs.Find(schDesc => sch.@ref == schDesc.id)).ToList();
            return listOfschedules;
        }

        private async Task<GroupedItineraryResponse> GetResultsForIteinaryInfo(string originCode, string destinyCode, DateTime date, ILambdaContext context)
        {
            GroupedItineraryResponse iteineraryResponse = null;
            var uri = new Uri($"https://api-crt.cert.havail.sabre.com/v1/offers/shop");
            try
            {
                var requestBuilder = new OTAAirLowFareSearchRQBuilder()
                    .WithDestinationLocation(originCode, destinyCode, date.Date)
                    .WithRequestedSeats(1);
                var request = JsonConvert.SerializeObject(requestBuilder.Build(), new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddThh:mm:ss" });
                var requestJson = new StringContent(request, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(uri, requestJson);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var converted = JsonConvert.DeserializeObject<RootObject>(jsonResponse);
                iteineraryResponse = converted?.groupedItineraryResponse;
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"\nException: {ex.Message}");
                context.Logger.LogLine($"\nStack Trace: {ex.StackTrace}");
            }
            
            return iteineraryResponse;
        }
    }
    
}
