using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cards;
using static System.IO.Directory;

namespace Utils
{
  public abstract class StringUtils
  {
    #region Common variables
    public static string TWINPACT_LINEBREAK = Environment.NewLine + "=========================" + Environment.NewLine;
    //Endpoints
    public const string URL_QUERY_CARDS = "https://duelmasters.fandom.com/api.php?action=query&list=embeddedin&eititle=Template:Cardtable&format=json&eilimit=max";
    public const string URL_QUERY_SINGLE = "https://duelmasters.fandom.com/api.php?action=query&prop=revisions&rvprop=content&rvslots=main&format=php&titles=";
    public const string URL_QUERY_IMAGE = "https://dm.takaratomy.co.jp/wp-content/card/cardimage/";

    //Filepaths
    public static string FILEPATH_DOCS(string fileName) => $"{GetCurrentDirectory()}\\docs\\{fileName}";
    public static string FILEPATH_DATA_SETS(string setName) => $"{GetCurrentDirectory()}\\data\\customsets\\{setName}";
    public static string FILEPATH_DATA_PICS(string setName) => $"{GetCurrentDirectory()}\\data\\pics\\downloadedPics\\{setName}";
    public static string CARDS_DATABASE => $"{GetCurrentDirectory()}\\data\\All_Cards.csv";

    //A specific filter to exclude TCG sets during image import because there are no official URL.
    public static string[] TCG_SETS = { "DM-01", "DM-02", "DM-03", "DM-04", "DM-05", "DM-06", "DM-07", "DM-08", "DM-09", "DM-10", "DM-11", "DM-12" };
    #endregion

    #region String Formatting
    //Get rid of any uni-codes or symbols not accepted by REST.
    public static string FormatCardREST(string cardName) => Regex.Unescape(cardName
      .Replace("\\\"", "\"")
      .Replace("=", "%3D")
      .Replace("&", "%26")
      .Replace("\\ufe0e", "%EF%B8%8E")
      .Replace("\\ud83d\\udca2", "💢")).Trim();

    //Get rid of any characters not suitable for XMl export.
    public static string FormatCardXML(string cardName) => cardName
      .Replace('_', ' ')
      .Replace("&", "and")
      .Replace("<br>", "")
      .Replace("<ref>", "")
      .Replace(" / ", "/")
       .Replace("·", " ");

    //A huge reformat to make effect strings look better in Cockatrice.
    public static string FormatCardEffect(string effect) => String.Join(Environment.NewLine, effect.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "")
      .Split("\n\n").Select(line =>
        {
          string newLine = line.Split("|")[0];
          if (!newLine.StartsWith("■"))
            newLine = $"■ {newLine}";
          else
            newLine.Replace("■ ", "■");
          return FormatCardXML(newLine);
        }).ToList());

    public static List<string> SeparateCardData(string cardData) => cardData.Split("\n|").ToList();

    public static List<string> RemoveNumericCharacters(string text) => Regex.Matches(text, @"\D+|\d+").Cast<Match>().Select(m => m.Value).ToList();
    public static List<string> KeepNumericCharacters(string text) => Regex.Matches(text, @"\d+").Cast<Match>().Select(m => m.Value).ToList();

    public static string FormatCardSetImageURI((string set, string setnum) exactSet)
    {
      string set = exactSet.set.Replace("DM", "");
      string setnum = exactSet.setnum;

      return URL_QUERY_IMAGE.ToString() + $"{set}-{setnum}";
    }

    #endregion

    #region Card related
    //TODO
    //Converts the endpoint response into a list of card names and an identifer that tells you if there's more cards (Each request is capped at 500).
    public static (string nextPageIdentifier, List<string> cards) CompileCardNames(string cardlist)
    {
      string[] datasets = cardlist.Split("},\"");

      //Grabs all cards first
      var cards = Regex.Matches(datasets.Last(), @"(?<=title\"":\"")(.*?)(?=\""})").Cast<Match>().Select(match => match.Value.Replace(' ', '_')).ToList();

      //Determines if search has hit the last page whilst returning data
      if (datasets.Length == 2)
        return ("LAST_PAGE", cards);
      else if (datasets.Length == 3)
        return (new Regex(@"(?<=eicontinue\"":\"").*(?=\"",\"")").Match(datasets[0]).Value, cards);
      else
        throw new Exception($"An invalid JSON has been picked up: {cardlist}");
    }

    public static Cards.Card GenerateCard(string cardName, string cardData)
    {
      return new Cards.Card(cardName, cardData.Split("\n|").Select(data => data.Trim()).ToList());
    }
    #endregion
  }
}