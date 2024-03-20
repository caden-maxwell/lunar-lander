using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace LunarLander.Storage;

public class Database
{
    private bool m_saving = false;
    private bool m_loading = false;
    private List<GameScore> m_loadedState = new();
    public List<GameScore> Scores { get; private set; }

    public Database()
    {
        LoadScores();
        Scores = new(m_loadedState);
    }

    public void SaveScore(GameScore score)
    {
        lock (this)
        {
            if (!m_saving)
            {
                m_saving = true;
                var result = FinalizeSaveAsync(score);
                result.Wait();
            }
        }
    }

    private async Task FinalizeSaveAsync(GameScore score)
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
                        m_loadedState.Add(score);
                        DataContractJsonSerializer mySerializer = new(typeof(List<GameScore>));
                        mySerializer.WriteObject(fs, m_loadedState);
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
                Task something = FinalizeLoadAsync();
                something.Wait();
            }
        }
    }

    private async Task FinalizeLoadAsync()
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
                            DataContractJsonSerializer mySerializer = new(typeof(List<GameScore>));
                            m_loadedState = (List<GameScore>)mySerializer.ReadObject(fs);
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

