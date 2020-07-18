using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEditorInternal;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public static GameManager Instance;

    public PlayerGameInstance[] players;
    public PlayerGameInstance currentPlayer;
    public RectTransform[] playerHand;
    public RectTransform[] playerRareDeck;
    public RectTransform[] playerUsedDeck;
    public RectTransform commonDeck;
    public RectTransform commonHand;
    public RectTransform cardNameContainer;
    public RectTransform gameOverScreen;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardComboText;
    public TextMeshProUGUI[] playerPts;
    public GameObject[] allCards;
    public Queue<GameObject> currentCards;



    private readonly int startingCardAmount = 10;
    public Card selectedCard;
    #region Properties
    public Card[] CardsInCommonDeck
    {
        get
        {
            return commonHand.GetComponentsInChildren<Card>();
        }
    }
    #endregion
    private Stage currentStage;
    private enum Stage { PlayerOneTurn, AddCards, PlayerTwoTurn, GameOver };
    private void Start()
    {
        Instance = this;
        allCards = Resources.LoadAll<GameObject>("Prefabs/Cards").Where(x => x.GetComponent<Card>().isRare == false).ToArray();
        allCards.Shuffle();
        currentCards = new Queue<GameObject>(allCards);

        players = new PlayerGameInstance[2];
        players[0] = new PlayerGameInstance("Sennek");
        players[1] = new PlayerGameInstance("AI");

        playerPts[0].text = playerPts[1].text = "0";


        StartCoroutine(GameStages());
    }
    #region Game Stages
    private IEnumerator GameStages()
    {
        yield return StartCoroutine(DeckStartingAnimation());
        yield return StartCoroutine(ShuffleCards());

        while (playerHand[0].childCount != 0 && playerHand[0].childCount != 0)
        {
            foreach (PlayerGameInstance player in players)
            {
                yield return StartCoroutine(PlayerTurn(player));
                yield return StartCoroutine(Manager.DealCards(commonHand, 1));
            }
        }
        yield return StartCoroutine(GameOver());
    }
    private IEnumerator PlayerTurn(PlayerGameInstance player)
    {

        currentStage = Stage.PlayerOneTurn;

        currentPlayer = player;
        int playerIndex = GetPlayerIndex(currentPlayer);

        if (playerIndex == 0)
        {
            SetPlayerReadingState(true);

            while (currentStage == Stage.PlayerOneTurn)
            {
                yield return null;
            }

            SetPlayerReadingState(false);
        }
        else
        {
            yield return StartCoroutine(AIPlayTurn());
        }

        currentStage = Stage.PlayerTwoTurn;
    }
    private IEnumerator AIPlayTurn()
    {
        yield return new WaitForSeconds(Random.Range(1, 2));
        Card[] commonHand = CardsInCommonDeck;
        foreach (Card card in playerHand[1].GetComponentsInChildren<Card>())
        {
            int targetIndex = Array.FindIndex(commonHand, x => x.cardType == card.cardType);

            if (targetIndex >= 0)
            {
                yield return StartCoroutine(PickCards(card, commonHand[targetIndex]));
                break;
            }
        }
    }
    private IEnumerator ShuffleCards()
    {
        yield return StartCoroutine(Manager.DealCards(playerHand[0], startingCardAmount));
        yield return StartCoroutine(Manager.DealCards(playerHand[1], startingCardAmount));
        yield return StartCoroutine(Manager.DealCards(commonHand, 8));
        yield return new WaitForSeconds(1);
        currentStage = Stage.PlayerOneTurn;
    }
    public IEnumerator PickCards(Card card1, Card card2)
    {
        DehighlightCommonCards();

        card1.blockOnMouseOver = true;
        card2.blockOnMouseOver = true;

        int playerIndex = GetPlayerIndex(currentPlayer);

        float targetX = Random.Range(-5, 5);
        float targetY = Random.Range(-5, 5);

        foreach (Card card in new Card[2] { card1, card2 })
        {
            card.SetCardState(0);
            card.transform.parent = playerUsedDeck[playerIndex];
            card.transform.SetAsFirstSibling();

            card.gameObject.LeanMoveLocal(new Vector3(targetX, targetY), 0.2f);
        }

        StartCoroutine(Manager.ResetCardsInHand(playerHand[GetPlayerIndex(currentPlayer)]));
        StartCoroutine(Manager.ResetCardsInHand(commonHand));
        AddCurrentPlayerPts(card1.cardPoints + card2.cardPoints);

        yield return new WaitForSeconds(0.4f);
        currentStage = Stage.AddCards;
    }
    public IEnumerator GameOver()
    {
        PlayerGameInstance playerWon = players[0].points > players[1].points ? players[0] : players[1];
        LeanTween.scale(gameOverScreen, new Vector3(4, 4), 0.3f).setOnComplete(() => { gameOverText.SetText($"{playerWon.playerName} Won!"); });
        yield return new WaitForSeconds(0.3f);
    }
    #endregion
    #region UI animations
    private IEnumerator DeckStartingAnimation()
    {
        LeanTween.moveLocalX(playerRareDeck[0].gameObject, 910, 0.5f);
        LeanTween.moveLocalX(playerRareDeck[1].gameObject, 910, 0.5f);
        LeanTween.moveLocalX(commonDeck.gameObject, 350, 0.5f);

        yield return new WaitForSeconds(1f);
    }

    public void ShowCardName(Card card)
    {
        if (card != null)
        {
            cardNameText.SetText($"{card.cardName} {card.cardPoints}pts");
            //cardNameText.SetText(card.cardName); show all cards that can be combined with it
            LeanTween.cancel(cardNameContainer);
            LeanTween.value(1150, 800, 0.5f).setOnUpdate((float val) => { cardNameContainer.localPosition = new Vector3(val, cardNameContainer.localPosition.y); });
        }
        else
        {
            LeanTween.cancel(cardNameContainer);
            LeanTween.value(800, 1150, 0.5f).setOnUpdate((float val) => { cardNameContainer.localPosition = new Vector3(val, cardNameContainer.localPosition.y); });
        }
    }
    #endregion
    #region MiscMethods
    private void AddCurrentPlayerPts(int points)
    {
        currentPlayer.points += points;
        playerPts[GetPlayerIndex(currentPlayer)].SetText($"{currentPlayer.points}");
    }
    public int GetPlayerIndex(PlayerGameInstance player)
    {
        return Array.FindIndex(players, x => x == player);
    }
    public void HighlightCommonCards(CardType type, int state)
    {
        foreach (Card card in CardsInCommonDeck)
        {
            if (card.cardType == type && card.cardState < 2)
            {
                card.SetCardState(state);
            }
        }
    }
    public void DehighlightCommonCards(int maxState = 2)
    {
        foreach (Card card in CardsInCommonDeck)
        {
            if (card.cardState <= maxState)
            {
                card.SetCardState(0);
            }
        }
    }
    public void SetPlayerReadingState(bool active)
    {
        foreach (Card card in playerHand[0].GetComponentsInChildren<Card>())
        {
            card.SetInteractable(active);
        }
    }
    public void CurrentCardSet(Card card)
    {
        selectedCard?.SetSelected(false);
        card.SetSelected(true);
    }
    public void CurrentCardDeselect()
    {
        selectedCard.SetSelected(false);
    }
    #endregion
}

public static class HelperFunc
{
    public static T RandomOrDefault<T>(this IList<T> source)
    {
        if (source.Count == 0)
            return default;

        int randomInt = Random.Range(0, source.Count);
        return source[randomInt];
    }
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public static class Manager
{
    public static IEnumerator DealCards(RectTransform target, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject card = GameObject.Instantiate(GameManager.Instance.currentCards.Dequeue(), GameManager.Instance.commonDeck);
            Card cardScript = card.GetComponent<Card>();


            card.transform.parent = target;
            card.transform.SetAsFirstSibling();
            card.LeanScale(new Vector3(0.8f, 0.8f, 0.8f), 0);

            cardScript.SetCardState(target.name == "OpponentHand" ? -1 : 0);

            if (target.name == "CommonHand")
            {
                cardScript.SetTargetable(true);
            }

            float targetX = card.GetComponent<RectTransform>().sizeDelta.x / 2 * card.transform.parent.childCount * card.transform.localScale.x;
            float targetY = Random.Range(-5, 5);

            LeanTween.moveLocal(card, new Vector3(targetX, targetY), 0.15f).setOnComplete(() => { cardScript.Init(); });
            yield return new WaitForSeconds(0.15f);
        }
    }
    public static IEnumerator ResetCardsInHand(RectTransform hand)
    {
        for (int i = 0; i < hand.transform.childCount; i++)
        {
            GameObject card = hand.GetChild(i).gameObject;

            float targetX = card.GetComponent<RectTransform>().sizeDelta.x / 2 * i;

            LeanTween.moveLocalX(card, targetX, 0);
        }
        yield return new WaitForEndOfFrame();
    }
}
public class Player
{
    public string playerName = "Player";

    public int losses;
    public int wins;

    public List<Card> rareCards;

}
public class PlayerGameInstance
{
    public string playerName;
    public List<Card> rareCards;
    public int points;

    public PlayerGameInstance(Player source)
    {
        playerName = source.playerName;
        rareCards = new List<Card>();
        rareCards.AddRange(source.rareCards);
    }
    public PlayerGameInstance(string name)
    {
        playerName = name;
        rareCards = new List<Card>();
    }
    public override int GetHashCode()
    {
        return playerName.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        return GetHashCode() == ((Player)obj).GetHashCode();
    }
}
public enum CardType
{
    Spring,
    Summer,
    Autumn,
    Winter
}
