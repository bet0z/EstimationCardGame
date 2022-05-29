using estimation.ai.manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    [HideInInspector]
    public GlobalValue.DashCall OnEventDashCall;
    [HideInInspector]
    public GlobalValue.BidCall OnEventBidCall;

    [HideInInspector]
    public GlobalValue.BidConfirm OnEventBidConfirm;

    [HideInInspector]
    public GlobalValue.TurnPlayed OnEventTurnPlayed;
    
    [HideInInspector]
    public RoundType enRoundType;

    public static RoundController Instance;

    public BidResultUICtrl bidResultUI;
    public LastTickCtrl lastTrickUI;

    private int nRoundIndex, nUserBidIndex;
   
    private bool blFinishBidStep, blStartArrangment;
    private int nDashRequestNumber = 0;
    private bool blQuestionForDashCall = false;
    private bool blDashCallEnd = false;

    private bool CanCallNextUser = false;
    private bool CanConfirmNextUser = false;
    [HideInInspector]
    public Bid currentBid;

    [HideInInspector]
    public State mainState;
    
    public void Inint(int roundIndex, RoundType type, int bidStarter)
    {
        foreach (UserGameCtrl user in CardGameController.Instance.lstUserCtrls)
        {
            user.avatarCtrl.InitAvatar(CardGameController.Instance.objCheckCoupleMode.gameObject.GetComponent<UIToggle>().isChecked);
            user.SetedBidValue = null;
            user.GAME_STATUS = UserGameStatus.Waiting;
            user.isDashCaller = false;
            user.isDashcallDeside = false;
            user.blCanPlayCard = false;
            user.InitScore();
            if (user.lstUserCards.Count > 0)
            {
                foreach(CardInfo info in user.lstUserCards)
                {
                    if (info.gameObject != null)
                        Destroy(info.gameObject);
                }

                user.lstWaitingCards.Clear();
                user.lstUserCards.Clear();
            }
            
        }

        nRoundIndex = roundIndex;
        nUserBidIndex = bidStarter;

        enRoundType = type;
        blFinishBidStep = false;
        blStartArrangment = false;
        blQuestionForDashCall = false;
        blDashCallEnd = false;
        currentBid = new Bid();

        mainState = new State(bidStarter);
        lastTrickUI.InitLastTrickUI();
        bidResultUI.HideBidResult();

        StartCoroutine(RandomCards());
    }

    // Start is called before the first frame update
    void Start()
    {
        OnEventDashCall += EvencDashCall;
        OnEventBidCall += EventBidCall;
        OnEventBidConfirm += EventBidConfirm;
        OnEventTurnPlayed += EventTurnPlayed;
        Instance = this;
    }




    // Update is called once per frame
    void Update()
    {
        if (CardGameController.Instance.isPause)
            return;
        if (!blStartArrangment && CardGameController.Instance.IsReadyToStartArrangement())
        {
            blStartArrangment = true;
            foreach (UserGameCtrl ctrl in CardGameController.Instance.lstUserCtrls)
            {
                if (ctrl.lstUserCards.Count == GlobalValue.Instance.CardNumber) {
                    StartCoroutine(ctrl.StartArrangement());
                }
            }

        }
        else if (!blFinishBidStep && CardGameController.Instance.IsReadyToStartBid())
        {
            if (!blQuestionForDashCall)
            {
                nDashRequestNumber = 0;
                blQuestionForDashCall = true;
                //Question for "DashCall" to everybody
                foreach (UserGameCtrl user in CardGameController.Instance.lstUserCtrls)
                {
                    user.DecideforDash(OnEventDashCall);
                }
            }
        }
    }

    private bool blCanNextTurn = false;
    private List<List<CardBaseInfo>> lstCardPlayLog;
    private List<CardBaseInfo> lstPlayedCards;

    public IEnumerator StartPlayStep(int starterUser)
    {
        int nMaxTrickNum = 2;
        int nMaxTurnNum = 4;

        int nTrickStartIndex = starterUser;
        lstCardPlayLog = new List<List<CardBaseInfo>>();
        for (int nTrackI = 0; nTrackI < nMaxTrickNum; nTrackI++)
        {
            int nTurnIndex = 0;
            lstPlayedCards = new List<CardBaseInfo>();
            while (nTurnIndex < nMaxTurnNum)
            {
                blCanNextTurn = false;
                int nCurUserIndex = mainState.GetCurrentPlayer().id;
                CardGameController.Instance.lstUserCtrls[nCurUserIndex].PlayTurn(CardGameController.Instance.nWaitingTime, OnEventTurnPlayed);
                while (true)
                {
                    yield return new WaitForSeconds(0.1f);
                    if (blCanNextTurn)
                        break;
                }
                nTurnIndex++;
            }
            int nTrickWinner = mainState.GetCurrentPlayer().id;
            yield return new WaitForSeconds(0.5f);
            CardGameController.Instance.lstUserCtrls[nTrickWinner].avatarCtrl.SetCallStatus(CardGameController.Instance.lstUserCtrls[nTrickWinner].SetedBidValue.value, mainState.GetTrickStatus(nTrickWinner));
            for (int nI = 0; nI < 4; nI++){
                lastTrickUI.setTrickStatus(nI, CardGameController.Instance.lstUserCtrls[nI].MeldCard.nCardValue, CardGameController.Instance.lstUserCtrls[nI].MeldCard.enCardType);
                CardGameController.Instance.lstUserCtrls[nI].MeldCard.SetEndPoint(CardGameController.Instance.lstUserCtrls[nTrickWinner].avatarCtrl.gameObject.transform);
                CardGameController.Instance.lstUserCtrls[nI].MeldCard.ChangeState(CardLogicState.PLAYING);
            }
            yield return new WaitForSeconds(0.5f);
            lstCardPlayLog.Add(lstPlayedCards);
        }
        int nWinnerNum = 0;
        int nMaxWinnerValue = 0;
        int nMaxWinnerIndex = 0;
        List<int> lstWinner = new List<int>();
        List<int> lstLoser = new List<int>();

        for(int nI = 0; nI < State.PLAYER_COUNT; nI++)
        {
            int nReuslt = mainState.GetTrickStatus(nI);

            
            if (CardGameController.Instance.lstUserCtrls[nI].SetedBidValue.value == nReuslt)
            {
                if(nMaxWinnerValue < nReuslt)
                {
                    nMaxWinnerValue = nReuslt;
                    nMaxWinnerIndex = nI;
                }
                CardGameController.Instance.lstUserCtrls[nI].avatarCtrl.VisibleRoundWinner();
                nWinnerNum++;
                lstWinner.Add(nI);
            }
            else
            {
                lstLoser.Add(nI);
            }
        }
        ScoreStatusListItem item = new ScoreStatusListItem(CardGameController.Instance.nRoundIndex+1, nBidSum - 13);
        // Calculate score
        foreach(int userIndex in lstWinner)
        {
            int nResult = CalculateWinnerScore(nBidSum, userIndex, lstWinner.Count == 1, CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.gameEventType == GameEventType.RiskEvent, CardGameController.Instance.lstUserCtrls[userIndex].SetedBidValue);
            CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.SetScoreNumber(nResult);
            CardGameController.Instance.lstUserCtrls[userIndex].AddScore(nResult);
            item.AddUserRoundStatus(userIndex, nResult, CardGameController.Instance.lstUserCtrls[userIndex].SetedBidValue.value, true, CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.gameEventType == GameEventType.RiskEvent, userIndex == currentBid.nLastUserIndex, CardGameController.Instance.lstUserCtrls[userIndex].SetedBidValue);
        }

        foreach(int userIndex in lstLoser)
        {
            int nResult = CalculateLoserScore(nBidSum, userIndex, lstLoser.Count == 1, CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.gameEventType == GameEventType.RiskEvent, CardGameController.Instance.lstUserCtrls[userIndex].SetedBidValue, mainState.GetTrickStatus(userIndex));
            CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.SetScoreNumber(nResult);
            CardGameController.Instance.lstUserCtrls[userIndex].AddScore(nResult);
            item.AddUserRoundStatus(userIndex, nResult, CardGameController.Instance.lstUserCtrls[userIndex].SetedBidValue.value, false, CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.gameEventType == GameEventType.RiskEvent, userIndex == currentBid.nLastUserIndex, CardGameController.Instance.lstUserCtrls[userIndex].SetedBidValue);
        }
        CardGameController.Instance.lstStatusList.Add(item);

        if (CardGameController.Instance.objCheckCoupleMode.gameObject.GetComponent<UIToggle>().isChecked)
        {
            int nTeam1Score = CardGameController.Instance.lstUserCtrls[0].GetScore() + CardGameController.Instance.lstUserCtrls[2].GetScore();
            int nTeam2Score = CardGameController.Instance.lstUserCtrls[1].GetScore() + CardGameController.Instance.lstUserCtrls[3].GetScore();
            CardGameController.Instance.labTeam1Score.text = nTeam1Score.ToString();
            CardGameController.Instance.labTeam2Score.text = nTeam2Score.ToString();
        }

        if (nWinnerNum == 0)
        {
            CardGameController.Instance.ContinueRound();
        }
        else
        {
            CardGameController.Instance.nStarterIndex = nMaxWinnerIndex;

            CardGameController.Instance.objStartRoundButton.SetActive(true);
            CardGameController.Instance.objCheckCoupleMode.SetActive(true);

            if (CardGameController.Instance.nRoundIndex < 17)
            {
                CardGameController.Instance.objRestartRoundButton.SetActive(true);
            }
        }
    }

    private int CalculateLoserScore(int nTotalBidSum, int userIndex, bool isOnlyLoser, bool isRisker, BidValue bidValue, int result)
    {
        int nScore = 0;

        nScore -= Mathf.Abs(nScore - (bidValue.value - result));

        if (userIndex == currentBid.nLastUserIndex || bidValue.type == BidType.WITH)
        {
            nScore -= 10;
        }

        if (isOnlyLoser)
            nScore -= 10;

        if (isRisker)
            nScore -= 10;

        if (bidValue.value > 7)
            nScore -= 10;

        if (CardGameController.Instance.lstUserCtrls[userIndex].isDashCaller)
        {
            if (nTotalBidSum > 13)
                nScore -= 25;
            else
                nScore -= 33;
        }

        return nScore;
    }

    private int CalculateWinnerScore(int nTotalBidSum,int userIndex, bool isOnlyWinner, bool isRisker, BidValue bidValue)
    {
        int nScore = 0;

        nScore = 13 + bidValue.value;
        
        if (userIndex == currentBid.nLastUserIndex || bidValue.type == BidType.WITH)
        {
            nScore += 10;
        }
        
        if (isOnlyWinner)
            nScore += 10;

        if (isRisker)
            nScore += 10;

        if (bidValue.value > 7)
            nScore += 10;

        if (CardGameController.Instance.lstUserCtrls[userIndex].isDashCaller)
        {
            if (nTotalBidSum > 13)
                nScore +=25;
            else
                nScore += 33;
        }

        return nScore;
    }

    public void EventTurnPlayed(int nUserIndex, CardBaseInfo info)
    {
        if (lstPlayedCards == null)
            lstPlayedCards = new List<CardBaseInfo>();
        lstPlayedCards.Add(info);
        blCanNextTurn = true;
    }

    public void EvencDashCall(int userIndex, bool isDashCall)
    {
        if (isDashCall)
        {
            int nIndex = 0;
            foreach (UserGameCtrl user in CardGameController.Instance.lstUserCtrls)
            {
                if (nIndex == userIndex && isDashCall)
                    user.SetDashCaller();
                else
                    user.StopDashCall(OnEventDashCall);
                nIndex++;
            }
            blDashCallEnd = true;
            StartCoroutine(StartBidCall());
        }
        else
        {
            nDashRequestNumber++;
        }
        if(nDashRequestNumber == 4)
        {
            StartCoroutine(StartBidCall());

            blDashCallEnd = true;
        }
    }

    IEnumerator StartBidCall()
    {
        yield return new WaitForEndOfFrame();

        while(!currentBid.isFinishCallStep())
        {
            yield return new WaitForSeconds(0.1f);
            int nUserIndex = nUserBidIndex % 4;
            if(currentBid.arrbidValue[nUserIndex] == null)
            {
                CanCallNextUser = false;
                CardGameController.Instance.lstUserCtrls[nUserIndex].BidCallStart(currentBid, OnEventBidCall);
                int waitSecond = 0;
                while (true)
                {
                    if (CanCallNextUser)
                    {
                        CardGameController.Instance.lstUserCtrls[nUserIndex].avatarCtrl.StopTimer();
                        break;
                    }
                    if (waitSecond == CardGameController.Instance.nWaitingTime)
                    {
                        CardGameController.Instance.lstUserCtrls[nUserIndex].DesideBidValue();
                        if (CardGameController.Instance.lstUserCtrls[nUserIndex].USER_TYPE == USER_TYPE.MainPlayer)
                            CardGameController.Instance.objBidCallWindow.GetComponent<CallDialogWindow>().CloseWindow();
                        CardGameController.Instance.lstUserCtrls[nUserIndex].avatarCtrl.StopTimer();
                        break;
                    }
                    yield return new WaitForSeconds(1f);
                    waitSecond++;
                }
            }
            nUserBidIndex++;
        }
        if (currentBid.getLastNormalBid() == null)
            CardGameController.Instance.ReStartNewRound();
        else
        {
            for (int nI = 0; nI < CardGameController.Instance.lstUserCtrls.Count; nI++)
            {
                CardGameController.Instance.lstUserCtrls[nI].avatarCtrl.callStatus.Disable();
            }
            nUserBidIndex = currentBid.nLastUserIndex;
            StartCoroutine(StartBidConfirm());
        }
    }

    IEnumerator StartBidConfirm()
    {
        yield return new WaitForEndOfFrame();

        while (!currentBid.isFinishConfirmStep())
        {
            yield return new WaitForSeconds(0.1f);
            int nUserIndex = nUserBidIndex % 4;
            if (CardGameController.Instance.lstUserCtrls[nUserIndex].SetedBidValue == null)
            {
                CanConfirmNextUser = false;
                CardGameController.Instance.lstUserCtrls[nUserIndex].BidConfirmStart(currentBid, OnEventBidConfirm);
                int waitSecond = 0;
                while (true)
                {
                    if (CanConfirmNextUser)
                    {
                        CardGameController.Instance.lstUserCtrls[nUserIndex].avatarCtrl.StopTimer();
                        break;
                    }
                    if (waitSecond == CardGameController.Instance.nWaitingTime)
                    {
                        CardGameController.Instance.lstUserCtrls[nUserIndex].BidConfirmMainUser(currentBid, OnEventBidConfirm);
                        CardGameController.Instance.lstUserCtrls[nUserIndex].avatarCtrl.StopTimer();
                        break;
                    }
                    yield return new WaitForSeconds(1f);
                    waitSecond++;
                }
            }
            nUserBidIndex++;
        }
        
    }


    private bool CanPlayGame()
    {
        int nNumber = 0;
        for (int nI = 0; nI < CardGameController.Instance.lstUserCtrls.Count; nI++)
        {
            if (CardGameController.Instance.lstUserCtrls[nI].SetedBidValue != null)
                nNumber++;
        }
        return CardGameController.Instance.lstUserCtrls.Count == nNumber;
    }
   
    public void DisplayEventType()
    {
        foreach (UserGameCtrl user in CardGameController.Instance.lstUserCtrls)
        {
            if (user.isDashCaller)
            {
                user.SetedBidValue.type = BidType.DashCALL;
                user.avatarCtrl.SetBidEventStatus(user.SetedBidValue.type);
            }
            if (currentBid.nLastUserIndex == user.nUserIndex)
            {
                user.SetedBidValue.type = BidType.CALL;
                user.avatarCtrl.SetBidEventStatus(user.SetedBidValue.type);
            }
            else if ((user.SetedBidValue.value == CardGameController.Instance.lstUserCtrls[currentBid.nLastUserIndex].SetedBidValue.value))
            {
                user.SetedBidValue.type = BidType.WITH;
                user.avatarCtrl.SetBidEventStatus(user.SetedBidValue.type);
            }
        }
    }
    private int nBidSum = 0;
    public void EventBidConfirm(int userIndex, int confirmValue)
    {
        Suit cardType = currentBid.getLastNormalBid().suit;
        CardGameController.Instance.lstUserCtrls[userIndex].SetBidValue(new BidValue(confirmValue, cardType));
        
        mainState.GetPlayerInfo(userIndex).bidValue = new BidValue(confirmValue, cardType);
        CanConfirmNextUser = true;
        if(currentBid.nLastUserIndex == userIndex)
        {
            bidResultUI.ShowBidResult(currentBid.getLastNormalBid().suit);
        }
        if (CanPlayGame())
        {
            nBidSum = 0;
            foreach (UserGameCtrl ctrl in CardGameController.Instance.lstUserCtrls)
            {
                nBidSum += ctrl.SetedBidValue.value;
            }
            CardGameController.Instance.SetTotalBidStatus(nBidSum - 13);
            if (Mathf.Abs(13 - nBidSum) >= 2)
            {
                int nLastConfirmer = currentBid.nLastUserIndex == 0 ? 3 : RoundController.Instance.currentBid.nLastUserIndex - 1;
                if (CardGameController.Instance.lstUserCtrls[nLastConfirmer].isDashCaller)
                    nLastConfirmer = 3;
                CardGameController.Instance.lstUserCtrls[nLastConfirmer].avatarCtrl.SetBidEventStatus(GameEventType.RiskEvent);

            }

            DisplayEventType();
            BidValue leadBid = currentBid.getLastNormalBid();
            mainState.leadBid = leadBid;
            
            mainState.InitCurrentPlayer(currentBid.nLastUserIndex);

            blFinishBidStep = true;
            StartCoroutine(StartPlayStep(currentBid.nLastUserIndex));
        }   
    }


    /// <summary>
    /// Call Event callback function
    /// </summary>
    /// <param name="userIndex">Called User index</param>
    /// <param name="bidValue">This value is can will seted like as "Call, DashCall, WithCall, Pass"</param>
    public void EventBidCall(int userIndex, BidValue bidValue)
    {
        CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.SetBidStatus(bidValue.type);
        switch (bidValue.type)
        {
            case BidType.PASS:
                CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.callStatus.Disable();
                break;
            case BidType.DashCALL:
                CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.callStatus.Disable();
                break;
            case BidType.CALL:
                CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.callStatus.SetValue(bidValue);
                break;
            case BidType.WITH:
                CardGameController.Instance.lstUserCtrls[userIndex].avatarCtrl.callStatus.SetValue(bidValue);
                break;
        }
        currentBid.setBidValue(userIndex, bidValue);
        CanCallNextUser = true;
    }

    IEnumerator RandomCards()
    {
        yield return new WaitForEndOfFrame();

        CardGameController.Instance.lstUserCtrls[0].InitCardList(mainState.GetPlayerInfo(0).hand);

        CardGameController.Instance.lstUserCtrls[1].InitCardList(mainState.GetPlayerInfo(1).hand);

        CardGameController.Instance.lstUserCtrls[2].InitCardList(mainState.GetPlayerInfo(2).hand);

        CardGameController.Instance.lstUserCtrls[3].InitCardList(mainState.GetPlayerInfo(3).hand);
    }
}
public class Bid
{
    public BidValue[] arrbidValue ;
    public int nLastUserIndex;

    public BidValue getLastNormalBid()
    {
        if(nLastUserIndex == -1)
            return null;
        return arrbidValue[nLastUserIndex];
    }

    public void setBidValue(int userIndex, BidValue value)
    {
        arrbidValue[userIndex] = value;
        if (value.type == BidType.CALL)
        {
            nLastUserIndex = userIndex;
            for (int nI = 0; nI < arrbidValue.Length; nI++)
            {
                if (userIndex != nI && arrbidValue[nI] != null && arrbidValue[nI].type == BidType.CALL && arrbidValue[nI].CompareBidValue(value))
                {
                    arrbidValue[nI] = null;
                }
            }
        }
    }

    public bool isFinishConfirmStep()
    {
        int nNumber = 0;
        foreach (UserGameCtrl value in CardGameController.Instance.lstUserCtrls)
        {
            if (value.SetedBidValue != null)
                nNumber++;
        }
        return nNumber == arrbidValue.Length;
    }

    public bool isFinishCallStep()
    {
        int nNumber = 0;
        foreach(BidValue value in arrbidValue)
        {
            if (value != null)
                nNumber++;
        }
        return  nNumber == arrbidValue.Length;
    }

    public Bid()
    {
        nLastUserIndex = -1;
        arrbidValue = new BidValue[4];
    }
}

public class BidValue
{
    public int value;
    public Suit suit;
    public BidType type;

    public BidValue()
    {
        SetPassBid();
    }

    public BidValue(BidType type)
    {
        this.type = type;
    }

    public BidValue(int value, Suit card, BidType type = BidType.CALL)
    {
        this.value = value;
        suit = card;
        this.type = type;
    }
    
    public string toString()
    {
        string result = "";
        if (type != BidType.PASS)
        {
            result = ": " + type.ToString() + "," + suit.ToString() + "-" + value;
        }
        else
        {
            result = ": PASS," + value;
        }
        return result;
    }

    /// <summary>
    /// Compare two BidValue
    /// </summary>
    /// <param name="value">Target BidValue</param>
    /// <returns> 
    /// True  : If target is bigger than self
    /// False : If target is smaller self
    /// </returns>
    public bool CompareBidValue(BidValue targetValue)
    {
        if (targetValue.value == value)
        {
            return targetValue.suit > suit;
        }
        else if (targetValue.value > value)
            return true;
        else
            return false;
    }

    public bool isEqual(BidValue value)
    {
        return this.value == value.value && suit == value.suit;
    }

    public void SetPassBid()
    {
        this.suit = Suit.NONE;
        value = -1;
        type = BidType.PASS;
    }

    public BidType GetBidType(BidValue value)
    {
        BidType type = BidType.CALL;

        return type;
    }
}