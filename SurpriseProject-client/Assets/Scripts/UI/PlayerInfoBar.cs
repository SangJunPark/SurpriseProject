using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoBar : MonoBehaviour
{
    [SerializeField] GameObject TargetPlayer;

    Image BurnImage; 
    // Start is called before the first frame update
    void Start()
    {
        BurnImage = transform.Find("Burn").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(TargetPlayer.transform.position);

        float x = screenPos.x;
        BurnImage.transform.position = new Vector3(screenPos.x, screenPos.y + 100, BurnImage.transform.position.z);
    }
}
