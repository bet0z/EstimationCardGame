using UnityEngine;
using System.Collections;

public class UserAvatarCtrl : MonoBehaviour {

	#region PUBLIC_VARIABLES
	public GameObject objCoupleModeRound, objNormalRound;
	public UITexture timerTexture;
	public UITexture txAvatar;
	public UILabel labGameStatus;

	public GameObject objGameStatus;
	public GameObject objRoundWinnerEffect;
	public UILabel labName, labScoreNum;
	public ScoreDisplayControl scoreDisplay;

	public UITexture txEvent_CallBadge;
	public UITexture txEvent_DCallBadge;
	public UITexture txEvent_RBadge;
	public UITexture txEvent_DoubleRBadge;
	public UITexture txEvent_AvoidBadge;
	public UITexture txEvent_WithBadge;


	/*
	txEvent_CallBadge
	txEvent_DCallBadge
	txEvent_RBadge
	txEvent_DoubleRBadge
	txEvent_AvoidBadge
	txEvent_WithBadge
	 */

	public UITexture txTop_CallBadge;
	public UITexture txTop_DCallBadge;
	public UITexture txTop_PassBadge;
	public UITexture txTop_WithBadge;

	public CallStatus callStatus;

	public bool blStartTimer = false;
	public float maxTime;
	private float rate, i;


	#endregion
	public void InitAvatar(bool isCoupleMode = false)
    {
		i = 0;
		labGameStatus.text ="0 / 0";
		objGameStatus.SetActive(false);
		timerTexture.gameObject.SetActive(false);
		blStartTimer = false;
		callStatus.Disable();
		DisabelBidStatus();
		DisableRoundWinner();
		DisableEventStatus();
		if(isCoupleMode)
		{
			objNormalRound.SetActive(false);
			objCoupleModeRound.SetActive(true);
        }
	}

	public void VisibleRoundWinner()
	{
		objRoundWinnerEffect.gameObject.SetActive(true);
    }

	public void DisableRoundWinner()
	{
		objRoundWinnerEffect.gameObject.GetComponent<TweenRotation>().ResetToBeginning();
		objRoundWinnerEffect.gameObject.SetActive(false);
	}

	public void DisabelBidStatus()
    {
		txTop_CallBadge.gameObject.SetActive(false);
		txTop_DCallBadge.gameObject.SetActive(false);
		txTop_PassBadge.gameObject.SetActive(false);
		txTop_WithBadge.gameObject.SetActive(false);
	}

	public void DisableEventStatus()
    {
		txEvent_CallBadge.gameObject.SetActive(false);
		txEvent_DCallBadge.gameObject.SetActive(false);
		txEvent_RBadge.gameObject.SetActive(false);
		txEvent_DoubleRBadge.gameObject.SetActive(false);
		txEvent_AvoidBadge.gameObject.SetActive(false);
		txEvent_WithBadge.gameObject.SetActive(false);
	}

	public GameEventType gameEventType;
	public BidType bidType;
	public void InitScoreNum()
    {
		labScoreNum.text = "0";
		scoreDisplay.labScore.text = "0";
		scoreDisplay.gameObject.SetActive(false);
	}
	public void SetScoreNumber(int nResult)
    {
		labScoreNum.text = nResult.ToString();
		scoreDisplay.SetText(nResult);
    }

	public void SetBidEventStatus(BidType type)
	{
		this.bidType = type;
		switch (type)
		{
			case BidType.CALL:
				txEvent_CallBadge.gameObject.SetActive(true);
				txEvent_DCallBadge.gameObject.SetActive(false);
				txEvent_RBadge.gameObject.SetActive(false);
				txEvent_DoubleRBadge.gameObject.SetActive(false);
				txEvent_AvoidBadge.gameObject.SetActive(false);
				txEvent_WithBadge.gameObject.SetActive(false);
				break;
			case BidType.DashCALL:
				txEvent_CallBadge.gameObject.SetActive(false);
				txEvent_DCallBadge.gameObject.SetActive(true);
				txEvent_RBadge.gameObject.SetActive(false);
				txEvent_DoubleRBadge.gameObject.SetActive(false);
				txEvent_AvoidBadge.gameObject.SetActive(false);
				txEvent_WithBadge.gameObject.SetActive(false);
				break;
			case BidType.WITH:
				txEvent_CallBadge.gameObject.SetActive(false);
				txEvent_DCallBadge.gameObject.SetActive(false);
				txEvent_RBadge.gameObject.SetActive(false);
				txEvent_DoubleRBadge.gameObject.SetActive(false);
				txEvent_AvoidBadge.gameObject.SetActive(false);
				txEvent_WithBadge.gameObject.SetActive(true);
				break;
		}
	}

	public void SetBidEventStatus(GameEventType type)
	{
		this.gameEventType = type;
		switch (type)
		{
			case GameEventType.RiskEvent:
				txEvent_RBadge.gameObject.SetActive(true);
				txEvent_DoubleRBadge.gameObject.SetActive(false);
				break;
			case GameEventType.doubleRisckEvent:
				txEvent_RBadge.gameObject.SetActive(false);
				txEvent_DoubleRBadge.gameObject.SetActive(true);
				break;
			case GameEventType.AvoidEvebt:
				txEvent_CallBadge.gameObject.SetActive(false);
				txEvent_DCallBadge.gameObject.SetActive(false);
				txEvent_RBadge.gameObject.SetActive(false);
				txEvent_DoubleRBadge.gameObject.SetActive(false);
				txEvent_AvoidBadge.gameObject.SetActive(true);
				txEvent_WithBadge.gameObject.SetActive(false);
				break;
		}
	}

	public void SetBidStatus(BidType type)
    {
		objGameStatus.SetActive(false);
		switch (type)
        {
			case BidType.CALL:
				txTop_CallBadge.gameObject.SetActive(true);
				txTop_DCallBadge.gameObject.SetActive(false);
				txTop_PassBadge.gameObject.SetActive(false);
				txTop_WithBadge.gameObject.SetActive(false);
				break;

			case BidType.DashCALL:
				txTop_CallBadge.gameObject.SetActive(false);
				txTop_DCallBadge.gameObject.SetActive(true);
				txTop_PassBadge.gameObject.SetActive(false);
				txTop_WithBadge.gameObject.SetActive(false);
				break;
			case BidType.PASS:
				txTop_CallBadge.gameObject.SetActive(false);
				txTop_DCallBadge.gameObject.SetActive(false);
				txTop_PassBadge.gameObject.SetActive(true);
				txTop_WithBadge.gameObject.SetActive(false);
				break;
			case BidType.WITH:
				txTop_CallBadge.gameObject.SetActive(false);
				txTop_DCallBadge.gameObject.SetActive(false);
				txTop_PassBadge.gameObject.SetActive(false);
				txTop_WithBadge.gameObject.SetActive(true);
				break;
		}
    }

	public void SetUserName(string userName)
    {
		labName.text = userName;

	}
	public void SetCallStatus(int totalCount, int currentCount)
    {
		DisabelBidStatus();
		if (!objGameStatus.active)
			objGameStatus.SetActive(true);
		labGameStatus.text = currentCount + " / " + totalCount;
	}

	#region UNITY_CALLBACKS
	void OnEnable()
	{
		timerTexture.gameObject.SetActive(false);
		blStartTimer = false;
		//StartTurnTimer(maxTime);//Start When Object will Enable.
	}

	#endregion
	UserGameCtrl.WatingTurnEnd waitngEndCallBack;
	#region PUBLIC_METHODS
	public void StartTurnTimer(float maxTime, UserGameCtrl.WatingTurnEnd callBackWatingEnd)//Call This Method When You Want to start Timer.
	{
		waitngEndCallBack = callBackWatingEnd;
		timerTexture.gameObject.SetActive(true);
		this.maxTime = maxTime;//Set Max Time.
		timerTexture.color = new Color(255, 243, 0, 255);// Color.green;
		i = 0;
		rate = 1 / maxTime;
		blStartTimer = true;
		StartCoroutine(StartTimer());
	}
	#endregion


	void Update()
	{
        if (blStartTimer)
        {
			i += rate * Time.deltaTime;
			timerTexture.fillAmount = Mathf.Lerp(1, 0, i);
			if (i > 0.65f)
				timerTexture.color = Color.red;
		}

	}

	#region PRIVATE_METHODS
	private IEnumerator StartTimer()
	{
		int nWaitngSecond = 0;
		while(nWaitngSecond < maxTime)
        {
			yield return new WaitForSeconds(1.0f);
			nWaitngSecond ++;
			if (!blStartTimer)
				break;
		}
		if (waitngEndCallBack != null && blStartTimer)
			waitngEndCallBack();

		blStartTimer = false;
	}

	public void StopTimer()//Call This Method To Stop Timer.
	{
		blStartTimer = false;
		timerTexture.gameObject.SetActive(false);
	}
	#endregion
}
