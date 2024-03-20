using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LunarLander.Storage;

[Serializable]
[DataContract(Name = "GameScore")]
public class GameScore
{
    public GameScore() { }

    /// <summary>
    /// Overloaded constructor used to create an object for long term storage
    /// </summary>
    /// <param name="score"></param>
    /// <param name="level"></param>
    public GameScore(uint score, ushort level)
    {
        this.Name = "Player1";
        this.Score = score;
        this.Level = level;
        this.TimeStamp = DateTime.Now;

        //keys.Add(1, "one");
        //keys.Add(2, "two");
    }

    [DataMember()]
    public string Name { get; set; }
    [DataMember()]
    public uint Score { get; set; }
    [DataMember()]
    public ushort Level { get; set; }
    [DataMember()]
    public DateTime TimeStamp { get; set; }
    //[DataMember()]
    //public Dictionary<int, String> keys = new();
}

