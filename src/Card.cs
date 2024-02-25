using System.Net;
using static Utils.StringUtils;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework.Internal;
using System.Threading.Tasks.Dataflow;

namespace Cards
{
  public class Card
  {
    public string EN_CardName;
    public string JP_CardName;
    public string effects;
    public string cardType;
    public string race;
    public string mana;
    public string civilization;
    public string power;
    public List<(string set, string setnum)> sets;
    public (string set, string setnum)? exactSet;
    public string altForm;

    public Card(string cardName, List<string> cardDetails)
    {
      //Converting card data into key-value pairs.
      //Due to the original data containing duplicate keys, lookup will be used instead of dictionary (See card Faerie Re:Life for more details).
      ILookup<string, string> cardData = ToLookup(RemoveUnwantedCardData(cardDetails));

      //Constructs the card object.
      EN_CardName = FormatCardXML(cardName);
      JP_CardName = GenerateOCGCardName(cardData);
      cardType = GenerateCardType(cardData);
      race = GenerateCardRaces(cardData);
      civilization = GenerateCardCivs(cardData);
      mana = cardData["cost"].FirstOrDefault() ?? "";
      power = cardData["power"].FirstOrDefault() ?? "";
      effects = GenerateCardEffects(cardData);
      sets = GenerateCardSets(cardData);
      altForm = "";
    }

    public Card(Card card, (string set, string setnum) set)
    {
      EN_CardName = card.EN_CardName;
      JP_CardName = card.JP_CardName;
      cardType = card.cardType;
      race = card.race;
      civilization = card.civilization;
      mana = card.mana;
      power = card.power;
      effects = card.effects;
      sets = card.sets;
      altForm = "";
      exactSet = set;
    }

    //First stage of processing card data by removing unwanted details.
    protected List<string> RemoveUnwantedCardData(List<string> cardDetails)
    {
      cardDetails.RemoveAt(0);
      cardDetails[cardDetails.Count() - 1] = SeparateCardData(cardDetails.LastOrDefault()!)[0];
      cardDetails.RemoveAll(card => !card.Contains("=") || card.Contains("flavor"));
      return cardDetails;
    }

    //Second stage of processing card data by converting them into key value pair
    protected ILookup<string, string> ToLookup(List<string> cardDetails)
    {

      List<string> keys = cardDetails.Select(detail => detail.Split("=", 2)[0]).ToList();
      List<string> values = cardDetails.Select(detail =>
        detail.Split("=", 2).Length < 2 ?
          detail.Split("  ", 2)[1] :
          detail.Split("=", 2)[1]
      ).ToList();

      return keys.Zip(values, (key, value) => new { key = key, value = value })
        .ToLookup(kvp => kvp.key.Trim(), kvp => kvp.value.Trim());
    }

    //Extracting and formatting OCG card names.
    protected string GenerateOCGCardName(ILookup<string, string> cardData) => String.Join(" / ", cardData
      .Where(data => data.Key.Contains("ocgname"))
      .Select(JPName =>
        Regex.Replace(JPName.FirstOrDefault()!, @"(?<={{)(.*?)(?=}})", (furigana) =>
        {
          var furiganas = furigana.Value.Split("|");
          return furiganas.Count() > 1 ? furiganas[1] : furiganas[0];
        }))
      .Select(name => FormatCardXML(name.Replace("{", "").Replace("}", "").Replace(" / ", "/"))));

    //Formatting card types (Mostly for twinpacts).
    protected string GenerateCardType(ILookup<string, string> cardData)
    {
      var types = cardData.Where(data => data.Key.Contains("type")).Select(type => type.FirstOrDefault()).ToList();
      return (types.Count < 1) ? "" : (types.Count > 1 ? $"Twinpact" : types[0]) ?? "";
    }

    //Formatting card races (For multiple races and twinpacts).
    protected string GenerateCardRaces(ILookup<string, string> cardData)
    {
      var races = cardData.Where(data => data.Key.Contains("race")).Select(race => cardData[race.Key].FirstOrDefault()).ToList();
      return (races.Count() > 0 ? (races.Count() > 1 ? String.Join("/", races) : races.FirstOrDefault()) : "") ?? "";
    }

    //Formatting card civilizations.
    protected string GenerateCardCivs(ILookup<string, string> cardData)
    {
      var civilizations = cardData.Where(data => data.Key.Contains("civilization")).Select(civilization => cardData[civilization.Key].FirstOrDefault());
      return (civilizations.Count() > 0 ? (civilizations.Count() > 1 ? String.Join("/", civilizations) : civilizations.FirstOrDefault()) : "Colorless") ?? "No race should not happen";
    }

    //Formatting card effects.
    protected string GenerateCardEffects(ILookup<string, string> cardData) =>
      //Check if card is vanilla.
      cardData["effect"].FirstOrDefault() != null ?
        //Check if card is twinpact
        (cardData["effect2"].FirstOrDefault() != null ?
          FormatCardEffect(cardData["effect"].FirstOrDefault()!) +
            TWINPACT_LINEBREAK +
            FormatCardEffect(cardData["effect2"].FirstOrDefault()!) :
          FormatCardEffect(cardData["effect"].FirstOrDefault()!)

      ) : "";

    //Formatting all the sets a card was printed in.
    //TODO - implement Promotional if possible.
    protected List<(string set, string setnum)> GenerateCardSets(ILookup<string, string> cardData)
    {
      List<(string set, string setnum)> cardSet = new List<(string set, string setnum)>();

      var debug = cardData.Where(data => data.Key.Contains("set"));
      List<string> set = new List<string>();
      List<string> setnum = new List<string>();

      foreach (IGrouping<string, string> data in cardData)
      {
        if (data.Key.StartsWith("set") && !data.Key.Contains("setnum") && data.FirstOrDefault()!.StartsWith("DM"))
        {
          //Skips TCG sets
          if (TCG_SETS.Contains(data.FirstOrDefault()!.Split(" ")[0]) && !data.FirstOrDefault()!.EndsWith("(OCG)"))
            continue;
          var setid = RemoveNumericCharacters(data.Key).Last();
          var setnumData = cardData[$"setnum{setid}"].FirstOrDefault();
          if (setnumData != null)
          {
            set.Add(data.FirstOrDefault()!);
            setnum.Add(setnumData);
          }
        }
      }
      var setDictionary = set.Zip(setnum, (key, value) => new { key = key, value = value })
        .ToLookup(kvp => kvp.key.Trim(), kvp => kvp.value.Trim());

      foreach (var kvp in setDictionary)
      {
        string setID = kvp.Key.Split(" ")[0];
        foreach (var setSeries in kvp.SelectMany(value => value.Split(",")).Select(setNumber => setNumber.Split("/")[0]))
        {
          cardSet.Add((setID, setSeries));
        }
      }
      return cardSet;
    }

    public string GenerateCockatriceCardData()
    {
      Random rnd = new Random();
      return $@"
<card>
  <name>{EN_CardName}</name>
  <text>
{effects}
  </text>
  <prop>
    <maintype>{cardType}</maintype>
    <type>{race}</type>
    <manacost>{mana}</manacost>
    <colors>{civilization}</colors>
    <pt>{power}</pt>
  </prop>
  <set num=""{exactSet!.Value.set}-{exactSet!.Value.setnum}"">{exactSet!.Value.set}</set>
  <related>{altForm}</related>
  <tablerow>2</tablerow>
</card>";
      // //Convert object into XML data - Had to be done manually due to the complexity atm
      // XElement test = new XElement("card",
      //   new XElement("name", EN_CardName),
      //   new XElement("text", effects),
      //   new XElement("prop",
      //     new XElement("maintype", cardType),
      //     new XElement("type", race),
      //     new XElement("manacost", mana),
      //     new XElement("colors", civilization),
      //     new XElement("pt", power)
      //   ),
      //   new XElement("set", new XAttribute("num", rnd.Next(99999999).ToString("00000000")), "MAIN"),
      //   new XElement("related", altForm),
      //   new XElement("tablerow", 2)
      // );
    }
  }
}