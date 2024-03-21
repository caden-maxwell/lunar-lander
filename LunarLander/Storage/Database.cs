using LunarLander.Input;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace LunarLander.Storage;

public enum ActionEnum
{
    Thrust,
    RotateClockwise,
    RotateCounterClockwise
}

public class Database
{
    private bool m_saving = false;
    private bool m_loading = false;
    private Dictionary<int, List<GameScore>> m_loadedScores = new();
    public Dictionary<int, List<GameScore>> Scores
    {
        get
        {
            for (int i = 1; i <= 5; i++)
                m_loadedScores[i].Sort();
            return m_loadedScores;
        }
    }

    private Dictionary<ActionEnum, Keys> m_loadedActions = new();
    public Dictionary<ActionEnum, Keys> Actions
    {
        get
        {
            return m_loadedActions;
        }
    }

    public Database()
    {
        // Make sure defaults exist

        LoadScores();
        for (int i = 1; i <= 5; i++)
            if (!m_loadedScores.ContainsKey(i))
                m_loadedScores.Add(i, new());

        LoadKeys();
        if (!m_loadedActions.ContainsKey(ActionEnum.Thrust))
            SaveAction(ActionEnum.Thrust, Keys.Up);
        if (!m_loadedActions.ContainsKey(ActionEnum.RotateClockwise))
            SaveAction(ActionEnum.RotateClockwise, Keys.Right);
        if (!m_loadedActions.ContainsKey(ActionEnum.RotateCounterClockwise))
            SaveAction(ActionEnum.RotateCounterClockwise, Keys.Left);
    }

    public void SaveScore(int level, GameScore score)
    {
        lock (this)
        {
            if (!m_saving)
            {
                m_saving = true;
                var result = FinalizeSaveAsync(level, score);
                result.Wait();
            }
        }
    }

    private async Task FinalizeSaveAsync(int level, GameScore score)
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Create);
                    if (fs != null)
                    {
                        m_loadedScores[level].Add(score);
                        DataContractJsonSerializer mySerializer = new(typeof(Dictionary<int, List<GameScore>>));
                        mySerializer.WriteObject(fs, m_loadedScores);
                    }
                }
                catch (IsolatedStorageException)
                {
                    Debug.WriteLine("IsolatedStorageException");
                }
            }

            this.m_saving = false;
        });
    }

    /// <summary>
    /// Demonstrates how to deserialize an object from storage device
    /// </summary>
    public void LoadScores()
    {
        lock (this)
        {
            if (!this.m_loading)
            {
                this.m_loading = true;
                Task something = FinalizeLoadScoresAsync();
                something.Wait();
            }
        }
    }

    private async Task FinalizeLoadScoresAsync()
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (storage.FileExists("HighScores.json"))
                    {
                        using IsolatedStorageFileStream fs = storage.OpenFile("HighScores.json", FileMode.Open);
                        if (fs != null)
                        {
                            DataContractJsonSerializer mySerializer = new(typeof(Dictionary<int, List<GameScore>>));
                            m_loadedScores = (Dictionary<int, List<GameScore>>)mySerializer.ReadObject(fs);
                        }
                    }
                }
                catch (IsolatedStorageException)
                {
                    Debug.WriteLine("IsolatedStorageException");
                }
            }

            this.m_loading = false;
        });
    }

    public void SaveAction(ActionEnum action, Keys key)
    {
        lock (this)
        {
            if (!m_saving)
            {
                m_saving = true;
                var result = FinalizeSaveAsync(action, key);
                result.Wait();
            }
        }
    }

    private async Task FinalizeSaveAsync(ActionEnum action, Keys key)
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using IsolatedStorageFileStream fs = storage.OpenFile("Actions.json", FileMode.Create);
                    if (fs != null)
                    {
                        m_loadedActions.Remove(action);
                        m_loadedActions.Add(action, key);
                        DataContractJsonSerializer mySerializer = new(typeof(Dictionary<ActionEnum, Keys>));
                        mySerializer.WriteObject(fs, m_loadedActions);
                    }
                }
                catch (IsolatedStorageException)
                {
                    Debug.WriteLine("IsolatedStorageException");
                }
            }

            this.m_saving = false;
        });
    }

    /// <summary>
    /// Demonstrates how to deserialize an object from storage device
    /// </summary>
    public void LoadKeys()
    {
        lock (this)
        {
            if (!this.m_loading)
            {
                this.m_loading = true;
                Task something = FinalizeLoadActionsAsync();
                something.Wait();
            }
        }
    }

    private async Task FinalizeLoadActionsAsync()
    {
        await Task.Run(() =>
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (storage.FileExists("Actions.json"))
                    {
                        using IsolatedStorageFileStream fs = storage.OpenFile("Actions.json", FileMode.Open);
                        if (fs != null)
                        {
                            DataContractJsonSerializer mySerializer = new(typeof(Dictionary<ActionEnum, Keys>));
                            m_loadedActions = (Dictionary<ActionEnum, Keys>)mySerializer.ReadObject(fs);
                        }
                    }
                }
                catch (IsolatedStorageException)
                {
                    Debug.WriteLine("IsolatedStorageException");
                }
            }

            this.m_loading = false;
        });
    }
}

