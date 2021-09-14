﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    public CanvasGroup itemListCanvas;
    public Transform itemListContent;
    public ItemButton ItemButtonPrefab;

    private List<Item> items;

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
                itemButton = itemListContent.GetChild(index).GetComponent<ItemButton>();
            itemButton.Initialize(id);
            index++;
        }
    }

    private void Refresh()
    {

    }

    public void CloseItemList()
    {
        itemListCanvas.alpha = 0;
        itemListCanvas.interactable = false;
        itemListCanvas.blocksRaycasts = false;
    }
}
