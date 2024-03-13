using LunarLander.Helpers;
using LunarLander.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LunarLander;

public class GamePlayView : GameStateView
{
    private SpriteFont m_font;
    private BasicEffect m_effect;
    private RasterizerState m_rasterizerState;

    private VertexPositionColor[] m_vertsTriStrip;
    private int[] m_indexTriStrip;
    private List<Line> m_lines = new();

    private const float SCALE = 1.4f; // Simulation scale - higher values zoom in (make things bigger)
    private readonly float m_srf = MathHelper.Clamp(0.55f / SCALE, 0.45f, 0.75f); // Surface roughness factor - higher is more rough
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

    private Dictionary<SpaceBodiesEnum, float> m_gravAccels = new()
    {
        { SpaceBodiesEnum.Sun, 274},
        { SpaceBodiesEnum.Mercury, 3.70f},
        { SpaceBodiesEnum.Venus, 8.87f},
        { SpaceBodiesEnum.Earth, 9.82f},
        { SpaceBodiesEnum.Moon, 1.62f},
        { SpaceBodiesEnum.Mars, 3.73f},
        { SpaceBodiesEnum.Jupiter, 25.92f},
        { SpaceBodiesEnum.Titan, 1.35f},
        { SpaceBodiesEnum.Saturn, 11.19f},
        { SpaceBodiesEnum.Uranus, 9.01f},
        { SpaceBodiesEnum.Neptune, 11.27f},
        { SpaceBodiesEnum.Pluto, 0.62f}
    };
    private const float PX_PER_METER = 4.28f * SCALE; // Approximate scale factor
    private float GRAV_ACCEL;
    private Vector2 m_gravity;
    private Texture2D m_texLander;
    private Rectangle m_rectLander = new();
    private Rectangle m_rectSpriteSource = new();
    private Lander m_lander;
    private Vector2 m_landerStartOrientation = new(2, 1);
    private Vector2 m_landerStartPosition;
    private bool m_landerThrustApplied = false;
    private int m_landerThrustTimer = 0;

    public GamePlayView(InputMapper inputMapper, SpaceBodiesEnum body)
    {
        m_inputMapper = inputMapper;
        GRAV_ACCEL = m_gravAccels[body];
        m_gravity = new(0, GRAV_ACCEL * PX_PER_METER / 1000000f); // px/ms^2
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
            Top = (int)(m_graphics.PreferredBackBufferHeight * (m_pctFromEdge + 0.1f)),
            Bottom = (int)(m_graphics.PreferredBackBufferHeight * (1 - m_pctFromEdge)),
            Left = (int)(m_graphics.PreferredBackBufferWidth * m_pctFromEdge),
            Right = (int)(m_graphics.PreferredBackBufferWidth * (1 - m_pctFromEdge))
        };

        m_landerStartPosition = new Vector2(
            m_graphics.PreferredBackBufferWidth * 0.35f, m_graphics.PreferredBackBufferHeight * 0.05f
        );

        float landerAccel = 15f; // m/s^2
        m_lander = new(
            m_landerStartPosition,
            m_landerStartOrientation,
            landerAccel * PX_PER_METER / 1000000f // px/ms^2
        );

        BuildTerrain();
    }

    #region terrain
    private void BuildTerrain()
    {
        // Have terrain start 2/3 the way down, varying by ~10% of screen height typically
        float randTerrainDisp = m_graphics.PreferredBackBufferHeight * 0.1f * (float)m_rand.NextGaussian(0, 1);
        m_terrainYLevel = (int)(m_graphics.PreferredBackBufferHeight * 0.66f + randTerrainDisp);

        float boundsWidth = m_bounds.Right - m_bounds.Left;
        float landingZoneSize = m_graphics.PreferredBackBufferWidth * 0.05f * SCALE; // Landing Zones only 5% of total width
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
        Color topColor = new(0x89, 0x89, 0x89);
        Color bottomColor = new(0x49, 0x49, 0x49);
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
    #endregion terrain

    public override void LoadContent(ContentManager contentManager)
    {
        m_font = contentManager.Load<SpriteFont>("Fonts/stats");
        m_texLander = contentManager.Load<Texture2D>("Images/lander");
        m_rectSpriteSource.Width = m_texLander.Width / 3;
        m_rectSpriteSource.Height = m_texLander.Height;

        float aspectRatio = m_rectSpriteSource.Width / (float)m_rectSpriteSource.Height;

        const float LANDER_HEIGHT = 7; // Meters tall
        float LANDER_WIDTH = LANDER_HEIGHT / aspectRatio;
        m_rectLander.Width = (int)(LANDER_HEIGHT * PX_PER_METER);
        m_rectLander.Height = (int)(LANDER_WIDTH * PX_PER_METER);
    }
    public override void Reload()
    {
        m_lander.Reset(m_landerStartPosition, m_landerStartOrientation);
        BuildTerrain();
    }

    public override void RegisterKeys(IInputDevice inputDevice)
    {
        base.RegisterKeys(inputDevice);

        inputDevice.RegisterCommand(
            m_inputMapper.KeyboardMappings[ActionEnum.Thrust],
            false,
            new CommandDelegate(m_lander.Thrust)
        );
        inputDevice.RegisterCommand(
            m_inputMapper.KeyboardMappings[ActionEnum.RotateClockwise],
            false,
            new CommandDelegate((gameTime, value) => m_lander.Rotate(gameTime, value, true))
        );
        inputDevice.RegisterCommand(
            m_inputMapper.KeyboardMappings[ActionEnum.RotateCounterClockwise],
            false,
            new CommandDelegate((gameTime, value) => m_lander.Rotate(gameTime, value, false))
        );
    }

    public override void Update(GameTime gameTime)
    {
        int elapsed = gameTime.ElapsedGameTime.Milliseconds;
        m_lander.Velocity += m_gravity * elapsed;
        m_landerThrustApplied = m_lander.UsingThrust;

        m_landerThrustTimer = m_landerThrustApplied ?
            m_landerThrustTimer + elapsed :
            m_landerThrustTimer - 2 * elapsed;
        m_landerThrustTimer = MathHelper.Clamp(m_landerThrustTimer, 0, 501);
        m_landerThrustTimer %= 500;

        m_lander.Update(gameTime);
        m_rectLander.Location = m_lander.Position.ToPoint();
    }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin(samplerState: SamplerState.PointClamp);

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

        m_rectSpriteSource.X = 0;
        if (m_landerThrustTimer > 250)
            m_rectSpriteSource.X = m_rectSpriteSource.Width * 2;
        else if (m_landerThrustTimer > 0)
            m_rectSpriteSource.X = m_rectSpriteSource.Width;

        // m_lander.Angle is the angle with respect to the x-axis -- we need it with respect to the y-axis
        float angle = Lander.DirectionToAngle(new Vector2(-m_lander.Direction.Y, m_lander.Direction.X));
        m_spriteBatch.Draw(
            m_texLander,
            m_rectLander,
            m_rectSpriteSource,
            Color.White,
            angle,
            new Vector2(m_rectSpriteSource.Width / 2, m_rectSpriteSource.Height / 2),
            SpriteEffects.None,
            0
        );

        float x = m_lander.Velocity.X / PX_PER_METER * 1000;
        float y = -m_lander.Velocity.Y / PX_PER_METER * 1000;
        string text = $"Horizontal Velocity:{x,7:0.00} m/s\nVertical Velocity: {y,7:0.00} m/s";
        float textX = m_graphics.PreferredBackBufferWidth * 0.01f;
        float textY = textX;
        Vector2 stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.White
        );
        textY += stringSize.Y + 10;

        float speed = m_lander.Speed / PX_PER_METER * 1000;
        text = $"Speed:";
        textX = m_graphics.PreferredBackBufferWidth * 0.01f;

        stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.White
        );
        textX += stringSize.X;

        text = $"{speed,7:0.00}";
        stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            speed < 2 ? Color.LightGreen : Color.White
        );
        textX += stringSize.X;

        text = $" m/s";
        stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.White
        );

        textX = m_graphics.PreferredBackBufferWidth * 0.01f;
        textY += stringSize.Y + 10;

        angle = MathHelper.ToDegrees(Math.Abs(angle));
        text = $"Angle:";
        stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.White
        );
        textX += stringSize.X;

        text = $"{angle,7:0.00}";
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            angle < 5 ? Color.LightGreen : Color.White
        );

        m_spriteBatch.End();
    }

    private class Lander
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; set; } // Meters per Second
        public float AngVelocity { get; private set; }
        public Vector2 Direction { get; private set; } // positive y thrusts up
        private float m_rotationForce = 1.5f / 1000000f;
        private float m_thrustAccel;
        public float Speed { get { return Velocity.Length(); } }
        public float Angle { get { return DirectionToAngle(Direction); } } // Angle with respect to x-axis
        public bool UsingThrust { get; set; }

        public Lander(Vector2 initialPosition, Vector2 initialDirection, float thrustAccel)
        {
            Reset(initialPosition, initialDirection);
            AngVelocity = 0;
            m_thrustAccel = thrustAccel; // px/ms^2
        }

        public void Update(GameTime gameTime)
        {
            UsingThrust = false;
            int elapsed = gameTime.ElapsedGameTime.Milliseconds;
            Position += Velocity * elapsed;
            Direction = AngleToDirection(Angle + AngVelocity * elapsed);
        }

        public void Thrust(GameTime gameTime, float value)
        {
            int elapsed = gameTime.ElapsedGameTime.Milliseconds;
            Velocity += m_thrustAccel * Direction * elapsed;
            AngVelocity -= AngVelocity * 0.001f * elapsed;
            UsingThrust = true;
        }

        public void Rotate(GameTime gameTime, float value, bool clockwise)
        {
            float elapsed = gameTime.ElapsedGameTime.Milliseconds;
            float changeVel = m_rotationForce * elapsed;
            if (clockwise)
                AngVelocity += changeVel;
            else
                AngVelocity -= changeVel;
        }

        public static Vector2 AngleToDirection(float angle)
        {
            return new Vector2(
              (float)Math.Cos(angle),
              (float)Math.Sin(angle)
            );
        }
        public static float DirectionToAngle(Vector2 Direction)
        {
            return (float)Math.Atan2(Direction.Y, Direction.X);
        }

        public void Reset(Vector2 position, Vector2 direction)
        {
            Position = position;
            Velocity = new(0, 0);
            AngVelocity = 0;
            direction.Y = -direction.Y;
            Direction = Vector2.Normalize(direction);
        }
    }
}

