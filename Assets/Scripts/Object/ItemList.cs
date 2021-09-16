using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemList : MonoBehaviour
{
    public CanvasGroup itemListCanvas;
    public ScrollRect scrollRect;
    public Transform itemListContent;
    public ItemButton ItemButtonPrefab;

    public Image leftArrow;
    public Image rightArrow;

    private List<Item> items;

    private void Awake()
    {
        scrollRect = itemListCanvas.GetComponent<ScrollRect>();
        ContentSizeFitter contentSizeFitter = GetComponent<ContentSizeFitter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (itemListCanvas.alpha > 0)
                CloseItemList();
            else
                OpenItemList();
        }
    }

    public void OpenItemList()
    {
        // 執行對話中，不可操作
        if (DialogueSystem.Instance.IsShowingDialogue())
            return;

        itemListCanvas.alpha = 1;
        itemListCanvas.interactable = true;
        itemListCanvas.blocksRaycasts = true;

        int index = 0;
        foreach (int id in PlayerData.Instance.items)
        {
            ItemButton itemButton;
            // 需要的索引值超出目前現有的物件數量，新增物件
            if (index >= itemListContent.childCount)
                itemButton = Instantiate(ItemButtonPrefab, itemListContent);
            // 物件足夠時，取得該物件
            else
            {
                itemButton = itemListContent.GetChild(index).GetComponent<ItemButton>();
                itemButton.gameObject.SetActive(true);
            }
            itemButton.Initialize(id);
            index++;
        }
        // 若超過已有道具數量，隱藏
        for (int i = index;i < itemListContent.childCount;i++)
            itemListContent.GetChild(i).gameObject.SetActive(false);
    }

    public void CloseItemList()
    {
        itemListCanvas.alpha = 0;
        itemListCanvas.interactable = false;
        itemListCanvas.blocksRaycasts = false;
    }

    public void Scroll(bool left)
    {
        Color32 color = leftArrow.color;
        color.a = 45;
        leftArrow.color = color;
        rightArrow.color = color;
        if (left)
            scrollRect.horizontalNormalizedPosition = 0;
        else
            scrollRect.horizontalNormalizedPosition = 1;
    }

    public void HideArrow()
    {
        Color32 color = leftArrow.color;
        color.a = 0;
        leftArrow.color = color;
        rightArrow.color = color;
    }
}
