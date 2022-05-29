using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GlobalAction
{
    public static int GetBitCallNumber(List<CardInfo> lstCardInfos)
    {
        int nReuslt = 0;
        for (int nI = 0; nI < 4; nI++)
        {
            List<CardInfo> lstType = lstCardInfos.FindAll(card => (int)card.enCardType == nI);
            if (lstType != null && lstType.Count > 0)
            {
                lstType.Sort((x, y) => x.nCardValue.CompareTo(y.nCardValue) * -1);
                
            }
        }
        return nReuslt;
    }

    public static Sprite[] GetCardFrontSprites(int cardType)
    {
        return GlobalValue.Instance.m_arrCardFrontSprites;
    }

    public static Sprite[] GetCardBackSprites(int cardType)
    {
        return GlobalValue.Instance.m_arrCardBackSprites;
    }

    public static int GetRoundCount(CardGameType type)
    {
        switch (type)
        {
            case CardGameType.FullBola:
            case CardGameType.EstimationCouple:
            case CardGameType.EstimationClassic:
                return 18;
            case CardGameType.MiniBola:
                return 10;
            case CardGameType.MicroBola:
                return 5;
            default:
                return 18;
        }
    }

    public static bool fnFindCard(int[] arrCardInfo, int[] values)
    {
        int nSameCount = 0;
        foreach(int cardVal in values)
        {
            int index = arrCardInfo.ToList().FindIndex(card => card == cardVal);
            if (index != -1)
                nSameCount++;
        }
        return nSameCount == values.Length;
    }

}
