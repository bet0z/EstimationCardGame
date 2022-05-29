using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserRoundStatus : MonoBehaviour
{
    public GameObject objLost, objWon;
    public UILabel labRoundScore, labOrderedNum;
    public GameObject objClubar, objDiamond, objSpade, objHeart, objSun, objRisk;

    private int scoreValue, orderNum;
    private bool isWon, isRisk, isCaller;
    private BidValue bidValue;

    public UserRoundStatus(int scoreValue, int orderNum, bool isWon, bool isRisk, bool isCaller,BidValue bidValue)
    {
        this.scoreValue = scoreValue;
        this.orderNum = orderNum;
        this.isWon = isWon;
        this.isRisk = isRisk;
        this.bidValue = bidValue;
        this.isCaller = isCaller;
    }

    public UserRoundStatus(UserRoundStatus status)
    {
        this.scoreValue = status.scoreValue;
        this.orderNum = status.orderNum;
        this.isWon = status.isWon;
        this.isRisk = status.isRisk;
        this.bidValue = status.bidValue;
        this.isCaller = status.isCaller;
    }

    public void DisplayData(UserRoundStatus status)
    {
        this.scoreValue = status.scoreValue;
        this.orderNum = status.orderNum;
        this.isWon = status.isWon;
        this.isRisk = status.isRisk;
        this.bidValue = status.bidValue;
        this.isCaller = status.isCaller;
        DisPlayInfo();
    }

    public void DisPlayInfo()
    {
        labRoundScore.text = this.scoreValue.ToString();
        labOrderedNum.text = this.bidValue.value.ToString();
        if (isWon)
        {
            objLost.SetActive(false);
            objWon.SetActive(true);
        }
        else
        {
            objLost.SetActive(true);
            objWon.SetActive(false);
        }
        HideAllEventIcon();
        if (isCaller)
        {
            switch (this.bidValue.suit)
            {
                case Suit.CLUB:
                    objClubar.SetActive(true);
                    break;
                case Suit.DIAMOND:
                    objDiamond.SetActive(true);
                    break;
                case Suit.HEART:
                    objHeart.SetActive(true);
                    break;
                case Suit.NONE:
                    objSun.SetActive(true);
                    break;
                case Suit.SPADE:
                    objSpade.SetActive(true);
                    break;

            }
        }
        else if (isRisk)
        {
            objRisk.SetActive(true);
        }
    }

    private void HideAllEventIcon()
    {
        objClubar.SetActive(false);
        objDiamond.SetActive(false);
        objSpade.SetActive(false);
        objHeart.SetActive(false);
        objSun.SetActive(false);
        objRisk.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
