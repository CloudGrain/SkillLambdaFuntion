using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Net.Http.Headers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace SkillLambda;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
    {
        var accessToken = input.Session.User.AccessToken;
        var log = context.Logger;
        log.LogLine(accessToken);
        var requestType = input.GetRequestType();

        SkillResponse? response = null;

        if(input.Request is LaunchRequest)
        {
           
            response = ResponseBuilder.Tell("Welcome to dot net weather API!");
            response.Response.ShouldEndSession = false;
           
        }
        else if (input.Request is IntentRequest)
        {
            var intentRequest = input.Request as IntentRequest;
            if (intentRequest?.Intent.Name == "WeatherIntent")
            {
                var ApiData = RestAPICall(accessToken);
                Random random = new Random();
                WeatherForecast weater = ApiData[random.Next(ApiData.Count)];
                string str = "weather will be " + weater.Summary + " for date " + weater.Date.ToShortDateString() + " and temperature " + weater.TemperatureC + " degree celsius";
                response = ResponseBuilder.Tell(str);
                response.Response.ShouldEndSession = false;
            }
        }
        else if (input.Request is SessionEndedRequest)
        {
            response = ResponseBuilder.Tell("See you next time!");
            response.Response.ShouldEndSession = true;
        }
        return response;
    }
    private List<WeatherForecast> RestAPICall(string accessToken)
    {
        RestClient client = new RestClient();
        client.Authenticator = new JwtAuthenticator(accessToken);
        RestRequest request = new RestRequest("https://aq40hqfru7.execute-api.us-east-1.amazonaws.com/Prod/api/WeatherForecast");
        var response = client.GetAsync<List<WeatherForecast>>(request);

        return response.Result;
    }
}
public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}
