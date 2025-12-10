using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ScreenSaverFna
{
    public class ScreenSaverGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _background;
        private Texture2D[] _snowflakeTextures;

        private Snowflake[] _snowflakes;
        private readonly Random _rnd = new Random();

        private KeyboardState _prevKb;
        private MouseState _prevMs;

        public ScreenSaverGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            // --- Полноэкранный режим ---
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background = Content.Load<Texture2D>("village.jpg");

            _snowflakeTextures = new[]
            {
                Content.Load<Texture2D>("q.png"),
                Content.Load<Texture2D>("w.png")
            };

            InitSnowflakes(260);
        }

        private void InitSnowflakes(int count)
        {
            var screenW = _graphics.PreferredBackBufferWidth;
            var screenH = _graphics.PreferredBackBufferHeight;

            _snowflakes = new Snowflake[count];

            for (var i = 0; i < count; i++)
            {
                var tex = _snowflakeTextures[_rnd.Next(_snowflakeTextures.Length)];

                var layer = (float)_rnd.NextDouble();
                var scale = MathHelper.Lerp(0.2f, 1.6f, layer);
                var speed = MathHelper.Lerp(40f, 350f, layer);

                var pos = new Vector2(
                    (float)_rnd.NextDouble() * screenW,
                    (float)_rnd.NextDouble() * screenH
                );

                var rotation = (float)_rnd.NextDouble() * MathHelper.TwoPi;
                var rotSpeed = (float)(_rnd.NextDouble() * 0.6 - 0.3);

                _snowflakes[i] = new Snowflake
                {
                    Texture = tex,
                    Position = pos,
                    Speed = speed,
                    Scale = scale,
                    Rotation = rotation,
                    RotationSpeed = rotSpeed,
                    Layer = layer
                };
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            var ms = Mouse.GetState();

            if (kb.GetPressedKeys().Length > 0 && _prevKb.GetPressedKeys().Length == 0)
                Exit();

            if ((ms.LeftButton == ButtonState.Pressed || ms.RightButton == ButtonState.Pressed) &&
                (_prevMs.LeftButton == ButtonState.Released && _prevMs.RightButton == ButtonState.Released))
                Exit();

            _prevKb = kb;
            _prevMs = ms;

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var screenW = _graphics.PreferredBackBufferWidth;
            var screenH = _graphics.PreferredBackBufferHeight;

            for (var i = 0; i < _snowflakes.Length; i++)
            {
                var s = _snowflakes[i];

                var drift = (float)Math.Sin((s.Position.Y + s.Position.X) * 0.001 + s.Layer * 10f)
                            * (15f * (1f - s.Layer));

                s.Position.X += drift * dt;
                s.Position.Y += s.Speed * dt;

                s.Rotation += s.RotationSpeed * dt;

                if (s.Position.Y > screenH + 50 || s.Position.X < -200 || s.Position.X > screenW + 200)
                {
                s.Position.X = (float)_rnd.NextDouble() * screenW;
                s.Position.Y = -(float)_rnd.NextDouble() * 200 - 10;

                s.Layer = (float)_rnd.NextDouble();
                s.Scale = MathHelper.Lerp(0.2f, 1.6f, s.Layer);
                s.Speed = MathHelper.Lerp(30f, 350f, s.Layer);
            }

            _snowflakes[i] = s;
        }

            base.


Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var dest = new Rectangle(
                0, 0,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight
            );

            _spriteBatch.Draw(_background, dest, Color.White);

            foreach (var s in _snowflakes)
            {
                var rect = new Rectangle(
                    (int)s.Position.X,
                    (int)s.Position.Y,
                    (int)(32 * s.Scale),
                    (int)(32 * s.Scale)
                );

                _spriteBatch.Draw(
                    s.Texture,
                    rect,
                    null,
                    Color.White * MathHelper.Lerp(0.5f, 1f, s.Layer),
                    s.Rotation,
                    new Vector2(s.Texture.Width / 2f, s.Texture.Height / 2f),
                    SpriteEffects.None,
                    0f
                );
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}