using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

[Serializable]
public class Card : MonoBehaviour, IPointerClickHandler
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
    private bool isRare;
    private bool isSelected;
    private bool isTargetable;
    private Vector3 originalPos;
    private LTDescr movementAnimation;

    public bool isInteractable = false;

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
        if (state < 0)
        {
            image.sprite = cardSpriteBack;
            SetInteractable(false);
            return;
        }

        SetInteractable(true);
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

        if (selected)
            GameManager.Instance.selectedCard = this;
        else
            GameManager.Instance.selectedCard = null;

    }
    public void OnPointerClick(PointerEventData data)
    {
        if (isInteractable)
        {
            if (data.button == PointerEventData.InputButton.Left)
            {
                if (!isSelected)
                {
                    SetCardState(2);
                    GameManager.Instance.HighlightCommonCards(cardType, 2);
                }
                else
                {
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
    private void OnMouseEnter()
    {
        if (isInteractable)
        {
            if (movementAnimation != null)
            {
                LeanTween.cancel(movementAnimation.id);
            }
            GameManager.Instance.HighlightCommonCards(cardType, 1);
            movementAnimation = LeanTween.moveLocalY(gameObject, 30, 0.5f);
        }
    }
    private void OnMouseExit()
    {
        if (isInteractable)
        {
            if (movementAnimation != null)
            {
                LeanTween.cancel(movementAnimation.id);
            }
            GameManager.Instance.DehighlightCommonCards();
            movementAnimation = LeanTween.moveLocalY(gameObject, originalPos.y, 0.5f);
        }
    }
}