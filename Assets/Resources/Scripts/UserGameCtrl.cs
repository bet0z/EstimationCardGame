
using estimation.ai;
using estimation.ai.common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallType
{
	public int nCardValue;
	public Suit CardType;
}

public class UserGameCtrl : MonoBehaviour
{
	[HideInInspector]
	public List<CardInfo> lstUserCards;
	private CardInfo PlayingCard;

	public List<CardInfo> lstWaitingCards;

	public UserGameStatus GAME_STATUS;
	public USER_TYPE USER_TYPE;
	public UserAvatarCtrl avatarCtrl;

	public BidValue SetedBidValue;

	public Transform trCardSpawnPos, trCardWaiSPos, trCardWaiEPos, trCardPlayPoint, trCardlastPoint;

	public int nUserIndex;

	[HideInInspector]
	public bool isDashCaller = false;
	[HideInInspector]
	public bool isDashcallDeside = false;

	public delegate void CardPlayed(CardBaseInfo info);
	private CardPlayed CardPlayedEvent;

	public delegate void WatingTurnEnd();
	private WatingTurnEnd delWatingTurnEnd;

	[HideInInspector]
	public bool blCanPlayCard = false;
	private bool blFinishDecideTurn = false;
	private Move nextMove = null;

	private int nTotalScore = 0;

	// Start is called before the first frame update
	void Start()
	{
		CardPlayedEvent += PlayedCard;
		delWatingTurnEnd += TurnWaitingEndCallBack;
	}

	public void InitScore()
    {
		nTotalScore = 0;
	}

	public void AddScore(int nScore)
    {
		nTotalScore += nScore;

	}

	public int GetScore()
    {
		return nTotalScore;
    }
	// Update is called once per frame
	void Update()
	{
		if (blCanPlayCard && ((Input.GetMouseButtonDown(0) || Input.touchCount > 0)))
		{
			Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100))
			{
				GameObject objHit = hit.collider.gameObject;
				if (objHit.tag == "Card")
				{
					CardInfo info = objHit.GetComponent<CardInfo>();
					Dictionary<int, Card> melds = RoundController.Instance.mainState.getMelds();
					if (melds != null && melds.Count > 0)
					{
						Suit suit = RoundController.Instance.mainState.getMelds().First().Value.getSuit();

						if (info.enCardType == suit || lstWaitingCards.FindIndex(card => card.enCardType == suit) == -1)
						{
							Move nextMove = new Move(new Player(nUserIndex), new Card(info.enCardType, info.nCardValue));
							PlayCard(nextMove);
						}
					}
                    else
                    {
						Move nextMove = new Move(new Player(nUserIndex), new Card(info.enCardType, info.nCardValue));
						PlayCard(nextMove);
					}
					
				}
			}
		}
		if(blFinishDecideTurn && nextMove != null)
		{
			PlayCard(nextMove);
			nextMove = null;
			blFinishDecideTurn = false;
		}
	}


	public void SetBidValue(BidValue value)
    {
		SetedBidValue = value;
		avatarCtrl.SetCallStatus(value.value, 0);
	}

	#region This is AI Part
	public IEnumerator DecideConfirm()
	{	
		if(USER_TYPE != USER_TYPE.MainPlayer)
			yield return new WaitForSeconds(1f);
		
		BidValue lastNormalCall = currentBidStatus.getLastNormalBid();
		Suit calledCardType = lastNormalCall != null ? lastNormalCall.suit : Suit.NONE;
		int nLastConfirmer = RoundController.Instance.currentBid.nLastUserIndex == 0 ? 3 : RoundController.Instance.currentBid.nLastUserIndex - 1;
		int nDisableNumber = 13;

		if (nUserIndex == nLastConfirmer)
        {
			foreach(UserGameCtrl user in CardGameController.Instance.lstUserCtrls)
            {
				if (user.SetedBidValue != null)
					nDisableNumber = nDisableNumber - user.SetedBidValue.value;
			}
		}

		CallType result = DesideCall(calledCardType, nDisableNumber);
		if (result.nCardValue > lastNormalCall.value) result.nCardValue = lastNormalCall.value;
		delBidConfirmEvent(nUserIndex, result.nCardValue);
	}

	public IEnumerator DesideBidCall()
	{
		CardGameController.Instance.lstUserCtrls[nUserIndex].avatarCtrl.StartTurnTimer(CardGameController.Instance.nWaitingTime, null);

		if (USER_TYPE == USER_TYPE.MainPlayer)
		{
			yield return new WaitForSeconds(1f);
			CardGameController.Instance.objBidCallWindow.GetComponent<CallDialogWindow>().ShowCallDialog(currentBidStatus, ClosedCallDialog);
			yield break;
        }
        else
        {
			yield return new WaitForSeconds(1f);
			DesideBidValue();
			yield break;
		}
	}

	public void DesideBidValue()
    {
		BidValue lastNormalCall = currentBidStatus.getLastNormalBid();
		Suit calledCardType = lastNormalCall != null ? lastNormalCall.suit : Suit.NONE;
		CallType result = DesideCall(Suit.NONE, 13, true);

		if (result.nCardValue < 4)
			CallBackForBidCall(BidType.PASS, 0, Suit.NONE);
		else
		{
			if (lastNormalCall == null)
				CallBackForBidCall(BidType.CALL, result.nCardValue, result.CardType);
			else
			{
				if (result.nCardValue >= lastNormalCall.value && result.CardType >= lastNormalCall.suit)
					CallBackForBidCall(BidType.CALL, result.nCardValue, result.CardType);
				else
				{
					CallBackForBidCall(BidType.PASS, 0, Suit.NONE);
				}
			}
		}
	}

	public CallType DesideCall(Suit called,int disableNumber = 13, bool isFirstCall = false)
	{
		CallType callResult = new CallType();

		if (isFirstCall)
		{
			callResult.CardType = DesideCallCardType();
		}
		else
		{
			callResult.CardType = called;
		}
		int callNumber = DesideCallNumberByCardType(callResult.CardType);
		if (disableNumber == callNumber)
			callNumber += 1;
		callResult.nCardValue = callNumber;
		return callResult;
		
	}

	public void DecideforDash(GlobalValue.DashCall dashCallEvent)
	{
		if (USER_TYPE != USER_TYPE.MainPlayer)
		{
			int nPosibleDashCount = 0;
			for (int nI = 0; nI < 4; nI++)
			{
				List<CardInfo> lstType = lstWaitingCards.FindAll(card => (int)card.enCardType == nI);
				if(lstType != null && lstType.Count > 0)
                {
					lstType.Sort((x, y) => x.nCardValue.CompareTo(y.nCardValue) * -1);
					if (12 - lstType[0].nCardValue >= lstType.Count)
						nPosibleDashCount++;
                }
                else
                {
					nPosibleDashCount++;
				}
			}
			dashCallEvent(nUserIndex, nPosibleDashCount == 4);
			isDashcallDeside = true;
		}
		else
		{
			this.delDashCallEvent = dashCallEvent;
			CardGameController.Instance.objDashCallWindow.SetActive(true);
		}
	}
	
	#endregion

	public void SetDashCaller()
    {
		isDashCaller = true;
		avatarCtrl.txTop_DCallBadge.gameObject.SetActive(true);

	}
	private GlobalValue.DashCall delDashCallEvent;
	private GlobalValue.BidCall delBidCallEvent;
	private GlobalValue.BidConfirm delBidConfirmEvent;
	private GlobalValue.TurnPlayed delTurnPlayed;
	private Bid currentBidStatus;

	public void PlayTurn(int waitingTime, GlobalValue.TurnPlayed turnPlayedEvent)
	{
		delTurnPlayed = turnPlayedEvent;
		if (USER_TYPE == USER_TYPE.AIPlayer)
        {
			avatarCtrl.StartTurnTimer(waitingTime,null);
			StartCoroutine(DesideTurnCard());
		}
        else
        {
			avatarCtrl.StartTurnTimer(waitingTime, delWatingTurnEnd);
			blCanPlayCard = true;
		}
	}

	public void TurnWaitingEndCallBack()
    {
		blCanPlayCard = false;
		StartCoroutine(DesideTurnCard(false));
	}
	public CardInfo MeldCard;
	public void PlayCard(Move nextMove)
	{
		RoundController.Instance.mainState.doMove(nextMove, false, true);
		Card discard = nextMove.GetCard();
		int nIndex = lstWaitingCards.FindIndex(finder => finder.enCardType == discard.getSuit() && finder.nCardValue == discard.getRank());
		CardInfo info = lstWaitingCards[nIndex];
		avatarCtrl.StopTimer();
		int nFindIndex = lstWaitingCards.FindIndex(0, (card) => card.nCardValue == info.nCardValue && card.enCardType == info.enCardType);
		if (nFindIndex != -1)
		{
			PlayingCard = lstWaitingCards[nFindIndex];
			lstWaitingCards[nFindIndex].SelectObject(PlayedCard);
			MeldCard = lstWaitingCards[nFindIndex];
			lstWaitingCards.RemoveAt(nFindIndex);
		}
	}

	public void PlayedCard(CardBaseInfo info)
	{
		blCanPlayCard = false;
		delTurnPlayed(nUserIndex, info);
	}

	public IEnumerator DesideTurnCard(bool isWaiting = true)
	{
		blFinishDecideTurn = false;
		ISMCTS aiNormal = new ISMCTS(RoundController.Instance.mainState, GlobalValue.m_nNormalAITime);
		ISMCTS aiCall = new ISMCTS(RoundController.Instance.mainState, GlobalValue.m_nCallerAITime);
		Move nextMove;
		Action<Move> callback = FinishDecideTurn;
		if (SetedBidValue.type != BidType.CALL)
		{
			aiNormal.setRootState(RoundController.Instance.mainState);
            System.Threading.Thread decideForNextTurn = new System.Threading.Thread(aiNormal.runThread);
			decideForNextTurn.Start(callback);
		}
		else
		{
			aiCall.setRootState(RoundController.Instance.mainState);
			System.Threading.Thread decideForNextTurn = new System.Threading.Thread(aiNormal.runThread);
			decideForNextTurn.Start(callback);
		}

		yield return null;
	}

	public void FinishDecideTurn(Move nextMove)
    {
		blFinishDecideTurn = true;
		this.nextMove = nextMove;
	}

	public IEnumerator DecideTurnPlay(int nWaitingTime)
	{
		float randSeconds = UnityEngine.Random.RandomRange(1f, 2f);
		yield return new WaitForSeconds(randSeconds);
		delBidConfirmEvent(nUserIndex, 2);
	}

	public void BidConfirmMainUser(Bid currentBid, GlobalValue.BidConfirm bidConfirmEvent)
	{
		CardGameController.Instance.objBidCallWindow.GetComponent<CallDialogWindow>().CloseWindow();
		delBidConfirmEvent = bidConfirmEvent;
		currentBidStatus = currentBid;
		if (isDashCaller)
		{
			delBidConfirmEvent(nUserIndex, 0);
		}
		else
		{
			StartCoroutine(DecideConfirm());
		}
	}

	public void BidConfirmStart(Bid currentBid, GlobalValue.BidConfirm bidConfirmEvent)
    {
		delBidConfirmEvent = bidConfirmEvent;
		currentBidStatus = currentBid;
		if (isDashCaller)
		{
			delBidConfirmEvent(nUserIndex, 0);
        }
        else
        {
			CardGameController.Instance.lstUserCtrls[nUserIndex].avatarCtrl.StartTurnTimer(CardGameController.Instance.nWaitingTime, null);

			if (USER_TYPE != USER_TYPE.MainPlayer)
				StartCoroutine(DecideConfirm());
			else
				CardGameController.Instance.objBidCallWindow.GetComponent<CallDialogWindow>().ShowCallDialog(currentBid, ClosedConfirmDialog, false);
		}
	}

	public void ClosedConfirmDialog(bool isPass, int nCallNumber, Suit callType)
	{
		delBidConfirmEvent(nUserIndex, nCallNumber);
		RoundController.Instance.currentBid.getLastNormalBid().suit = callType;
	}
	
	public void BidCallStart(Bid currentBid, GlobalValue.BidCall bidCallEvent)
    {
		delBidCallEvent = bidCallEvent;
		currentBidStatus = currentBid;

		if (isDashCaller)
        {
			CallBackForBidCall(BidType.DashCALL, 0, Suit.NONE);
        }
        else
        {
			StartCoroutine(DesideBidCall());
		}
	}

	public void CallBackForBidCall(BidType bidType, int callNumber, Suit callType)
    {
        switch (bidType)
        {
			case BidType.PASS:
			case BidType.DashCALL:
				delBidCallEvent(nUserIndex, new BidValue(bidType));
				break;
			case BidType.CALL:
				BidValue lastValue = currentBidStatus.getLastNormalBid();
				BidType newType =  BidType.CALL;
				if (lastValue != null && lastValue.value == callNumber && lastValue.suit == callType)
				{
					newType = BidType.WITH;
				}
				else
				{
					newType = BidType.CALL;
				}
				delBidCallEvent(nUserIndex, new BidValue(callNumber, callType, newType));
				break;
        }
	}

	public void ClosedCallDialog(bool isPass, int nCallNumber, Suit callType)
	{
		BidValue lastValue = currentBidStatus.getLastNormalBid();

		if ( delBidCallEvent != null)
        {
			BidType bidType = BidType.PASS;
			if (!isPass)
				bidType = BidType.CALL;
			CallBackForBidCall(bidType, nCallNumber, callType);
		}
	}

	


	public void DashCallWindowReulstTrue()
    {
		CardGameController.Instance.objDashCallWindow.SetActive(false);
		isDashcallDeside = true;
		delDashCallEvent(nUserIndex, true);
	}

	public void DashCallWindowReulstFalse()
	{
		CardGameController.Instance.objDashCallWindow.SetActive(false);
		isDashcallDeside = true;
		delDashCallEvent(nUserIndex, false);
	}

	public void StopDashCall(GlobalValue.DashCall dashCallEvent)
    {
		if(USER_TYPE == USER_TYPE.MainPlayer)
			CardGameController.Instance.objDashCallWindow.SetActive(false);
    }

	public CallType DesideCallType()
	{
		CallType callType = new CallType();
		if (lstWaitingCards != null || lstWaitingCards.Count == 0)
        {
			return null;
        }
        else
        {

        }
		return callType;
    }
	public List<int> GetCardInfos(Suit card)
	{
		return dicCards[card];

	}
	public Suit DesideCallCardType()
    {
		if(RoundController.Instance.enRoundType  == RoundType.NormalRound)
		{
			int maxCardNum = 0;
			Suit maxNumCardType = Suit.CLUB;

			foreach (Suit cardType in dicCards.Keys)
			{
				if (maxCardNum < dicCards[cardType].Count)
				{
					maxCardNum = dicCards[cardType].Count;
					maxNumCardType = cardType;
				}
			}

			return maxNumCardType;
        }
        else
        {
			switch (RoundController.Instance.enRoundType)
			{
				case RoundType.NONERound:
					return Suit.NONE;
				case RoundType.HEARTRound:
					return Suit.HEART;
				case RoundType.DIAMONDRound:
					return Suit.DIAMOND;
				case RoundType.SPADERound:
					return Suit.SPADE;
				case RoundType.CLUBRound:
					return Suit.CLUB;
				default: return Suit.NONE;
			}
		}
		
	}

	public int GetNumCanCall(List<int> arrCardValue)
    {
		float result = 0;
		
		if(GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9, 10, 11, 12 }))
			result += 4;
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9, 10, 11 }))
        {
			if (arrCardValue.Count >= 5) 
				result += 3;
			else if(arrCardValue.Count >= 3)
				result += 2;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9, 10,  12 }))
        {
			if (arrCardValue.Count >= 4)
				result += 3;
			else if (arrCardValue.Count >= 3)
				result += 2;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 10, 11, 12 }))
        {
			result += 3;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9, 10 }))
        {
			if (arrCardValue.Count >= 5)
				result += 2;
			else if (arrCardValue.Count >= 4)
				result += 1;
			else if (arrCardValue.Count >= 3)
				result += 0.5f;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9, 11 }))
        {
			if (arrCardValue.Count >= 5)
				result += 2;
			else if (arrCardValue.Count >= 4)
				result += 1;
			else if (arrCardValue.Count >= 2)
				result += 0.5f;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9, 12 }))
		{
			if (arrCardValue.Count >= 5)
				result += 3;
			else if (arrCardValue.Count >= 4)
				result += 2;
			else if (arrCardValue.Count >= 2)
				result += 1;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 10, 11 }))
		{
			if (arrCardValue.Count >= 5)
				result += 3;
			else if (arrCardValue.Count >= 4)
				result += 2;
			else if (arrCardValue.Count >= 2)
				result += 0.7f;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 10, 12 }))
		{
			if (arrCardValue.Count >= 5)
				result += 3;
			else if (arrCardValue.Count >= 4)
				result += 2;
			else if (arrCardValue.Count >= 2)
				result += 1;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 11, 12 }))
		{
			if (arrCardValue.Count >= 5)
				result += 3;
			else if (arrCardValue.Count >= 2)
				result += 2;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 9 }))
        {
			if (arrCardValue.Count >= 5)
				result += 1;
			else if (arrCardValue.Count >= 4)
				result += 0.5f;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 10 }))
		{
			if (arrCardValue.Count >= 4)
				result += 1;
			else if (arrCardValue.Count >= 3)
				result += 0.5f;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 11 }))
		{
			if (arrCardValue.Count >= 3)
				result += 1;
			else if (arrCardValue.Count >= 2)
				result += 0.5f;
		}
		else if (GlobalAction.fnFindCard(arrCardValue.ToArray(), new int[] { 12 }))
		{
			result += 1;
		}
		
		return (int)result;
    }
	

	public int DesideCallNumberByCardType(Suit calledCard)
	{
		int totalNum = 0;
		foreach (Suit cardType in dicCards.Keys)
        {
			totalNum += GetNumCanCall(dicCards[cardType]);
		}
		return totalNum;
	}

	


	private Dictionary<Suit, List<int>> dicCards = new Dictionary<Suit, List<int>>();


	public void InitCardList(List<Card> lstCardList)
    {
		lstUserCards = new List<CardInfo>();
		dicCards = new Dictionary<Suit, List<int>>();
		List<int> lstClub = new List<int>();
		List<int> lstDiamond = new List<int>();
		List<int> lstHeart = new List<int>();
		List<int> lstSpade = new List<int>();

		foreach(Card card in lstCardList)
        {
			switch (card.getSuit())
			{
				case Suit.CLUB:
					lstClub.Add(card.getRank());
					break;
				case Suit.DIAMOND:
					lstDiamond.Add(card.getRank());
					break;
				case Suit.HEART:
					lstHeart.Add(card.getRank());
					break;
				case Suit.SPADE:
					lstSpade.Add(card.getRank());
					break;
			}
		}

		lstClub.Sort();
		lstClub.Reverse();
		dicCards.Add(Suit.CLUB, lstClub);

		lstDiamond.Sort();
		lstDiamond.Reverse();
		dicCards.Add(Suit.DIAMOND, lstDiamond);

		lstHeart.Sort();
		lstHeart.Reverse();
		dicCards.Add(Suit.HEART, lstHeart);

		lstSpade.Sort();
		lstSpade.Reverse();
		dicCards.Add(Suit.SPADE, lstSpade);

		Suit cardType = Suit.CLUB;
		foreach (int cardValue in lstClub)
		{
			AddCardInfo(cardValue, cardType);
		}
		cardType = Suit.DIAMOND;
		foreach (int cardValue in lstDiamond)
		{
			AddCardInfo(cardValue, cardType);
		}
		cardType = Suit.HEART;
		foreach (int cardValue in lstHeart)
		{
			AddCardInfo(cardValue, cardType);
		}
		cardType = Suit.SPADE;
		foreach (int cardValue in lstSpade)
		{
			AddCardInfo(cardValue, cardType);
		}
	}

	public void InitCardList(List<int> uniqueRandList)
    {
		lstUserCards = new List<CardInfo>();
		dicCards = new Dictionary<Suit, List<int>>();

		List<int> lstClub = new List<int>();
		List<int> lstDiamond = new List<int>();
		List<int> lstHeart = new List<int>();
		List<int> lstSpade = new List<int>();

		foreach (int rand in uniqueRandList)
		{
			int cardValue = rand % GlobalValue.Instance.CardNumber;
			Suit type = (Suit)(rand / GlobalValue.Instance.CardNumber);
			switch (type)
			{
				case Suit.CLUB:
					lstClub.Add(cardValue);
					break;
				case Suit.DIAMOND:
					lstDiamond.Add(cardValue);
					break;
				case Suit.HEART:
					lstHeart.Add(cardValue);
					break;
				case Suit.SPADE:
					lstSpade.Add(cardValue);
					break;
			}
		}

		lstClub.Sort();
		lstClub.Reverse();
		dicCards.Add(Suit.CLUB, lstClub);

		lstDiamond.Sort();
		lstDiamond.Reverse();
		dicCards.Add(Suit.DIAMOND, lstDiamond);

		lstHeart.Sort();
		lstHeart.Reverse();
		dicCards.Add(Suit.HEART, lstHeart);

		lstSpade.Sort();
		lstSpade.Reverse();
		dicCards.Add(Suit.SPADE, lstSpade);

		Suit cardType = Suit.CLUB;
		foreach (int cardValue in lstClub)
		{
			AddCardInfo(cardValue, cardType);
		}
		cardType = Suit.DIAMOND;
		foreach (int cardValue in lstDiamond)
		{
			AddCardInfo(cardValue, cardType);
		}
		cardType = Suit.HEART;
		foreach (int cardValue in lstHeart)
		{
			AddCardInfo(cardValue, cardType);
		}
		cardType = Suit.SPADE;
		foreach (int cardValue in lstSpade)
		{
			AddCardInfo(cardValue, cardType);
		}

	}

	public void AddCardInfo(int cardValue, Suit cardType)
	{
		Vector3 v3StartPosition = trCardWaiSPos.localPosition;
		bool isMainCard = false;
		if (USER_TYPE == USER_TYPE.MainPlayer)
        {
			int cardHeight = (int)(GlobalValue.Instance.m_Root.activeHeight / 5);
			int nI = lstUserCards.Count;
			int nV = Mathf.Abs(6 - nI);
			float stepX = (trCardWaiEPos.localPosition.x - trCardWaiSPos.localPosition.x) / GlobalValue.Instance.CardNumber;

			v3StartPosition.y = v3StartPosition.y + cardHeight / 3 - nV * cardHeight / 30;
			v3StartPosition.x = trCardWaiSPos.localPosition.x + nI * stepX;
			v3StartPosition.z = -0.1f*nI;
			isMainCard = true;
		}

		GameObject card = new GameObject("Card_" + cardValue + "_" + cardType.ToString());
		CardInfo cardInfo = card.AddComponent<CardInfo>();
		cardInfo.SpawnCardInfo(this.transform, trCardSpawnPos, v3StartPosition, trCardPlayPoint.localPosition,  cardValue, (Suit)cardType, isMainCard);
		lstUserCards.Add(cardInfo);
	}

	public IEnumerator StartArrangement()
	{
		yield return new WaitForEndOfFrame();
		lstWaitingCards = new List<CardInfo>();
		foreach (CardInfo cardinfo in lstUserCards)
        {
			yield return new WaitForSeconds(0.2f);
			cardinfo.ChangeState(CardLogicState.PLAYING);
			lstWaitingCards.Add(cardinfo);
		}
	}

}
