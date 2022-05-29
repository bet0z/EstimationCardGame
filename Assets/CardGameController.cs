using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CardGameController : MonoBehaviour
{
    public UILabel labRoundNumberl, labBidStatus;
    public UILabel labTeam1Score, labTeam2Score;

    public List<UserGameCtrl> lstUserCtrls;
    public GameObject objDashCallWindow;
    public GameObject objBidCallWindow;
    public GameObject objStartRoundButton, objRestartRoundButton;
    public GameObject objCheckCoupleMode;

    [HideInInspector]
    public static CardGameController Instance;

    [HideInInspector]
    public bool isPause = false;
    [HideInInspector]
    public int nCardType, nPlateType, nWaitingTime;

    private CardGameType enCardGameType;

    public int nRoundIndex, nRoundTotalCount;
    
    private Sprite[] arrCardFrontSprites;
    private Sprite[] arrCardBackSprites;

    public int nStarterIndex;

    public GameStatusWindowCtrl StatusWindow;
    public List<ScoreStatusListItem> lstStatusList;
    public GameObject RoundStatusBack;

    public IEnumerator InitCardGame( CardGameType gameType, int cardtype, int platetype, int waitingtime, int starterUserIndex = 0) {
        yield return new WaitForEndOfFrame();
        enCardGameType = gameType;
        nCardType = cardtype;
        nPlateType = platetype;
        nWaitingTime = waitingtime;

        nRoundTotalCount = GlobalAction.GetRoundCount(gameType);

        arrCardFrontSprites = GlobalAction.GetCardFrontSprites(cardtype);
        arrCardBackSprites = GlobalAction.GetCardBackSprites(cardtype);

        nWaitingTime = waitingtime;
        
        RoundController.Instance.Inint(nRoundIndex, GetRoundType(nRoundIndex), starterUserIndex);
    }

    public RoundType GetRoundType(int nRoundIndex)
    {
        int nTotalRound = 18;

        if (enCardGameType == CardGameType.FullBola)
        {
            nTotalRound = 18;
        }

        switch (nTotalRound - (nRoundIndex+1))
        {
            case 4:
                return RoundType.NONERound;
            case 3:
                return RoundType.SPADERound;
            case 2:
                return RoundType.HEARTRound;
            case 1:
                return RoundType.DIAMONDRound;
            case 0:
                return RoundType.CLUBRound;
            default:
                return RoundType.NormalRound;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void ContinueRound()
    {
        labBidStatus.text = "";
        nRoundIndex++;
        objStartRoundButton.SetActive(false);
        objRestartRoundButton.SetActive(false);
        objCheckCoupleMode.SetActive(false);
        ReStartNewRound();
    }

    public void ReStartNewRound()
    {
        StartCoroutine(InitCardGame(CardGameType.FullBola, 1, 1, 10, nRoundIndex % 4));
    }

    public void StartNewRound()
    {
        labBidStatus.text = "";
        nRoundIndex = 0;
        nStarterIndex = nRoundIndex % 4;

        objCheckCoupleMode.SetActive(false);
        objStartRoundButton.SetActive(false);
        objRestartRoundButton.SetActive(false);
        
        lstStatusList = new List<ScoreStatusListItem>();

        foreach (UserGameCtrl user in lstUserCtrls)
            user.avatarCtrl.InitScoreNum();
        ReStartNewRound();
        if (objCheckCoupleMode.gameObject.GetComponent<UIToggle>().isChecked)
        {
            labTeam1Score.gameObject.SetActive(true);
            labTeam2Score.gameObject.SetActive(true);
            labTeam1Score.text = "0";
            labTeam2Score.text = "0";
        }
        else
        {
            labTeam1Score.gameObject.SetActive(false);
            labTeam2Score.gameObject.SetActive(false);
        }
    }

    public void FinishedDownList()
    {
        if(RoundStatusBack.GetComponent<TweenPosition>().direction == AnimationOrTween.Direction.Reverse)
        {
            Debug.Log("Backward");
            StatusWindow.InitItems();
        }
        else if (RoundStatusBack.GetComponent<TweenPosition>().direction == AnimationOrTween.Direction.Forward)
        {
            Debug.Log("Forward");
            StatusWindow.SetData(lstStatusList);

            StartCoroutine(DisplayScoreList());
        }
        
    }
    public IEnumerator DisplayScoreList()
    {
        yield return new WaitForSeconds(2.0f);

    }
    public void SetTotalBidStatus(int status)
    {
        string part = status > 0 ? "+" : "";
        if(status > 0)
        {
            part = "+";
            labBidStatus.color = new Color(140f / 255f, 16f / 255f, 50f / 255f, 255f / 255f);
        }
        else
        {
            part = "";
            labBidStatus.color = new Color(16f / 255f, 33f / 255f, 140f / 255f, 255f / 255f);
        }
        labBidStatus.text = part + status.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        labRoundNumberl.text = (nRoundIndex + 1).ToString();
    }
    
    public void OpenCallWindow()
    {
        Bid currentBid = new Bid();
        currentBid.nLastUserIndex = 0;
        currentBid.arrbidValue[0] = new BidValue(4, Suit.DIAMOND);
        CardGameController.Instance.objBidCallWindow.GetComponent<CallDialogWindow>().ShowCallDialog(currentBid, null);
    }

    public bool IsReadyToStartBid()
    {
        int nNumber = 0;
        foreach (UserGameCtrl ctrl in lstUserCtrls)
        {
            if (ctrl.lstWaitingCards.Count == GlobalValue.Instance.CardNumber)
            {
                nNumber++;
            }
        }
        return nNumber == 4;
    }
    public bool IsReadyToStartArrangement()
    {
        int nNumber = 0;
        foreach (UserGameCtrl ctrl in lstUserCtrls)
        {
            if (ctrl.lstUserCards.Count == GlobalValue.Instance.CardNumber)
            {
                nNumber++;
            }
        }
        return nNumber == 4;
    }
}
