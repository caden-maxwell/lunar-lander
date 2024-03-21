using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LunarLander.Storage;

[Serializable]
[DataContract(Name = "GameScore")]
public class GameScore : IComparable<GameScore>
{
    public GameScore() { }

    public GameScore(float score)
    {
        this.Name = "Player1";
        this.Score = score;
        this.TimeStamp = DateTime.Now;
    }

    [DataMember()]
    public string Name { get; set; }
    [DataMember()]
    public float Score { get; set; }
    [DataMember()]
    public DateTime TimeStamp { get; set; }

    public int CompareTo(GameScore other)
    {
        return other.Score > Score ? 1 : -1;
    }
}

