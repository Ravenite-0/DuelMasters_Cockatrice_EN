using static Utils.RestUtils;
using static Utils.StringUtils;
using System.Text.RegularExpressions;

namespace Cards
{
  public class Cardlist
  {
    //Retrieve the full list of cards in the Fandom via API.
    public static List<string> GetAllCards()
    {
      string nextPageIdentifier = "FIRST_PAGE";
      List<string> cards = new List<string>();
      do
      {
        //Because each request can only pull a maximum of 500 cards, it had to loop through the pages
        var result = CompileCardNames(ImportOCGCards(nextPageIdentifier));
        if (cards.Count() < 1)
          cards.AddRange(result.cards);
        else
        {
          List<string> newCards = cards.Union(result.cards).ToList();
          cards = newCards;
        }
        nextPageIdentifier = result.nextPageIdentifier;
      } while (nextPageIdentifier != "LAST_PAGE");

      return cards.Select(card => FormatCardREST(card)).ToList();
    }
  }
}