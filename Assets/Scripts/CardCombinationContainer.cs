using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardCombinationContainer : MonoBehaviour
{
    public static CardCombinationContainer cardCombo;
    public CardCombination[] cardCombinations;

    private void Start()
    {
        cardCombo = this;
    }

    public CardCombination[] GetCombinationsWithCard(Card card)
    {
        return cardCombinations.Where(x => x.CombinationCards.Contains(card.name)).ToArray();
    }

}

[Serializable]
public class CardCombination
{
    public string CombinationName;
    public string[] CombinationCards;
    public int CombinationPoints;
    public bool hasBeenUnlockedOnce;
}