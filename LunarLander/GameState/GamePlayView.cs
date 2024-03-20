using LunarLander.Helpers;
using LunarLander.Input;
using LunarLander.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LunarLander;

public class GamePlayView : GameStateView
{
    public override GameStateEnum State { get; } = GameStateEnum.GamePlay;
    public override GameStateEnum NextState { get; set; } = GameStateEnum.GamePlay;

    // Terrain/World
    private BasicEffect m_effect;
    private RasterizerState m_rasterizerState;
    private VertexPositionColor[] m_vertsTriStrip;
    private int[] m_indexTriStrip;
    private float m_srf; // Surface roughness factor - higher is more rough
    private const float TERRAIN_DETAIL = 10; // X distance between terrain vertices - higher is less detailed
    private const float BOUNDS_PCT_FROM_EDGE = 0.15f; // Percent of screen that bounds are away from window edges
    private float m_terrainYLevel; // Starting y-level for terrain
    private struct Bounds
    {
        public float Top;
        public float Bottom;
        public float Left;
        public float Right;
    };
    private Bounds m_bounds; // Defines bounds for landing zones and top of terrain
    private readonly List<Line> m_lines = new();
    private const int MAX_LANDING_ZONES = 2;
    private readonly List<Line> m_landingZones = new();
    private float PX_PER_METER;
    private Vector2 m_gravity; // Virtual-world vector in px/ms^2
    private readonly Dictionary<SpaceBodiesEnum, float> GRAV_ACCELS = new()
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
    private readonly float GRAV_ACCEL;

    // Gameplay
    private enum GamePlayState
    {
        Transition,
        Playing,
        Paused,
        Lose,
        Win,
        End
    }
    private GamePlayState m_gameState;
    private float m_transitionTimer = 3000;
    private int m_level;
    private readonly Dictionary<int, (float, int, float)> m_levels = new() // <levelID, (Map scale, number landing zones, size of landing zone with respect to lander width)>
    {
        {1, (1.3f, 2, 2.50f) },
        {2, (1.1f, 1, 1.75f) },
        {3, (1.0f, 2, 1.25f) },
        {4, (0.9f, 2, 1.25f) },
        {5, (0.7f, 1, 1.10f) },
    };

    private readonly InputMapper m_inputMapper;
    private readonly RandomGen m_rand = new();
    private readonly int[] m_movingFPS = new int[50];

    // Fonts
    private SpriteFont m_font;
    private SpriteFont m_fontBig;

    // Textures
    private Texture2D m_backgroundTex;
    private Texture2D m_landerTex;
    private Rectangle m_landerRect = new();
    private Rectangle m_landerRectSpriteSource = new();
    private float m_landerAspectRatio;

    // Particles
    private SpriteBatch m_particleSpriteBatch; // Separate spritebatch for additive blending
    private ParticleSystem m_particleSystemFire;
    private ParticleSystemRenderer m_renderFire;
    private ParticleSystem m_particleSystemSmoke;
    private ParticleSystemRenderer m_renderSmoke;

    // Sounds
    private SoundEffectInstance m_engineSound;
    private SoundEffectInstance m_explosionSound;
    private SoundEffectInstance m_clappingSound;

    // Lander
    private Lander m_lander;
    private Vector2 m_landerStartOrientation = new(1, 0);
    private Vector2 m_landerStartPosition;
    private bool m_landerThrustApplied = false;
    private float m_landerThrustTimer = 0; // Used to keep track of sound fade in/out and spritesheet
    private const float MAX_THRUST_TIMER = 500; // ms
    private const float SAFE_LANDING_SPEED = 2; // m/s
    private const float SAFE_LANDING_ANGLE = 5; // Degrees
    private bool m_isSafeSpeed = false;
    private bool m_isSafeAngle = false;

    public GamePlayView(InputMapper inputMapper, SpaceBodiesEnum body)
    {
        m_inputMapper = inputMapper;
        GRAV_ACCEL = GRAV_ACCELS[body];
    }

    public override void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, IInputDevice m_inputDevice)
    {
        base.Initialize(graphicsDevice, graphics, m_inputDevice);

        m_particleSpriteBatch = new(graphicsDevice);

        m_rasterizerState = new RasterizerState
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.CullCounterClockwiseFace,
            MultiSampleAntiAlias = true,
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
            Top = (int)(m_graphics.PreferredBackBufferHeight * (BOUNDS_PCT_FROM_EDGE + 0.1f)),
            Bottom = (int)(m_graphics.PreferredBackBufferHeight * (1 - BOUNDS_PCT_FROM_EDGE)),
            Left = (int)(m_graphics.PreferredBackBufferWidth * BOUNDS_PCT_FROM_EDGE),
            Right = (int)(m_graphics.PreferredBackBufferWidth * (1 - BOUNDS_PCT_FROM_EDGE))
        };

        m_landerStartPosition = new Vector2(
            m_graphics.PreferredBackBufferWidth * 0.35f, m_graphics.PreferredBackBufferHeight * 0.05f
        );

        m_particleSystemFire = new ParticleSystem(500);
        m_renderFire = new ParticleSystemRenderer("Images/fire");

        m_particleSystemSmoke = new ParticleSystem(0.05f);
        m_renderSmoke = new ParticleSystemRenderer("Images/smoke");
    }

    #region terrain
    private void BuildTerrain()
    {
        // Have terrain start 2/3 the way down, varying by ~10% of screen height typically
        float randTerrainDisp = m_graphics.PreferredBackBufferHeight * 0.1f * (float)m_rand.NextGaussian(0, 1);
        m_terrainYLevel = (int)(m_graphics.PreferredBackBufferHeight * 0.66f + randTerrainDisp);

        m_landingZones.Clear();
        float boundsWidth = m_bounds.Right - m_bounds.Left;
        float landingZoneSize = m_lander.Width * m_levels[m_level].Item3;
        List<Line> zones = new();
        Vector2 prevEnd = new(0, m_terrainYLevel);
        int numLandingZones = m_levels[m_level].Item2;
        for (int i = 0; i < numLandingZones; i++)
        {
            // Segment possible landing zone areas into columns
            float leftBound = m_bounds.Left + (i / (float)numLandingZones * boundsWidth);
            float rightBound = m_bounds.Right - ((numLandingZones - i - 1) / (float)numLandingZones* boundsWidth);

            // Get random starting point somewhere in its column
            Vector2 landingZoneStart = new(
                m_rand.NextRange(leftBound, (rightBound - landingZoneSize)),
                m_rand.NextRange(m_bounds.Top, m_bounds.Bottom)
            );
            Vector2 landingZoneEnd = landingZoneStart + new Vector2(landingZoneSize, 0);

            Line terrainZone = new(prevEnd, landingZoneStart);
            Line landingZone = new(landingZoneStart, landingZoneEnd);

            zones.Add(terrainZone);
            zones.Add(landingZone);
            m_landingZones.Add(landingZone);

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
        Color topColor = Color.DarkOrange;
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
        if (line.DistX() <= TERRAIN_DETAIL)
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
        // Fonts
        m_font = contentManager.Load<SpriteFont>("Fonts/stats");
        m_fontBig = contentManager.Load<SpriteFont>("Fonts/stats-big");

        // Textures
        m_backgroundTex = contentManager.Load<Texture2D>("Images/gargantua");
        m_landerTex = contentManager.Load<Texture2D>("Images/lander");
        m_landerRectSpriteSource.Width = m_landerTex.Width / 3;
        m_landerRectSpriteSource.Height = m_landerTex.Height;
        m_landerAspectRatio = m_landerRectSpriteSource.Width / (float)m_landerRectSpriteSource.Height;

        // Particles
        m_renderFire.LoadContent(contentManager);
        m_renderSmoke.LoadContent(contentManager);

        // Sounds
        m_engineSound = contentManager.Load<SoundEffect>("Audio/enginehum").CreateInstance();
        m_engineSound.IsLooped = true;
        m_engineSound.Pitch = -1.0f;
        m_engineSound.Volume = 0.6f;
        m_explosionSound = contentManager.Load<SoundEffect>("Audio/explosion").CreateInstance();
        m_explosionSound.Pitch = -0.3f;
        m_clappingSound = contentManager.Load<SoundEffect>("Audio/clapping").CreateInstance();
    }

    public override void Reload()
    {
        m_level = 0;
        m_particleSystemFire.Clear();
        m_particleSystemSmoke.Clear();

        m_lander = new();
        SetScale(1.0f);

        NewLevel();
    }

    private void NewLevel()
    {
        m_level++;
        m_gameState = GamePlayState.Transition;
        m_transitionTimer = 3000;
        SetScale(m_levels[m_level].Item1);

        BuildTerrain();
    }

    private void SetScale(float scale)
    {
        m_srf = MathHelper.Clamp(0.40f / scale, 0.25f, 0.70f); // Dont get too sharp or too smooth
        PX_PER_METER = 5 * scale;
        m_gravity = new(0, ScaleNumber(GRAV_ACCEL, MeasurementType.Acceleration));
        ResetLander();
    }

    private void ResetLander()
    {
        int width = (int)ScaleNumber(9.4f, MeasurementType.Value);
        int height = (int)(width / m_landerAspectRatio);
        float landerAccel = ScaleNumber(15f, MeasurementType.Acceleration);

        m_lander.Set(m_landerStartPosition, m_landerStartOrientation, width, height, landerAccel);

        m_landerRect.Location = m_landerStartPosition.ToPoint();
        m_landerRect.Width = m_lander.Width;
        m_landerRect.Height = m_lander.Height;
    }

    private enum MeasurementType
    {
        Value,
        Velocity,
        Acceleration
    }

    /// <summary>
    /// Convert to and from real- and virtual-world measurements
    /// </summary>
    private float ScaleNumber(float value, MeasurementType type, bool toVirtual = true)
    {
        if (!toVirtual)
            return value / PX_PER_METER * (float)Math.Pow(1000, (int)type);
        return value * PX_PER_METER / (float)Math.Pow(1000, (int)type);
    }

    public override void RegisterKeys()
    {
        base.RegisterKeys();
        m_inputDevice.RegisterCommand(
            Keys.Enter,
            true,
            new CommandDelegate(EnterPressed)
        );
    }

    public override void Update(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        if (m_gameState == GamePlayState.Transition)
        {
            m_transitionTimer -= elapsed;
            if (m_transitionTimer < 0)
            {
                m_inputDevice.RegisterCommand(
                    m_inputMapper.KeyboardMappings[ActionEnum.Thrust],
                    false,
                    new CommandDelegate(m_lander.Thrust)
                );
                m_inputDevice.RegisterCommand(
                    m_inputMapper.KeyboardMappings[ActionEnum.RotateClockwise],
                    false,
                    new CommandDelegate((gameTime, value) => m_lander.Rotate(gameTime, value, true))
                );
                m_inputDevice.RegisterCommand(
                    m_inputMapper.KeyboardMappings[ActionEnum.RotateCounterClockwise],
                    false,
                    new CommandDelegate((gameTime, value) => m_lander.Rotate(gameTime, value, false))
                );
                m_gameState = GamePlayState.Playing;
            }
        }

        m_isSafeAngle = MathHelper.ToDegrees(Math.Abs(m_lander.AngleYAxis)) < SAFE_LANDING_ANGLE;
        m_isSafeSpeed = ScaleNumber(m_lander.Speed, MeasurementType.Velocity, false) < SAFE_LANDING_SPEED;
        if (m_gameState == GamePlayState.Playing)
        {
            m_lander.Velocity += m_gravity * elapsed;
            m_landerRect.Location = m_lander.Position.ToPoint();
            m_landerThrustApplied = m_lander.UsingThrust;
            m_lander.Update(gameTime);

            CollisionType collision = CollisionDetector();
            if (collision == CollisionType.Terrain)
            {
                m_lander.Destroyed = true;
                Debug.WriteLine("Terrain");
            }

            if (collision == CollisionType.LandingZone)
                if (m_isSafeSpeed && m_isSafeAngle)
                    m_lander.Landed = true;
                else
                {
                    Debug.WriteLine(ScaleNumber(m_lander.Speed, MeasurementType.Velocity, false));
                    Debug.WriteLine(MathHelper.ToDegrees(Math.Abs(m_lander.AngleYAxis)));
                    m_lander.Destroyed = true;
                }

            if (m_lander.Landed)
            {
                m_inputDevice.UnregisterAll();
                RegisterKeys(); // Regregister enter key
                m_landerThrustApplied = false;
                if (m_level == m_levels.Count)
                    m_gameState = GamePlayState.End;
                else
                    m_gameState = GamePlayState.Win;
                m_clappingSound.Play();
            }
            else if (m_lander.Destroyed)
            {
                m_inputDevice.UnregisterAll();
                RegisterKeys();
                m_landerThrustApplied = false;
                m_gameState = GamePlayState.Lose;
                m_explosionSound.Play();

                m_particleSystemFire.ShipCrash(
                    5000,
                    m_lander.Position,
                    ScaleNumber(15, MeasurementType.Velocity),
                    ScaleNumber(3, MeasurementType.Value),
                    1500
                );
                m_particleSystemSmoke.ShipCrash(
                    1000,
                    m_lander.Position,
                    ScaleNumber(5, MeasurementType.Velocity),
                    ScaleNumber(5, MeasurementType.Value),
                    5500
                );
            }
        }

        if (m_landerThrustApplied)
        {

            Line bottomLine = new(m_lander.Corners[2].ToVector2(), m_lander.Corners[3].ToVector2());
            Vector2 thrustPoint = bottomLine.Split().Item1.End;

            m_landerThrustTimer += elapsed;
            m_particleSystemFire.ShipThrust(
                elapsed,
                thrustPoint,
                -m_lander.Direction,
                m_lander.ThrustAccel * 10000,
                ScaleNumber(1, MeasurementType.Value),
                15
            );

            m_particleSystemSmoke.ShipThrust(
                elapsed,
                thrustPoint,
                -m_lander.Direction,
                m_lander.ThrustAccel * 500,
                ScaleNumber(4, MeasurementType.Value),
                3000
            );
            m_engineSound.Play();
        }
        else
        {
            m_landerThrustTimer -= elapsed;
        }

        m_landerThrustTimer = MathHelper.Clamp(m_landerThrustTimer, 0, MAX_THRUST_TIMER);
        float volume = MathHelper.Lerp(0, m_landerThrustTimer, 0.5f) / MAX_THRUST_TIMER;
        m_engineSound.Volume = volume;
        if (volume == 0) m_engineSound.Pause();

        // Moving FPS
        if (elapsed > 0)
        {
            for (int i = 0; i < m_movingFPS.Length - 1; i++)
                m_movingFPS[i] = m_movingFPS[i + 1];
            m_movingFPS[^1] = (int)(1000 / elapsed);
        }

        m_particleSystemFire.Update(gameTime);
        m_particleSystemSmoke.Update(gameTime);
    }

    private enum CollisionType
    {
        Terrain,
        LandingZone,
        None
    }

    private CollisionType CollisionDetector()
    {
        List<Point> corners = m_lander.Corners;
        List<Line> landerLines = new();
        for (int i = 0; i < corners.Count; i++)
            landerLines.Add(new(corners[i].ToVector2(), corners[(i + 1) % corners.Count].ToVector2()));

        CollisionType collision = CollisionType.None;
        foreach (Line terrainLine in m_lines)
            foreach (Line landerLine in landerLines)
                if (Line.Intersecting(landerLine, terrainLine))
                    if (m_landingZones.Contains(terrainLine))
                        // Don't immediatly return for landing zone, we could still be colliding with terrain
                        collision = CollisionType.LandingZone;
                    else
                        return CollisionType.Terrain;

        return collision;
    }

    private void EnterPressed(GameTime gameTime, float value)
    {
        if (m_gameState == GamePlayState.Win)
            NewLevel();
        if (m_gameState == GamePlayState.Lose)
            Reload();
    }

    public override void Render(GameTime gameTime)
    {
        m_spriteBatch.Begin();

        Rectangle rect = new(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight);
        m_spriteBatch.Draw(
            m_backgroundTex,
            rect,
            null,
            Color.White
        );

        m_spriteBatch.End();

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

        m_spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        m_landerRectSpriteSource.X = 0;
        if (m_landerThrustTimer > 250)
            m_landerRectSpriteSource.X = m_landerRectSpriteSource.Width * 2;
        else if (m_landerThrustTimer > 0)
            m_landerRectSpriteSource.X = m_landerRectSpriteSource.Width;

        m_spriteBatch.Draw(
            m_landerTex,
            m_landerRect,
            m_landerRectSpriteSource,
            m_lander.Destroyed ? Color.Transparent : Color.White,
            m_lander.AngleYAxis,
            new Vector2(m_landerRectSpriteSource.Width / 2, m_landerRectSpriteSource.Height / 2),
            SpriteEffects.None,
            0
        );

        float textX = m_graphics.PreferredBackBufferWidth * 0.01f;
        float textY = textX;
        float speed = ScaleNumber(m_lander.Speed, MeasurementType.Velocity, false);
        string text = $"Speed:";
        textX = m_graphics.PreferredBackBufferWidth * 0.01f;

        Vector2 stringSize = m_font.MeasureString(text);
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
            m_isSafeSpeed ? Color.LightGreen : Color.White
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

        float angle = MathHelper.ToDegrees(Math.Abs(m_lander.AngleYAxis));
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
            m_isSafeAngle ? Color.LightGreen : Color.White
        );

        textX = m_graphics.PreferredBackBufferWidth * 0.01f;
        textY += stringSize.Y + 10;

        text = $"Fuel:";
        stringSize = m_font.MeasureString(text);
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.White
        );
        textX += stringSize.X;

        float fuel = m_lander.Fuel;
        text = $"{fuel,7:0.00}%";
        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            fuel > 0 ? Color.LightGreen : Color.White
        );

        text = $"LEVEL {m_level}";
        stringSize = m_fontBig.MeasureString(text);
        textX = m_graphics.PreferredBackBufferWidth * 0.5f - (stringSize.X / 2);
        textY = m_graphics.PreferredBackBufferHeight * 0.01f;

        m_spriteBatch.DrawString(
            m_fontBig,
            text,
            new Vector2(textX, textY),
            Color.White
        );

        float sum = 0;
        foreach (float num in m_movingFPS)
            sum += num;
        float fps = sum / m_movingFPS.Length;

        text = $"FPS: {fps,7:0.}";
        stringSize = m_font.MeasureString(text);
        textX = (int)(m_graphics.PreferredBackBufferWidth * 0.99) - stringSize.X;
        textY = m_graphics.PreferredBackBufferHeight * 0.01f;

        m_spriteBatch.DrawString(
            m_font,
            text,
            new Vector2(textX, textY),
            Color.White
        );

        textX = m_graphics.PreferredBackBufferWidth * 0.50f;
        textY = m_graphics.PreferredBackBufferHeight * 0.50f;

        text = m_gameState switch
        {
            GamePlayState.Transition => $"{(int)m_transitionTimer / 1000 + 1}",
            GamePlayState.Playing => "",
            GamePlayState.Paused => throw new NotImplementedException(),
            GamePlayState.Win => "MISSION SUCCESS\nPRESS ENTER TO PLAY NEXT LEVEL",
            GamePlayState.Lose => "MISSION FAILED",
            GamePlayState.End => "End. :)",
            _ => throw new NotImplementedException(),
        };

        stringSize = m_fontBig.MeasureString(text);
        m_spriteBatch.DrawString(
            m_fontBig,
            text,
            new Vector2(textX - (stringSize.X / 2), textY),
            Color.White
        );


        m_spriteBatch.End();

        m_renderFire.Render(m_particleSpriteBatch, m_particleSystemFire);
        m_renderSmoke.Render(m_particleSpriteBatch, m_particleSystemSmoke);
    }

    private class Lander
    {
        public Vector2 Position { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Point> Corners
        {
            get
            {
                Point tl = new();
                Point tr = new();
                Point br = new();
                Point bl = new();
                float angle = AngleXAxis + MathHelper.PiOver2;
                float halfWidth = Width / 2;
                float halfHeight = Height / 2;

                // https://stackoverflow.com/questions/41898990/find-corners-of-a-rotated-rectangle-given-its-center-point-and-rotation
                // - Needed to swap top and bottom since Y-axis is inverted
                bl.X = (int)(Position.X - ((halfWidth) * Math.Cos(angle)) - ((halfHeight - Height * 0.1f) * Math.Sin(angle)));
                bl.Y = (int)(Position.Y - ((halfWidth) * Math.Sin(angle)) + ((halfHeight - Height * 0.1f) * Math.Cos(angle)));

                br.X = (int)(Position.X + ((halfWidth) * Math.Cos(angle)) - ((halfHeight - Height * 0.1f) * Math.Sin(angle)));
                br.Y = (int)(Position.Y + ((halfWidth) * Math.Sin(angle)) + ((halfHeight - Height * 0.1f) * Math.Cos(angle)));

                tr.X = (int)(Position.X + ((halfWidth) * Math.Cos(angle)) + ((halfHeight) * Math.Sin(angle)));
                tr.Y = (int)(Position.Y + ((halfWidth) * Math.Sin(angle)) - ((halfHeight) * Math.Cos(angle)));

                tl.X = (int)(Position.X - ((halfWidth) * Math.Cos(angle)) + ((halfHeight) * Math.Sin(angle)));
                tl.Y = (int)(Position.Y - ((halfWidth) * Math.Sin(angle)) - ((halfHeight) * Math.Cos(angle)));

                return new()
                {
                    tl,
                    tr,
                    br,
                    bl,
                };
            }
        }
        public Vector2 Velocity { get; set; } // px/ms
        public float AngVelocity { get; private set; }
        public Vector2 Direction { get; private set; } // positive y thrusts up
        private readonly float m_rotationForce = 1.5f / 1e6f;
        public float ThrustAccel { get; private set; }
        public float Speed { get { return Velocity.Length(); } } // px/ms

        /// <summary>
        ///  Angle with respect to X-axis 
        /// </summary>
        public float AngleXAxis { get { return LunarMath.DirectionToAngle(Direction); } }
        /// <summary>
        ///  Angle with respect to Y-axis 
        /// </summary>
        public float AngleYAxis { get { return LunarMath.DirectionToAngle(new Vector2(-Direction.Y, Direction.X)); } }
        public float Fuel { get; private set; }
        private float m_fuelRate;

        public bool UsingThrust { get; private set; }
        public bool Destroyed { get; set; }
        public bool Landed { get; set; }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Position += Velocity * elapsed;
            Direction = LunarMath.AngleToDirection(AngleXAxis + AngVelocity * elapsed);
            UsingThrust = false;
        }

        public void Thrust(GameTime gameTime, float value)
        {
            if (Fuel == 0) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Velocity += ThrustAccel * Direction * elapsed;
            AngVelocity -= AngVelocity * 0.001f * elapsed;
            UsingThrust = true;
            Fuel -= m_fuelRate * elapsed;
            Fuel = Fuel < 0 ? 0 : Fuel;
        }

        public void Rotate(GameTime gameTime, float value, bool clockwise)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            float changeVel = m_rotationForce * elapsed;
            if (clockwise)
                AngVelocity += changeVel;
            else
                AngVelocity -= changeVel;
        }

        public void Set(Vector2 position, Vector2 direction, int width, int height, float thrustAccel)
        {
            Destroyed = false;
            Landed = false;
            Position = position;
            Velocity = new(0, 0);
            AngVelocity = 0;
            direction.Y = -direction.Y;
            Direction = Vector2.Normalize(direction);
            Fuel = 100;
            m_fuelRate = thrustAccel * 300; // Arbitrary
            Width = width;
            Height = height;
            ThrustAccel = thrustAccel;
        }
    }
}

