using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallDialogWindow : MonoBehaviour
{
    public List<CallNodeInfo> lstNodeInfos;
    public List<CallNodeInfo> lstFlowerNodeInfos;
    public int nCurCallNumberNode;
    public int nCurCallFlowerNode;
    public Action<bool, int, Suit> CloseCallBack;
    public GameObject btnCancel, btnCall, btnWith, btnConfirm;

    private Bid CurrentStatus;

    private BidValue lastBid;
    private bool isCallWindow;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(InitWindow());
    }

    private void OnEnable()
    {
        this.GetComponent<TweenScale>().Play(true);
    }

    private void OnDisable()
    {
        this.GetComponent<TweenScale>().ResetToBeginning();
    }

    IEnumerator SelectBack()
    {
        yield return new WaitForEndOfFrame();

        if (lastBid != null)
        {
            if (isCallWindow)
            {
                if (lastBid.value == nCurCallNumberNode)
                {
                    if (nCurCallFlowerNode < ((int)lastBid.suit))
                    {
                        lstFlowerNodeInfos[nCurCallFlowerNode].HideBack();
                        nCurCallFlowerNode = ((int)lastBid.suit);
                    }
                    for (int nI = 0; nI < ((int)lastBid.suit); nI++)
                    {
                        lstFlowerNodeInfos[nI].Disable();
                    }
                }
                else if(lastBid.value < nCurCallNumberNode)
                {
                    if(RoundController.Instance.enRoundType == RoundType.NormalRound)
                    {
                        for (int nI = 0; nI < ((int)lastBid.suit); nI++)
                        {
                            lstFlowerNodeInfos[nI].Enable();
                        }
                    }
                    btnCall.SetActive(true);
                    btnWith.SetActive(false);
                }
            
                if (nCurCallFlowerNode == ((int)lastBid.suit) && lastBid.value == nCurCallNumberNode)
                {
                    btnCall.SetActive(false);
                    btnWith.SetActive(true);
                }
                else
                {
                    btnCall.SetActive(true);
                    btnWith.SetActive(false);
                }
            }
        }
        lstNodeInfos[nCurCallNumberNode].ShowBack();
        try {

            lstFlowerNodeInfos[nCurCallFlowerNode].ShowBack();
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowCallDialog(Bid currentBidStatus, Action<bool, int, Suit> closeCallback, bool isCall = true)
    {
        this.gameObject.SetActive(true);
        isCallWindow = isCall;

        CurrentStatus = currentBidStatus;
        CloseCallBack = closeCallback;
        lastBid = CurrentStatus.getLastNormalBid();

        for (int nI = 0; nI < lstNodeInfos.Count; nI++)
        {
            lstNodeInfos[nI].Enable();
        }
        for (int nI = 0; nI < lstFlowerNodeInfos.Count; nI++)
        {
            lstFlowerNodeInfos[nI].Enable();
        }

        for (int nI = 0; nI < lstNodeInfos.Count; nI++)
        {
            lstNodeInfos[nI].HideBack();
        }
        for (int nI = 0; nI < lstFlowerNodeInfos.Count; nI++)
        {
            lstFlowerNodeInfos[nI].HideBack();
        }

        if (isCallWindow)
        {
            btnCall.SetActive(true);
            btnCancel.SetActive(true);
            btnConfirm.SetActive(false);
            for (int nI = 0; nI < 4; nI++)
            {
                lstNodeInfos[nI].Disable();
            }

            if (lastBid != null)
            {
                nCurCallNumberNode = lastBid.value;
                nCurCallFlowerNode = ((int)lastBid.suit);
                for (int nI = 4; nI < nCurCallNumberNode; nI++)
                {
                    lstNodeInfos[nI].Disable();
                }
            }
            else
            {
                nCurCallNumberNode = 4;
                if(RoundController.Instance.enRoundType == RoundType.NormalRound)
                    nCurCallFlowerNode = ((int)Suit.CLUB);
                else
                {
                    nCurCallFlowerNode = (int)GetCorrectSuit(RoundController.Instance.enRoundType);
                }
            }
            DisableFlowerNode(RoundController.Instance.enRoundType);
        }
        else
        {
            int nLastConfirmer = RoundController.Instance.currentBid.nLastUserIndex == 0 ? 3 : RoundController.Instance.currentBid.nLastUserIndex - 1;
            int nDisableNumber = 13;

            if (0 == nLastConfirmer)
            {
                foreach (UserGameCtrl user in CardGameController.Instance.lstUserCtrls)
                {
                    if (user.SetedBidValue != null)
                        nDisableNumber = nDisableNumber - user.SetedBidValue.value;
                }
            }

            btnWith.SetActive(false);
            btnCall.SetActive(false);
            btnCancel.SetActive(false);
            btnConfirm.SetActive(true);
            if (lastBid != null)
            {
                nCurCallNumberNode = lastBid.value;
                nCurCallFlowerNode = ((int)lastBid.suit);
                
                for (int nI = 0; nI < lstNodeInfos.Count; nI++)
                {
                    if(RoundController.Instance.currentBid.nLastUserIndex == 0)
                    {
                        if (nI < nCurCallNumberNode)
                            lstNodeInfos[nI].Disable();
                        else
                            lstNodeInfos[nI].Enable();
                    }
                    else
                    {
                        if (nI > nCurCallNumberNode)
                            lstNodeInfos[nI].Disable();
                        else
                            lstNodeInfos[nI].Enable();
                    }
                    
                }
                if(nDisableNumber != 13)
                    lstNodeInfos[nDisableNumber].Disable();
                /*
                for (int nI = 0; nI < lstFlowerNodeInfos.Count; nI++)
                {
                    if(nI != nCurCallFlowerNode)
                        lstFlowerNodeInfos[nI].Disable();
                    else
                        lstFlowerNodeInfos[nI].Enable();
                }
                */
            }
        }
        StartCoroutine(SelectBack());
    }

    public Suit GetCorrectSuit(RoundType roundType)
    {
        Suit enableSuit = Suit.CLUB;
        switch (roundType)
        {
            case RoundType.NONERound:
                enableSuit = Suit.NONE;
                break;
            case RoundType.HEARTRound:
                enableSuit = Suit.HEART;
                break;
            case RoundType.DIAMONDRound:
                enableSuit = Suit.DIAMOND;
                break;
            case RoundType.SPADERound:
                enableSuit = Suit.SPADE;
                break;
            case RoundType.CLUBRound:
                enableSuit = Suit.CLUB;
                break;
        }
        return enableSuit;
    }

    public void DisableFlowerNode(RoundType roundType)
    {
        if (roundType == RoundType.NormalRound)
            return;
        Suit enableSuit = GetCorrectSuit(roundType);
        
        for (int nI = 0; nI < lstFlowerNodeInfos.Count; nI++)
        {
            if (nI != ((int)enableSuit))
                lstFlowerNodeInfos[nI].Disable();
            else
                lstFlowerNodeInfos[nI].Enable();
        }
    }

    public void ClickConfirm()
    {
        this.gameObject.SetActive(false);
        if (CloseCallBack != null)
            CloseCallBack(true, nCurCallNumberNode, (Suit)nCurCallFlowerNode);
    }

    public void ClickPass()
    {
        this.gameObject.SetActive(false);
        if (CloseCallBack != null)
            CloseCallBack(true, 0, 0);
    }

    public void ClickWith()
    {
        this.gameObject.SetActive(false);
        if (CloseCallBack != null)
            CloseCallBack(false, nCurCallNumberNode, (Suit)nCurCallFlowerNode);
    }

    public void selectCallNumber()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (((Input.GetMouseButtonDown(0) || Input.touchCount > 0)))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                GameObject objHit = hit.collider.gameObject;
                if (objHit.tag == "CallNode")
                {
                    CallNodeInfo info = objHit.GetComponent<CallNodeInfo>();
                    if (info.isEnabled)
                    {
                        if (info.isCallNumber)
                        {
                            lstNodeInfos[nCurCallNumberNode].HideBack();
                            nCurCallNumberNode = info.nCallNumber;
                        }
                        else
                        {
                            lstFlowerNodeInfos[nCurCallFlowerNode].HideBack();
                            nCurCallFlowerNode = ((int)info.FlowerType);
                        }
                        StartCoroutine(SelectBack());
                    }
                }
            }
        }
    }
}
