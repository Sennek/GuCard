using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
[Serializable]
public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("0=inactive; 1=highlighted; 2=selected")]
    public Sprite[] cardSprites;
    public Sprite cardSpriteBack;
    public string cardName;
    [TextArea] public string cardDescription;
    public int cardPoints;
    public CardType cardType;
    public bool isRare;
    public bool isSelected;
    public bool isTargetable;
    public bool isInteractable;
    public bool blockOnMouseOver;

    private Image image;
    private Vector3 originalPos;


    [HideInInspector] public int cardState;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void InitLocalPos()
    {
        originalPos = transform.localPosition;
    }
    public void SetCardState(int state)
    {
        cardState = state;
        if (state < 0)
        {
            image.sprite = cardSpriteBack;
            isInteractable = false;
            return;
        }

        image.sprite = cardSprites[state];
        image.SetNativeSize();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (!selected)
            GameManager.Instance.DehighlightCommonCards();

        SetCardState(selected ? 2 : 0);
        GameManager.Instance.selectedCard = selected ? this : null;
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (isInteractable)
        {
            if (data.button == PointerEventData.InputButton.Left)
            {
                SoundManager.Play(SoundManager.Instance.cardSelect);

                Card[] matchingCards = GameManager.Instance.CardsInCommonDeck.Where(x => x.cardType == cardType).ToArray();

                if (matchingCards.Length == 0)
                {
                    GameManager.Instance.NoMatchingCardsLeft(this);
                    return;
                }

                if (!isSelected)
                {
                    GameManager.Instance.CurrentCardSet(this);
                    GameManager.Instance.HighlightCommonCards(cardType, 2);
                }
                else
                {
                    GameManager.Instance.CurrentCardDeselect();
                    GameManager.Instance.DehighlightCommonCards();
                }
            }
        }
        else if (isTargetable)
        {
            if (data.button == PointerEventData.InputButton.Left && cardType == GameManager.Instance.selectedCard.cardType)
            {
                StartCoroutine(GameManager.Instance.PickCards(GameManager.Instance.selectedCard, this));
            }
        }
    }
    public void OnPointerEnter(PointerEventData data)
    {
        if (isInteractable && !blockOnMouseOver)
        {
            GameManager.Instance.ShowCardName(this);
            GameManager.Instance.HighlightCommonCards(cardType, 1);

            LeanTween.cancel(gameObject);
            LeanTween.moveLocalY(gameObject, 30, 0.2f);
        }
    }
    public void OnPointerExit(PointerEventData data)
    {
        if (isInteractable && !blockOnMouseOver && !isSelected)
        {
            GameManager.Instance.ShowCardName(null);

            GameManager.Instance.DehighlightCommonCards(1);

            LeanTween.cancel(gameObject);
            LeanTween.moveLocalY(gameObject, originalPos.y, 0.2f);
        }
    }
}