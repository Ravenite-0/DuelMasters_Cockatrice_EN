using Cards;
using NUnit.Framework;
using static Utils.StringUtils;
using static Utils.RestUtils;
using System.Xml.Linq;


namespace Tests
{
  public abstract class UnitTest
  {
    [Test]
    public static void DebugCardByName()
    {
      string cardName = "Überdragon Jabaha";
      Card card = GenerateCard(cardName, ImportCardByName(cardName));
      return;
    }

    [Test]
    public static void ReadXMLFile()
    {
      XDocument file = XDocument.Load(FILEPATH_DATA_SETS("MAIN.xml"));
      var testa = file.Descendants("cards").Elements().Select(card => card.Element("name")!.Value);
      return;

    }
  }
}