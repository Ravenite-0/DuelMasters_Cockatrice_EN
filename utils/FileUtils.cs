using System.Xml.Linq;
using Cards;
using static Utils.RestUtils;

namespace Utils
{
  public class FileUtils
  {
    public static void ExportCards(List<Card> cards)
    {

      // Master XML file
      var debugXML = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
      <cockatrice_carddatabase version =""4"">
        <sets>
          <set>
          <name>{cards.First().exactSet!.Value.set}</name>
            <longname>{cards.First().exactSet!.Value.set}</longname>
            <settype>{cards.First().exactSet!.Value.set}</settype>
            <releasedate>{DateTime.Today.ToString("yyyy-MM-dd")}</releasedate>
          </set>
        </sets>
      <cards>
          {string.Join("", cards.Select(card =>
      {
        // DownloadCardImage(card);
        return card.GenerateCockatriceCardData();
      }))}
        </cards>
        </cockatrice_carddatabase>";

      //Export XML
      File.WriteAllText($"{Directory.GetCurrentDirectory()}\\\\data\\\\customsets\\\\{cards.First().exactSet!.Value.set}.xml", XDocument.Parse(debugXML).ToString());
      //TODO - fix
      //       //Master XML file
      //       var debugXML = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
      // <cockatrice_carddatabase version =""4"">
      //   <sets>
      //     <set>
      //     <name>{cards[0].exactSet!.Value.set}</name>
      //       <longname></longname>
      //       <settype>{StringUtils.RemoveNumericCharacters(cards[0].exactSet!.Value.set)}</settype>
      //       <releasedate>{DateTime.Today.ToString("yyyy-MM-dd")}</releasedate>
      //     </set>
      //   </sets>
      // <cards>
      //     {string.Join("", cards.Select(card => card.GenerateCockatriceCardData()))}
      //   </cards>
      //   </cockatrice_carddatabase>";

      //       //Export XML
      //       File.WriteAllText($"{Directory.GetCurrentDirectory()}\\\\data\\\\customsets\\\\MAIN.xml", XDocument.Parse(debugXML).ToString());
    }

    public static List<Card> ExpandCardsBySet(List<Card> cards)
    {
      var test = cards.SelectMany(card =>
      {
        List<Card> newCards = new List<Card>();
        if (card.sets.Count > 1)
        {
          newCards.AddRange(card.sets.Select(set => new Card(card, set)));
        }
        return newCards;
      });
      return test.ToList();
    }
  }
}