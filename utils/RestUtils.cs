using RestSharp;
using static Utils.StringUtils;
using Polly;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Cards;

namespace Utils
{
  public static class RestUtils
  {
    //Basic REST implementation.
    static RestResponse ExecuteRestRequest(string url, string? message = null) =>
      new RestClient(new RestClientOptions(url)).Execute(new RestRequest());
    // Policy.HandleResult<RestResponse>(response => !response.IsSuccessful)
    //   .WaitAndRetry(5, span => TimeSpan.FromSeconds(2), (iRestResponse, timeSpan, retryCount, context) =>
    //   {
    //     throw new Exception(message ?? $"An error occurred with endpoint {url}");
    //   }).Execute(() => new RestClient(new RestClientOptions(url)).Execute(new RestRequest()));

    public static void DownloadCardImage(Card card)
    {
      var asd = card.exactSet!.Value;
      string set = card.exactSet.Value.set.Replace("-", "").ToLower();

      string setnum = card.exactSet.Value.setnum;
      if (setnum.StartsWith("S"))
        setnum = "S" + int.Parse(KeepNumericCharacters(setnum)[0]).ToString("00");
      else
        setnum = int.Parse(card.exactSet.Value.setnum).ToString("000");
      if (card.cardType == "Twinpact")
      {
        setnum += "a";
      }
      var uri = $"{set}-{setnum}.jpg";
      var url = URL_QUERY_IMAGE + uri;

      //Check if folder exists first.
      if (!Directory.Exists(FILEPATH_DATA_PICS($"\\{card.exactSet!.Value.set}")))
        Directory.CreateDirectory(FILEPATH_DATA_PICS($"\\{card.exactSet!.Value.set}"));
      try
      {
        File.WriteAllBytes(FILEPATH_DATA_PICS($"\\{card.exactSet!.Value.set}\\{card.EN_CardName.Replace("/", "∕")}.jpg"),
          new RestClient(URL_QUERY_IMAGE).DownloadData(new RestRequest(uri, Method.Get))!);
      }
      catch
      {
        return;
      }
    }



    //Imports OCG cards from duel masters Fandom and returns arrays of data.
    public static string ImportOCGCards(string nextPageIdentifier) =>
      ExecuteRestRequest(nextPageIdentifier.Contains("FIRST_PAGE") ?
        URL_QUERY_CARDS :
        $"{URL_QUERY_CARDS}&eicontinue={nextPageIdentifier}").Content
          ?? throw new Exception($"Error retrieving card pages from {URL_QUERY_CARDS}&eicontinue={nextPageIdentifier}");

    //Imports all details regarding a specific card.
    public static string ImportCardByName(string cardName)
    {
      var response = ExecuteRestRequest(URL_QUERY_SINGLE + cardName);
      //Extra check to make sure card is found. Must be in place.
      if (response.Content!.Split("\n|").Count() < 3)
        throw new Exception($"{cardName} cannot be located in the wiki, please inform the admin");
      else
        return response.Content!;
    }
  }
}