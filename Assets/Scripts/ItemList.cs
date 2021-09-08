using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    public Vector2 startPosition;
    public float width;
    public float space;

    public CanvasGroup itemListCanvas;
    public ItemButton ItemButtonPrefab;

    private List<Item> items;

    // Start is called before the first frame update
    void Start()
    {

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
        itemListCanvas.alpha = 1;
        itemListCanvas.interactable = true;
        itemListCanvas.blocksRaycasts = true;

        int index = 0;
        foreach (int id in PlayerData.Instance.items)
        {
            ItemButton itemButton;
            // 需要的索引值超出目前現有的物件數量，新增物件
            if (index >= itemListCanvas.transform.childCount)
                itemButton = Instantiate(ItemButtonPrefab, itemListCanvas.transform);
            // 物件足夠時，取得該物件
            else
                itemButton = itemListCanvas.transform.GetChild(index).GetComponent<ItemButton>();

            itemButton.GetComponent<RectTransform>().offsetMax = new Vector2(startPosition.x + width + index * (width + space), startPosition.y + width);
            itemButton.GetComponent<RectTransform>().offsetMin = new Vector2(startPosition.x + index * (width + space), startPosition.y);
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
