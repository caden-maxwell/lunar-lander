﻿using LunarLander.Helpers;
using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LunarLander;

public class GamePlayView : GameStateView
{
    private SpriteFont m_font;
    private BasicEffect m_effect;
    private RasterizerState m_rasterizerState;

    private VertexPositionColor[] m_vertsTriStrip;
    private int[] m_indexTriStrip;
    private List<Line> m_lines = new();

    private readonly float m_srf = 0.65f; // Surface roughness factor - higher is more rough
    private readonly float m_terrainDetail = 5; // X distance between terrain vertices - higher is less detailed
    private readonly float m_pctFromEdge = 0.15f; // Percent of screen that bounds are away from window edges
    private float m_terrainYLevel; // Starting y-level for terrain
    private struct Bounds
    {
        public float Top;
        public float Bottom;
        public float Left;
        public float Right;
    };
    private Bounds m_bounds; // Defines bounds for safety zones and top of terrain

    private int m_level = 1;
    private int m_numLandingZones = 2;
    private enum GamePlayState
    {
        Transition,
        Playing,
        Paused,
        End
    }

    private readonly RandomGen m_rand = new();

    private readonly InputMapper m_inputMapper;

    public override GameStateEnum State { get; } = GameStateEnum.GamePlay;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.GamePlay;

    public GamePlayView(InputMapper inputMapper)
    {
        m_inputMapper = inputMapper;
    }

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

        m_bounds = new Bounds()
        {
            Top = (int)(m_graphics.PreferredBackBufferHeight * m_pctFromEdge),
            Bottom = (int)(m_graphics.PreferredBackBufferHeight * (1 - m_pctFromEdge)),
            Left = (int)(m_graphics.PreferredBackBufferWidth * m_pctFromEdge),
            Right = (int)(m_graphics.PreferredBackBufferWidth * (1 - m_pctFromEdge))
        };

        BuildTerrain();
    }

    public override void Reload()
    {
        BuildTerrain();
    }

    public override void RegisterKeys(IInputDevice inputDevice)
    {
        base.RegisterKeys(inputDevice);

        inputDevice.RegisterCommand(
            m_inputMapper.KeyboardMappings[ActionEnum.Thrust],
            false,
            new CommandDelegate(Thrust)
        );
        inputDevice.RegisterCommand(
            m_inputMapper.KeyboardMappings[ActionEnum.RotateClockwise],
            false,
            new CommandDelegate((gameTime, value) => Rotate(gameTime, value, true))
        );
        inputDevice.RegisterCommand(
            m_inputMapper.KeyboardMappings[ActionEnum.RotateCounterClockwise],
            false,
            new CommandDelegate((gameTime, value) => Rotate(gameTime, value, false))
        );
    }

    private void BuildTerrain()
    {
        // Have terrain start 2/3 the way down, varying by ~10% of screen height typically
        double randTerrainDisp = m_graphics.PreferredBackBufferHeight * 0.1f * m_rand.NextGaussian(0, 1);
        m_terrainYLevel = (int)(m_graphics.PreferredBackBufferHeight * 0.66f + randTerrainDisp);

        float boundsWidth = m_bounds.Right - m_bounds.Left;
        float landingZoneSize = m_graphics.PreferredBackBufferWidth * 0.05f; // Landing Zones only 5% of total width
        List<Line> zones = new();
        Vector2 prevEnd = new(0, m_terrainYLevel);
        for (int i = 0; i < m_numLandingZones; i++)
        {
            // Segment possible landing zone areas into columns
            float leftBound = m_bounds.Left + (i / (float)m_numLandingZones * boundsWidth);
            float rightBound = m_bounds.Right - ((m_numLandingZones - i - 1) / (float)m_numLandingZones * boundsWidth);

            // Get random starting point somewhere in its column
            Vector2 landingZoneStart = new(
                m_rand.NextRange((int)leftBound, (int)(rightBound - landingZoneSize)),
                m_rand.NextRange((int)m_bounds.Top, (int)m_bounds.Bottom)
            );
            Vector2 landingZoneEnd = landingZoneStart + new Vector2(landingZoneSize, 0);

            Line terrainZone = new(prevEnd, landingZoneStart);
            Line landingZone = new(landingZoneStart, landingZoneEnd);

            zones.Add(terrainZone);
            zones.Add(landingZone);

            prevEnd = landingZoneEnd;
        }
        Line lastZone = new(prevEnd, new Vector2(m_graphics.PreferredBackBufferWidth, m_terrainYLevel));
        zones.Add(lastZone);

        m_lines.Clear();
        bool isLandingZone = false; // Alternate between terrain and landing zones
        for (int i = 0; i < zones.Count; i++)
        {
            Line currentZone = zones[i];
            if (isLandingZone) // Dont alter landing zones
            {
                m_lines.Add(currentZone);
                isLandingZone = false;
                continue;
            }

            RandMidpointDisplacement(currentZone, m_lines, m_rand);
            isLandingZone = true;
        }

        // Create an array for each of the unique vertices
        m_vertsTriStrip = new VertexPositionColor[2 * m_lines.Count];
        m_indexTriStrip = new int[2 * m_lines.Count];

        int lineIdx;
        float x;
        float y;
        Color topColor = Color.Black;
        Color bottomColor = Color.DarkRed;
        for (lineIdx = 0; lineIdx < m_lines.Count; lineIdx++)
        {
            x = m_lines[lineIdx].Start.X;
            y = m_lines[lineIdx].Start.Y;

            m_vertsTriStrip[2 * lineIdx].Position = new(x, m_graphics.PreferredBackBufferHeight, 0);
            m_vertsTriStrip[2 * lineIdx + 1].Position = new(x, y, 0);

            m_vertsTriStrip[2 * lineIdx].Color = bottomColor;
            m_vertsTriStrip[2 * lineIdx + 1].Color = topColor;

            m_indexTriStrip[2 * lineIdx] = 2 * lineIdx;
            m_indexTriStrip[2 * lineIdx + 1] = 2 * lineIdx + 1;
        }
        lineIdx--;

        x = m_lines[lineIdx].End.X;
        y = m_lines[lineIdx].End.Y;

        m_vertsTriStrip[2 * lineIdx].Position = new(x, m_graphics.PreferredBackBufferHeight, 0);
        m_vertsTriStrip[2 * lineIdx + 1].Position = new(x, y, 0);

        m_vertsTriStrip[2 * lineIdx].Color = bottomColor;
        m_vertsTriStrip[2 * lineIdx + 1].Color = topColor;

        m_indexTriStrip[2 * lineIdx] = 2 * lineIdx;
        m_indexTriStrip[2 * lineIdx + 1] = 2 * lineIdx + 1;
    }

    private void RandMidpointDisplacement(Line line, List<Line> lines, RandomGen rand)
    {
        if (line.DistX() <= m_terrainDetail)
        {
            lines.Add(line);
            return;
        }
        Line firstHalf;
        Line secondHalf;
        (firstHalf, secondHalf) = line.Split();

        float lenX = firstHalf.DistX();
        float disp = m_srf * (float)rand.NextGaussian(0, 1) * lenX;

        if (firstHalf.End.Y + disp >= m_bounds.Top && firstHalf.End.Y + disp < m_graphics.PreferredBackBufferHeight)
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

    public override void Update(GameTime gameTime) { }

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

    private void Thrust(GameTime gameTime, float value)
    {
        Debug.WriteLine("Thrusting!!!");
    }
    private void Rotate(GameTime gameTime, float value, bool clockwise)
    {
        if (clockwise)
            Debug.WriteLine("Rotating Clockwise!!!");
        else
            Debug.WriteLine("Rotating Counterclockswise!!!");
    }

    private class Lander
    {
        public Lander()
        {

        }
    }
}
