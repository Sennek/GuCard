using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    public GameObject[] allCards;

    private int startingCardAmount = 10;
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
        allCards = Resources.LoadAll<GameObject>("Prefabs/Cards");

        players = new PlayerGameInstance[2];
        players[0] = new PlayerGameInstance("Sennek");
        players[1] = new PlayerGameInstance("AI");

        StartCoroutine(GameStages());
    }
    #region Game Stages
    private IEnumerator GameStages()
    {
        yield return StartCoroutine(DeckStartingAnimation());
        yield return StartCoroutine(ShuffleCards());

        while (currentStage != Stage.GameOver)
        {
            foreach (PlayerGameInstance player in players)
            {
                yield return StartCoroutine(PlayerTurn(player));
                yield return StartCoroutine(Manager.DealCardsToCommonHand(1));
            }
        }
    }

    private IEnumerator PlayerTurn(PlayerGameInstance player)
    {
        currentPlayer = player;
        int playerIndex = GetPlayerNo(currentPlayer);

        if (playerIndex == 0)
        {
            SetPlayerReadingState(true);
            while (currentStage == Stage.PlayerOneTurn)
            {
                yield return null;
            }

            SetPlayerReadingState(false);
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
        yield return StartCoroutine(Manager.DealCards(players[0], startingCardAmount));
        yield return StartCoroutine(Manager.DealCards(players[1], startingCardAmount));
        yield return StartCoroutine(Manager.DealCardsToCommonHand(startingCardAmount - 2));
        yield return new WaitForSeconds(1);
        currentStage = Stage.PlayerOneTurn;
    }
    public IEnumerator PickCards(Card card1, Card card2)
    {
        int playerIndex = GetPlayerNo(currentPlayer);

        float targetX = Random.Range(-2, 2);
        float targetY = Random.Range(-2, 2);

        foreach (Card card in new Card[2] { card1, card2 })
        {
            card.SetCardState(0);
            card.transform.parent = playerUsedDeck[playerIndex];
            card.transform.SetAsFirstSibling();

            card.gameObject.LeanMoveLocal(new Vector3(targetX, targetY), 1);
        }
        yield return new WaitForSeconds(1);
        currentStage = Stage.AddCards;
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
    #endregion
    #region MiscMethods
    public int GetPlayerNo(PlayerGameInstance player)
    {
        return Array.FindIndex(players, x => x == player);
    }
    public void HighlightCommonCards(CardType type, int state)
    {
        foreach (Card card in CardsInCommonDeck)
        {
            if (card.cardType == type)
            {
                card.SetCardState(state);
            }
        }
    }
    public void DehighlightCommonCards()
    {
        foreach (Card card in CardsInCommonDeck)
        {
            card.SetCardState(0);
        }
    }
    public void SetPlayerReadingState(bool active)
    {
        foreach (Card card in playerHand[0].GetComponentsInChildren<Card>())
        {
            card.SetInteractable(active);
        }
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
}

public static class Manager
{
    public static IEnumerator DealCards(PlayerGameInstance player, int amount)
    {
        int playerIndex = GameManager.Instance.GetPlayerNo(player);

        for (int i = 0; i < amount; i++)
        {
            GameObject card = GameObject.Instantiate(GameManager.Instance.allCards.RandomOrDefault(), GameManager.Instance.commonDeck);
            Card cardScript = card.GetComponent<Card>();


            card.transform.parent = GameManager.Instance.playerHand[playerIndex];
            card.transform.SetAsFirstSibling();
            card.LeanScale(new Vector3(0.8f, 0.8f, 0.8f), 0);

            cardScript.SetCardState(playerIndex == 0 ? 0 : -1);

            float targetX = card.GetComponent<RectTransform>().sizeDelta.x / 2 * card.transform.parent.childCount * card.transform.localScale.x;
            float targetY = Random.Range(-5, 5);

            LeanTween.moveLocal(card, new Vector3(targetX, targetY), 0.15f).setOnComplete(() => { cardScript.Init(); });
            yield return new WaitForSeconds(0.15f);
        }
    }
    public static IEnumerator DealCardsToCommonHand(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject card = GameObject.Instantiate(GameManager.Instance.allCards.RandomOrDefault(), GameManager.Instance.commonDeck);
            Card cardScript = card.GetComponent<Card>();

            card.transform.parent = GameManager.Instance.commonHand;
            card.transform.SetAsFirstSibling();
            card.LeanScale(new Vector3(0.8f, 0.8f, 0.8f), 0);

            card.transform.LeanRotateZ(Random.Range(-5, 5), 0);

            cardScript.SetCardState(0);

            float targetX = card.GetComponent<RectTransform>().sizeDelta.x / 2 * card.transform.parent.childCount * card.transform.localScale.x;
            float targetY = Random.Range(-5, 5);

            LeanTween.moveLocal(card, new Vector3(targetX, targetY), 0.15f);
            yield return new WaitForSeconds(0.15f);
        }
    }
    public static IEnumerator ResetCardsInHand(RectTransform hand)
    {
        for (int i = 0; i < hand.transform.childCount; i++)
        {
            GameObject card = hand.GetChild(i).gameObject;
            card.transform.parent = GameManager.Instance.commonHand;
            card.transform.SetAsFirstSibling();

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
