﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

[Serializable]
public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("0=inactive; 1=highlighted; 2=selected")]
    public Sprite[] cardSprites;
    public Sprite cardSpriteBack;
    public string cardName;
    public string cardDescription;
    public int cardPoints;
    public CardType cardType;
    public bool isRare;
    [SerializeField] private bool isSelected;
    [SerializeField] private bool isTargetable;
    private Image image;
    private Vector3 originalPos;
    private LTDescr movementAnimation;

    public bool isInteractable;
    public bool blockOnMouseOver;
    [HideInInspector] public int cardState;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void InitLocalPost()
    {
        originalPos = transform.localPosition;
    }
    public void SetCardState(int state)
    {
        cardState = state;
        if (state < 0)
        {
            image.sprite = cardSpriteBack;
            SetInteractable(false);
            return;
        }

        image.sprite = cardSprites[state];
        image.SetNativeSize();
    }
    public void SetInteractable(bool interactable)

    {
        if (interactable == isInteractable)
            return;

        isInteractable = interactable;
    }
    public void SetTargetable(bool targetable)
    { isTargetable = targetable; }
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (!selected)
        {
            GameManager.Instance.DehighlightCommonCards();
        }

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
            if (data.button == PointerEventData.InputButton.Left)
            {
                if (cardType == GameManager.Instance.selectedCard.cardType)
                {
                    StartCoroutine(GameManager.Instance.PickCards(GameManager.Instance.selectedCard, this));
                }
            }
        }
    }
    public void OnPointerEnter(PointerEventData data)
    {
        if (isInteractable && !blockOnMouseOver)
        {
            GameManager.Instance.ShowCardName(this);
            if (movementAnimation != null)
            {
                LeanTween.cancel(gameObject);
            }
            GameManager.Instance.HighlightCommonCards(cardType, 1);
            movementAnimation = LeanTween.moveLocalY(gameObject, 30, 0.2f);
        }
    }
    public void OnPointerExit(PointerEventData data)
    {
        if (isInteractable && !blockOnMouseOver && !isSelected)
        {
            GameManager.Instance.ShowCardName(null);

            if (movementAnimation != null)
            {
                LeanTween.cancel(gameObject);
            }
            GameManager.Instance.DehighlightCommonCards(1);
            movementAnimation = LeanTween.moveLocalY(gameObject, originalPos.y, 0.2f);
        }
    }
}