using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastTickCtrl : MonoBehaviour
{
    [HideInInspector]
    public bool isShow = true;

    public List<GameObject> lstTrickStatus;
    public GameObject EmptyTrick;
    public GameObject TrickStatus;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitLastTrickUI()
    {
        EmptyTrick.SetActive(true);
        TrickStatus.SetActive(false);
    }

    public void setTrickStatus(int nUserIndex, int cardValue, Suit cardType)
    {
        if (!TrickStatus.active) {
            TrickStatus.SetActive(true);
            EmptyTrick.SetActive(false);
        } 
        int nResourceIndex = ((int)cardType) * 13 + cardValue - 1;
        lstTrickStatus[nUserIndex].GetComponent<UI2DSprite>().sprite2D = GlobalValue.Instance.m_arrCardFrontSprites[nResourceIndex];
    }
}
