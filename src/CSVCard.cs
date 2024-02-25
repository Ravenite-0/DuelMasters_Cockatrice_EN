using static Utils.StringUtils;

namespace Cards
{
  public class CSVCard
  {
    public string CardName { get; set; }
    public string CardEffect { get; set; }
    public string CardType { get; set; }
    public string CardRace { get; set; }
    public string Mana { get; set; }
    public string Civilization { get; set; }
    public string Power { get; set; }
    public string Set { get; set; }
    public string SetNum { get; set; }
    public string ImageURL { get; set; }

    public CSVCard(Card card)
    {
      CardName = card.EN_CardName;
      CardEffect = card.effects;
      CardType = card.cardType;
      CardRace = card.race;
      Mana = card.mana;
      Civilization = card.civilization;
      Power = card.power;
      Set = card.exactSet!.Value.set;
      SetNum = card.exactSet!.Value.setnum;
      ImageURL = FormatCardSetImageURI(((string set, string setnum))card.exactSet);
    }
  }
}