using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BidResultUICtrl : MonoBehaviour
{
    public GameObject objSpadBid;
    public GameObject objDiamondBid;
    public GameObject objClubarBid;
    public GameObject objHeartBid;
    public GameObject objNTBid;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
    }

    public void TweenFinished(GameObject obj)
    {
        foreach(TweenScale scale in obj.GetComponents<TweenScale>())
        {
            scale.enabled = true;
            scale.ResetToBeginning();
        }
        obj.GetComponent<TweenColor>().enabled = true;
        obj.GetComponent<TweenColor>().ResetToBeginning();
    }

    public void HideBidResult()
    {
        this.gameObject.SetActive(false);
        TweenFinished(objSpadBid);
        TweenFinished(objDiamondBid);
        TweenFinished(objClubarBid);
        TweenFinished(objHeartBid);
        TweenFinished(objNTBid);
    }

    public void ShowBidResult(Suit bidSuit)
    {
        this.gameObject.SetActive(true);
        switch (bidSuit)
        {
            case Suit.SPADE:
                objSpadBid.SetActive(true);
                objDiamondBid.SetActive(false);
                objClubarBid.SetActive(false);
                objHeartBid.SetActive(false);
                objNTBid.SetActive(false);
                break;
            case Suit.DIAMOND:
                objSpadBid.SetActive(false);
                objDiamondBid.SetActive(true);
                objClubarBid.SetActive(false);
                objHeartBid.SetActive(false);
                objNTBid.SetActive(false);
                break;
            case Suit.CLUB:
                objSpadBid.SetActive(false);
                objDiamondBid.SetActive(false);
                objClubarBid.SetActive(true);
                objHeartBid.SetActive(false);
                objNTBid.SetActive(false);
                break;
            case Suit.HEART:
                objSpadBid.SetActive(false);
                objDiamondBid.SetActive(false);
                objClubarBid.SetActive(false);
                objHeartBid.SetActive(true);
                objNTBid.SetActive(false);
                break;
            case Suit.NONE:
                objSpadBid.SetActive(false);
                objDiamondBid.SetActive(false);
                objClubarBid.SetActive(false);
                objHeartBid.SetActive(false);
                objNTBid.SetActive(true);
                break;
        }
    }
}
