using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Item Item { get; private set; }

    private EventObject eventObject;

    private Image image;

    private GridLayoutGroup parent;

    private RectTransform rectTransform;
    private int oringinIndex;
    private Vector2 startPosition;
    private bool isDraging;

    private CanvasGroup itemNameBG;
    private Text itemNameText;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        eventObject = GetComponent<EventObject>();
        parent = transform.parent.GetComponent<GridLayoutGroup>();
        itemNameBG = transform.parent.parent.parent.GetChild(3).GetComponent<CanvasGroup>();
        itemNameText = itemNameBG.transform.GetChild(0).GetComponent<Text>();
    }

    /// <summary>
    /// 初始化，並從資料庫中取得道具資料
    /// </summary>
    /// <param name="id"></param>
    public void Initialize(int id)
    {
        eventObject.StopAllCoroutines();
        Item = GameDatabase.Instance.ItemDB[id];
        gameObject.name = Item.itemName;
        image.sprite = Item.sprite;
        eventObject.eventPoint = new List<EventPoint>(Item.clickEvent);
        eventObject.StartCoroutine(eventObject.RunEvent());
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        oringinIndex = transform.GetSiblingIndex();
        // 設定父物件為最上層，這樣才不會受到 mask 影響
        transform.SetParent(transform.parent.parent.parent);
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
        bool mix = false;
        if (EventSystem.current.IsPointerOverGameObject())
        {
            ItemButton itemButton = eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemButton>();
            if (itemButton != null)
            {
                Item targetItem = itemButton.Item;
                if (targetItem != null)
                {
                    foreach (ItemMixSet itemMixSet in GameDatabase.Instance.ItemMixDatabase)
                    {
                        if (itemMixSet.item1Id == Item.id && itemMixSet.item2Id == targetItem.id ||
                            itemMixSet.item2Id == Item.id && itemMixSet.item1Id == targetItem.id)
                        {
                            // 合成資料庫中找到可合成組合，合成後觸發事件
                            mix = true;
                            PlayerData.Instance.LoseItem(itemMixSet.item1Id);
                            PlayerData.Instance.LoseItem(itemMixSet.item2Id);
                            PlayerData.Instance.GainItem(itemMixSet.resultId);
                            EventExcutor.Instance.Register(eventObject, itemMixSet.commands);
                        }
                    }
                }
            }
        }
        if (!mix)
            rectTransform.position = startPosition;
        transform.SetParent(parent.transform);
        transform.SetSiblingIndex(oringinIndex);
        image.raycastTarget = true;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // 拖曳結束也會觸發，因此不能啟動點擊
        if (isDraging)
            return;
        eventObject.Clicked = true;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        itemNameBG.alpha = 1;
        itemNameText.text = Item.itemName;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemNameBG.alpha = 0;
    }
}
