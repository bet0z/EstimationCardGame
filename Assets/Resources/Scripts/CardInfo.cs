using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CardPosState
{
	AT_SPAWNPOINT,
	GO_TO_STARTPOINT,
	AT_STARTPOINT,
	GO_TO_WAITPOINT,
	AT_WAITPOINT,
	GO_TO_PLAYPOINT,
	AT_PLAYPOINT,
	GO_TO_ENDPOINT,
	AT_ENDPOINT
}

public enum CardLogicState
{
	WAITING,
	PLAYING
}


public class CardBaseInfo
{
	public int nCardNum;
	public Suit cardType;
	public CardBaseInfo(int cardNum, Suit type)
    {
		nCardNum = cardNum;
		cardType = type;
	}
}

public class CardInfo : MonoBehaviour
{
	public float uncoverTime = 12.0f;
	public float speed = 2500f;
	public GameObject cardFront, cardBack;
	public int nCardValue;
	public Suit enCardType;

	private Vector3 v3StartPos, v3PlayPos, v3EndPos;
	private int cardHeight = -1, cardWidth = -1;

	private CardPosState cardPosState;
	private CardLogicState cardState;
	private bool IsMineCard;
	private CardBaseInfo baseInfo;

	public void SpawnCardInfo(Transform parent, Transform spawnPosition, Vector3 startPos, Vector3 playPos, int cardValue, Suit cardType, bool isMineCard = false)
	{
		IsMineCard = isMineCard;
		cardHeight = (int)(GlobalValue.Instance.m_Root.activeHeight / 5);
		cardWidth = (int)(cardHeight / 1.4f);
		v3StartPos = startPos;
		v3PlayPos = playPos;
		nCardValue = cardValue;
		enCardType = cardType;

		baseInfo = new CardBaseInfo(cardValue, cardType);
		cardFront = new GameObject("CardFront");
		cardBack = new GameObject("CardBack");
		cardFront.transform.parent = this.transform; // make front child of card
		cardBack.transform.parent = this.transform; // make back child of card

		int nResourceIndex = ((int)cardType) * 13 + cardValue -1;
		// front (motive)
		cardFront.AddComponent<UI2DSprite>();
		cardFront.GetComponent<UI2DSprite>().sprite2D = GlobalValue.Instance.m_arrCardFrontSprites[nResourceIndex];
		cardFront.GetComponent<UI2DSprite>().depth = -10;

		// back
		cardBack.AddComponent<UI2DSprite>();
		cardBack.GetComponent<UI2DSprite>().sprite2D = GlobalValue.Instance.m_arrCardBackSprites[0];
		cardBack.GetComponent<UI2DSprite>().depth = 10;


		cardFront.GetComponent<UI2DSprite>().width = cardWidth;
		cardFront.GetComponent<UI2DSprite>().height = cardHeight;
		cardBack.GetComponent<UI2DSprite>().width = cardWidth;
		cardBack.GetComponent<UI2DSprite>().height = cardHeight;

		this.tag = "Card";
		this.transform.parent = parent;
		this.transform.position = spawnPosition.position;

		//Rigidbody rigid = this.gameObject.AddComponent<Rigidbody>();
		//rigid.useGravity = false;

		this.transform.localScale = new Vector3(1f, 1f, 1f);
		BoxCollider colider = this.gameObject.AddComponent<BoxCollider>();
		if (colider != null)
			colider.size = new Vector3(cardWidth, cardHeight, 2);
		DragRotator rotator = this.gameObject.AddComponent<DragRotator>();
		DragRotatorInfo info = new DragRotatorInfo();
		info.m_PitchInfo = new DragRotatorAxisInfo(10f, 40f, 50f, 2f);
		info.m_RollInfo = new DragRotatorAxisInfo(10f, 40f, 50f, 2f);
		rotator.SetInfo(info);
		cardPosState = CardPosState.AT_SPAWNPOINT;
	}

	void Start()
	{
		cardState = CardLogicState.WAITING;
	}

	// Update is called once per frame
	void Update()
	{
		if (cardState == CardLogicState.PLAYING)
		{
			switch (cardPosState)
			{
				case CardPosState.AT_SPAWNPOINT:
					cardPosState = CardPosState.GO_TO_STARTPOINT;
					break;
				case CardPosState.GO_TO_STARTPOINT:
					if (this.transform.localPosition != v3StartPos)
					{
						float step = speed * Time.deltaTime; // calculate distance to move
						this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, v3StartPos, step);
					}
					else
					{
						StartCoroutine(uncoverCard(true));
						cardPosState = CardPosState.AT_STARTPOINT;
						cardState = CardLogicState.WAITING;
					}
					break;
				case CardPosState.GO_TO_PLAYPOINT:
					if (this.transform.localPosition != v3PlayPos)
					{
						float step = 2 * speed * Time.deltaTime; // calculate distance to move
						this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, v3PlayPos, step);
					}
					else
					{
						cardPosState = CardPosState.AT_PLAYPOINT;
						cardState = CardLogicState.WAITING;
						if (CardPlayedCallBack != null)
							CardPlayedCallBack(baseInfo);
					}
					break;
				case CardPosState.AT_PLAYPOINT:
					cardPosState = CardPosState.GO_TO_ENDPOINT;
					break;
				case CardPosState.GO_TO_ENDPOINT:
					if (this.transform.localPosition != v3EndPos)
					{
						float step = speed * Time.deltaTime; // calculate distance to move
						this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, v3EndPos, step);
					}
					else
					{
						cardPosState = CardPosState.AT_ENDPOINT;
						cardState = CardLogicState.WAITING;
						this.gameObject.SetActive(false);
					}
					break;
			}
		}

	}
	[HideInInspector]
	public UserGameCtrl.CardPlayed CardPlayedCallBack;

	public CardBaseInfo GetBaseInfo()
	{
		return baseInfo;
	}
	public void SelectObject(UserGameCtrl.CardPlayed callBack)
	{
		switch (cardPosState)
		{
			case CardPosState.AT_STARTPOINT:
				cardPosState = CardPosState.GO_TO_PLAYPOINT;
				cardState = CardLogicState.PLAYING;
				this.gameObject.SetActive(true);
				CardPlayedCallBack = callBack;
				break;
		}
	}


	public void SetEndPoint(Transform trEndPos)
    {
		v3EndPos = trEndPos.localPosition;
    }
	public void ChangeState(CardLogicState state)
    {
		cardState = state;
	}
	public IEnumerator uncoverCard(bool uncover)
	{
		for (int i = 0; i < this.transform.childCount; i++)
		{
            // reverse sorting order to show the otherside of the card
            // otherwise you would still see the same sprite because they are sorted 
            // by order not distance (by default)
			Transform c = this.transform.GetChild(i);
			c.GetComponent<UI2DSprite>().depth *= -1;
		}
		if (!IsMineCard)
		{
			this.gameObject.SetActive(false);
		}
		yield return 0;
	}
}
