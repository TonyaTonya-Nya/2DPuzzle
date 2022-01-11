using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAnime : MonoBehaviour
{
    public GameObject bg;
    public SpriteRenderer title;
    public GameObject gameList;
    public GameObject againBtn;
    public GameObject exitBtn;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayerData.Instance.Post_Http());
    }

    // Update is called once per frame
    void Update()
    {
        title.color = new Color(title.color.r, title.color.g, title.color.b, title.color.a - 0.2f/60f);

        if (title.color.a <= 0.05f)
        {
            gameList.SetActive(true);
        }

        if (gameList.transform.position.y > 2150)
        {
            bg.SetActive(true);
            againBtn.SetActive(true);
            exitBtn.SetActive(true);
        }
    }
}
