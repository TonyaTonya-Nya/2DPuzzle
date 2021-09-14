using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Item Item { get; private set; }

    private Image image;
    private EventObject eventObject;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private bool isDraging;

    private void Awake()
    {
        image = GetComponent<Image>();
        eventObject = GetComponent<EventObject>();
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 初始化，並從資料庫中取得道具資料
    /// </summary>
    /// <param name="id"></param>
    public void Initialize(int id)
    {
        Item = GameDatabase.Instance.ItemDB[id];
        gameObject.name = Item.itemName;
        image.sprite = Item.sprite;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        // 拖曳中的物件設為最後一個子物件，才能顯示在所有人上方
        transform.SetAsLastSibling();
        startPosition = rectTransform.position;
        isDraging = true;
        // 拖曳，關閉偵測，避免拖曳結束後觸發點擊，同時讓拖曳到的目標物件能得到偵測
        image.raycastTarget = false;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Vector2 mosue = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = mosue;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Item targetItem = eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemButton>().Item;
            //if (Item.canMix && targetItem != null)
            //{
            //    if (Item.mixTarget == targetItem.id)
            //    {
            //        eventObject.eventPoint = new List<EventPoint>(Item.mixEvent);
            //        eventObject.clicked = true;
            //    }
            //    else
            //    {
            //        rectTransform.position = startPosition;
            //    }
            //}
            //else
            //{
            //    rectTransform.position = startPosition;
            //}
        }
        else
        {
            rectTransform.position = startPosition;
        }
        image.raycastTarget = true;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // 拖曳結束也會觸發，因此不能啟動點擊
        if (isDraging)
            return;
        eventObject.eventPoint = new List<EventPoint>(Item.clickEvent);
        eventObject.clicked = true;
    }
}
