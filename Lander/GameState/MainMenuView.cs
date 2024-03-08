using Lander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lander;

public class MainMenuView : GameStateView
{
    private SpriteFont m_fontMenu;
    private SpriteFont m_fontMenuSelect;

    public override GameStateEnum State { get; } = GameStateEnum.MainMenu;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.MainMenu;

    private enum MenuState
    {
        NewGame,
        HighScores,
        Help,
        About,
        Configs,
        Quit,
        N
    }

    private MenuState m_currentSelection = MenuState.NewGame;

    public override void RegisterKeys(KeyboardInput keyboardInput)
    {
        keyboardInput.RegisterCommand(Keys.Down, true, new CommandDelegate(SelectBelow));
        keyboardInput.RegisterCommand(Keys.Up, true, new CommandDelegate(SelectAbove));
        keyboardInput.RegisterCommand(Keys.Enter, true, new CommandDelegate(EnterPressed));
        keyboardInput.RegisterCommand(Keys.Escape, true, new CommandDelegate(EscPressed));
    }

    public override void LoadContent(ContentManager contentManager)
    {
        m_fontMenu = contentManager.Load<SpriteFont>("Fonts/menu");
        m_fontMenuSelect = contentManager.Load<SpriteFont>("Fonts/menu-select");
    }

    public override void Update(GameTime gameTime) { }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        // I split the first one's parameters on separate lines to help you see them better
        float bottom = DrawMenuItem(
            m_currentSelection == MenuState.NewGame ? m_fontMenuSelect : m_fontMenu,
            "New Game",
            200,
            m_currentSelection == MenuState.NewGame ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.HighScores ? m_fontMenuSelect : m_fontMenu, "High Scores", bottom, m_currentSelection == MenuState.HighScores ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.Help ? m_fontMenuSelect : m_fontMenu, "Help", bottom, m_currentSelection == MenuState.Help ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.About ? m_fontMenuSelect : m_fontMenu, "About", bottom, m_currentSelection == MenuState.About ? Color.Yellow : Color.Blue);
        bottom = DrawMenuItem(m_currentSelection == MenuState.Configs ? m_fontMenuSelect : m_fontMenu, "Configs", bottom, m_currentSelection == MenuState.Configs ? Color.Yellow : Color.Blue);
        DrawMenuItem(m_currentSelection == MenuState.Quit ? m_fontMenuSelect : m_fontMenu, "Quit", bottom, m_currentSelection == MenuState.Quit ? Color.Yellow : Color.Blue);

        m_spriteBatch.End();
    }

    private float DrawMenuItem(SpriteFont font, string text, float y, Color color)
    {
        Vector2 stringSize = font.MeasureString(text);
        m_spriteBatch.DrawString(
            font,
            text,
            new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y),
            color);

        return y + stringSize.Y;
    }

    private void SelectBelow(GameTime gameTime, float value)
    {
        int i = (int)m_currentSelection + 1;
        i %= (int)MenuState.N;
        m_currentSelection = (MenuState)i;
    }

    private void SelectAbove(GameTime gameTime, float value)
    {
        int i = (int)m_currentSelection - 1 + (int)MenuState.N; // C# mod doesn't handle negative values well, need to add N
        i %= (int)MenuState.N;
        m_currentSelection = (MenuState)i;
    }

    private void EnterPressed(GameTime gameTime, float value)
    {
        switch (m_currentSelection)
        {
            case MenuState.NewGame: NextState = GameStateEnum.GamePlay; break;
            case MenuState.HighScores: NextState = GameStateEnum.HighScores; break;
            case MenuState.Help: NextState = GameStateEnum.Help; break;
            case MenuState.About: NextState = GameStateEnum.About; break;
            case MenuState.Configs: NextState = GameStateEnum.Config; break;
            case MenuState.Quit: NextState = GameStateEnum.Exit; break;
        }
    }

    public override void EscPressed(GameTime gameTime, float value)
    {
        NextState = GameStateEnum.Exit;
    }
}