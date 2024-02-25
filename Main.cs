using static Utils.StringUtils;
using static Utils.RestUtils;
using static Utils.FileUtils;
using Cards;
using CsvHelper;
using static System.Globalization.CultureInfo;

namespace Main
{
  class Program
  {
    //Everything starts here.
    static void Main(string[] args)
    {
      //Temp tests
      // DownloadRestImage("https://dm.takaratomy.co.jp/wp-content/card/cardimage/");

      // string cardName = "Colorless_Rainbow,_Zenith_of_\"Color_Disaster\"_/_Zenith_Hazard";
      // string cardName = "Terror_Pit";
      // Card testcard = GenerateCard(cardName, ImportCardByName(cardName));
      // var cardList = ExpandCardsBySet(new Card[] { testcard }.ToList());
      // File.WriteAllText(FILEPATH_DOCS("debugCard.txt"), card.GenerateCockatriceCardData());
      // foreach (var cd in cardList)
      // {
      //   DownloadCardImage(cd);
      // }
      // var test = cardList
      //     .GroupBy(card => card.exactSet!.Value.set)
      //     .Select(card => card.ToList());
      // foreach (var set in test)
      // {
      //   foreach (var cd in set)
      //   {
      //     
      //   }
      //   ExportCards(set);
      // }

      // ExportCards(Cardlist.GetAllCards()
      //  //Formats and standardizes all imported card data.
      //  .Select(card => GenerateCard(card, ImportCardByName(card))).ToList());


      //TODO - fix all image downloads
      //Retrieve all card data from API

      List<List<Card>> fullData;

      fullData = ExpandCardsBySet(Cardlist.GetAllCards()
        //Formats and standardizes all imported card data.
        .Select(card => GenerateCard(card, ImportCardByName(card))).ToList())
        //Categorizes each card to their respective sets
        .GroupBy(card => card.exactSet!.Value.set).Select(card => card.ToList())
        .Select(set => set.ToList()).ToList();


      using (var writer = new StreamWriter(CARDS_DATABASE))
      using (var csv = new CsvWriter(writer, InvariantCulture))
      {
        foreach (var set in fullData)
        {
          List<CSVCard> CSVSet = set.Select(card => new CSVCard(card)).ToList();
          csv.WriteRecords(CSVSet);
          ExportCards(set);
        }
      }
      return;
    }
  }
}