using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoundStep
{
    BidStep,
    PlayStep
}

public enum RoundType
{
    NormalRound,

    NONERound,
    
    SPADERound,
    
    HEARTRound,
    
    DIAMONDRound,
    
    CLUBRound 
}


public enum CardGameType
{
    /// <summary>
    /// 18 rounds: 13 normal rounds + 5 color rounds
    /// </summary>
    FullBola,

    /// <summary>
    /// 10 rounds: 5 normal rounds + 5 color rounds
    /// </summary>
    MiniBola,

    /// <summary>
    /// 5 rounds: 5 normal rounds
    /// </summary>
    MicroBola,

    /// <summary>
    /// 18 rounds: FullBola | 2 vs 2
    /// </summary>
    EstimationCouple,

    /// <summary>
    /// 18 rounds: FullBola | old school score calculatons
    /// </summary>
    EstimationClassic
}

public enum WaitingType
{
    /// <summary>
    /// Inifinity way
    /// </summary>
    Infinity,

    /// <summary>
    /// 30s
    /// </summary>
    Thertysecond,

    /// <summary>
    /// 25s
    /// </summary>
    TwFivesecond,

    /// <summary>
    /// 20s
    /// </summary>
    Twentysecond,

    /// <summary>
    /// 15s
    /// </summary>
    Fiftensecond,

    /// <summary>
    /// 10s
    /// </summary>
    tensecond
}


public enum BidType
{
    DashCALL = 0,
    CALL = 1,
    PASS = 2,
    WITH = 3,
}

public enum GameEventType
{
    CallEvent,
    DashEvent,
    WithEvent,
    AvoidEvebt,
    doubleRisckEvent,
    RiskEvent
}

