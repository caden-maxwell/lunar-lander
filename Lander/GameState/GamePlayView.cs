using Lander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lander;

public class GamePlayView : GameStateView
{
    private SpriteFont m_font;
    private BasicEffect m_effect;
    private RasterizerState m_rasterizerState;

    private VertexPositionColor[] m_vertsTriStrip;
    private int[] m_indexTriStrip;
    private List<Line> m_lines = new();
    private float m_srf; // Surface roughness factor
    private float m_detailLevel; // X distance between terrain vertices - higher is less detailed
    private int m_startY; // Starting y-level for terrain

    public override GameStateEnum State { get; } = GameStateEnum.GamePlay;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.GamePlay;

    public override void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics)
    {
        base.Initialize(graphicsDevice, graphics);

        m_rasterizerState = new RasterizerState
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.CullCounterClockwiseFace,
            MultiSampleAntiAlias = true
        };

        m_effect = new BasicEffect(m_graphics.GraphicsDevice)
        {
            VertexColorEnabled = true,
            View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up),
            Projection = Matrix.CreateOrthographicOffCenter(
                0,
                m_graphics.GraphicsDevice.Viewport.Width,
                m_graphics.GraphicsDevice.Viewport.Height,
                0,
                0.1f,
                2
            ),
        };

        m_srf = 0.6f;
        m_detailLevel = 10; // Lower values = more detailed
        m_startY = (int)(m_graphics.PreferredBackBufferHeight * 0.66f);

        BuildTerrain();
    }

    public override void Reload()
    {
        BuildTerrain();
    }

    private void BuildTerrain()
    {
        Line startLine = new(new Vector2(0, m_startY), new Vector2(m_graphics.PreferredBackBufferWidth, m_startY));
        m_lines.Clear();
        RandMidpointDisplacement(startLine, m_lines, new RandomGen());

        // Create an array for each of the unique vertices
        m_vertsTriStrip = new VertexPositionColor[2 * m_lines.Count];
        m_indexTriStrip = new int[2 * m_lines.Count];

        int i;
        float x;
        float y;
        Color topColor = Color.Black;
        Color bottomColor = Color.DarkRed;
        for (i = 0; i < m_lines.Count; i++)
        {
            x = m_lines[i].Start.X;
            y = m_lines[i].Start.Y;

            m_vertsTriStrip[2 * i].Position = new(x, m_graphics.PreferredBackBufferHeight, 0);
            m_vertsTriStrip[2 * i + 1].Position = new(x, y, 0);

            m_vertsTriStrip[2 * i].Color = bottomColor;
            m_vertsTriStrip[2 * i + 1].Color = topColor;

            m_indexTriStrip[2 * i] = 2 * i;
            m_indexTriStrip[2 * i + 1] = 2 * i + 1;
        }
        i--;

        x = m_lines[i].End.X;
        y = m_lines[i].End.Y;

        m_vertsTriStrip[2 * i].Position = new(x, m_graphics.PreferredBackBufferHeight, 0);
        m_vertsTriStrip[2 * i + 1].Position = new(x, y, 0);

        m_vertsTriStrip[2 * i].Color = bottomColor;
        m_vertsTriStrip[2 * i + 1].Color = topColor;

        m_indexTriStrip[2 * i] = 2 * i;
        m_indexTriStrip[2 * i + 1] = 2 * i + 1;

    }

    private void RandMidpointDisplacement(Line line, List<Line> lines, RandomGen rand)
    {
        if (line.DistX() <= m_detailLevel)
        {
            lines.Add(line);
            return;
        }
        Line firstHalf;
        Line secondHalf;
        (firstHalf, secondHalf) = line.Split();

        float lenX = firstHalf.DistX();
        float disp = m_srf * (float)rand.NextGaussian(0, 1) * lenX;

        int UPPER_BOUND = (int)(m_graphics.PreferredBackBufferHeight * 0.15f);
        if (firstHalf.End.Y + disp >= UPPER_BOUND && firstHalf.End.Y + disp < m_graphics.PreferredBackBufferHeight)
        {
            firstHalf.DisplaceY(disp, false);
            secondHalf.DisplaceY(disp, true);
        }

        RandMidpointDisplacement(firstHalf, lines, rand);
        RandMidpointDisplacement(secondHalf, lines, rand);
    }

    public override void LoadContent(ContentManager contentManager)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/menu");
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        m_graphics.GraphicsDevice.RasterizerState = m_rasterizerState;
        foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            m_graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleStrip,
                m_vertsTriStrip,
                0,
                m_vertsTriStrip.Length,
                m_indexTriStrip,
                0,
                2 * (m_lines.Count - 1)
            );
        }

        m_spriteBatch.End();
    }
}

public class Line
{
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }

    public Line(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public float DistX()
    {
        return End.X - Start.X;
    }

    public (Line, Line) Split()
    {
        Vector2 midpoint = (Vector2.Subtract(End, Start) / 2) + Start;
        return (new Line(Start, midpoint), new Line(midpoint, End));
    }

    public void DisplaceY(float disp, bool start)
    {
        Vector2 dispVec = new(0, disp);
        if (start)
            Start += dispVec;
        else
            End += dispVec;
    }

    public override string ToString()
    {
        return Start.ToString() + ", " + End.ToString();
    }
}
