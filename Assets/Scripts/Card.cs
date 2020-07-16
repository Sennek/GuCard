using System;
using System.Collections;
using System.Collections.Generic;
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
    public CardCombination[] cardCombination;
    private Image image;
    public bool isRare;
    [SerializeField] private bool isSelected;
    [SerializeField] private bool isTargetable;
    private Vector3 originalPos;
    private LTDescr movementAnimation;

    public bool isInteractable;
    public bool blockOnMouseOver;
    public int cardState;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    public void Init()
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
            if (movementAnimation != null)
            {
                LeanTween.cancel(gameObject);
            }
            GameManager.Instance.DehighlightCommonCards(1);
            movementAnimation = LeanTween.moveLocalY(gameObject, originalPos.y, 0.2f);
        }
    }
}