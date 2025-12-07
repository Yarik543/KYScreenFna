using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ScreenSaverFna
{
    public class ScreenSaver : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Текстуры
        Texture2D background;
        Texture2D[] snowflakeTextures;

        // Массив снежинок
        Snowflake[] snowflakes;
        const int SNOW_COUNT = 1200; 

        Random rnd = new Random();

        public ScreenSaver()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Полноэкранный режим
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            // Если хочешь окно — поставь IsFullScreen=false и укажи размеры.
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            try
            {
                background = Texture2D.FromStream(GraphicsDevice, File.OpenRead("Content/village.jpg"));

                snowflakeTextures = new Texture2D[]
                {
                    Texture2D.FromStream(GraphicsDevice, File.OpenRead("Content/q.png")),
                    Texture2D.FromStream(GraphicsDevice, File.OpenRead("Content/w.png"))
                };
            }
            catch (Exception ex)
            {
                // Если файлов нет — создадим простые текстуры (резерв)
                Console.WriteLine("Could not load Content files: " + ex.Message);
                background = new Texture2D(GraphicsDevice, 1, 1);
                background.SetData(new[] { Color.CornflowerBlue });

                var px = new Texture2D(GraphicsDevice, 8, 8);
                Color[] data = new Color[8 * 8];
                for (var i = 0; i < data.Length; i++) data[i] = Color.White;
                px.SetData(data);
                snowflakeTextures = new Texture2D[] { px, px };
            }

            // Инициализация снежинок
            InitSnowflakes();
        }

        // Создает массив снежинок
        private void InitSnowflakes()
        {
            var screenW = graphics.PreferredBackBufferWidth;
            var screenH = graphics.PreferredBackBufferHeight;

            snowflakes = new Snowflake[SNOW_COUNT];
            for (var i = 0; i < SNOW_COUNT; i++)
            {
                // layer — глубина: 0..1 (далее ближе)
                float layer = (float)rnd.NextDouble();

                var tex = snowflakeTextures[rnd.Next(snowflakeTextures.Length)];

                // скорость и масштаб зависят от layer (дальние медленнее и мельче)
                float Scale = MathHelper.Lerp(0.05f, 0.25f, layer);
                float maxSize = 64f;
                float scaleLimit = maxSize / tex.Width;
                float scale = Math.Min(Scale, scaleLimit);
                float speed = MathHelper.Lerp(20f, 420f, layer) * (0.7f + (float)rnd.NextDouble() * 0.6f);

                // позиция случайная по экрану (разброс по Y чтобы не все сверху)
                var pos = new Vector2(
                    (float)rnd.NextDouble() * screenW,
                    (float)rnd.NextDouble() * screenH
                );

                // текстура выбираем случайно из массива

                // вращение/скорость вращения для красоты
                float rotation = (float)rnd.NextDouble() * MathHelper.TwoPi;
                float rotSpeed = (float)(rnd.NextDouble() * 0.6 - 0.3);

                snowflakes[i] = new Snowflake
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

        protected override void UnloadContent()
        {
            // По желанию: освобождение ресурсов
            // background?.Dispose();
            // foreach (var t in snowflakeTextures) t?.Dispose();
            base.UnloadContent();
        }

        KeyboardState prevKb;
        MouseState prevMs;

        protected override void Update(GameTime gameTime)
        {
            // Выход на любую клавишу или клик мышью
            var kb = Keyboard.GetState();
            var ms = Mouse.GetState();

            if (kb.GetPressedKeys().Length > 0 && prevKb.GetPressedKeys().Length == 0)
                Exit();

            if ((ms.LeftButton == ButtonState.Pressed || ms.RightButton == ButtonState.Pressed)
                 && (prevMs.LeftButton == ButtonState.Released && prevMs.RightButton == ButtonState.Released))
                Exit();

            prevKb = kb;
            prevMs = ms;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var screenW = graphics.PreferredBackBufferWidth;
            var screenH = graphics.PreferredBackBufferHeight;

            // Обновление снежинок
            for (var i = 0; i < snowflakes.Length; i++)
            {
                var s = snowflakes[i];

                // горизонтальный дрейф (ветер)
                float drift = (float)Math.Sin((s.Position.Y + s.Position.X) * 0.001 + s.Layer * 10f) * (10f * (1f - s.Layer));
                s.Position.X += drift * dt;
                s.Position.Y += s.Speed * dt;

                s.Rotation += s.RotationSpeed * dt;

                // респаун сверху, когда вышли за экран
                if (s.Position.Y > screenH + 50 || s.Position.X < -200 || s.Position.X > screenW + 200)
                {
                    s.Position.X = (float)rnd.NextDouble() * screenW;
                    s.Position.Y = -(float)rnd.NextDouble() * 200 - 10;
                    // можно немного менять скорость/scale при респавне:
                    s.Layer = (float)rnd.NextDouble();
                    s.Scale = MathHelper.Lerp(0.3f, 1.6f, s.Layer);
                    s.Speed = MathHelper.Lerp(20f, 420f, s.Layer) * (0.7f + (float)rnd.NextDouble() * 0.6f);
                }

                snowflakes[i] = s;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Рисуем фон растянутым по экрану
            Rectangle dest = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            spriteBatch.Draw(background, dest, Color.White);

            // Рисуем снежинки в порядке дальность->близко (можно оптимизировать)
            // Здесь просто рисуем все — порядок не критичен, т.к. все white alpha
            for (var i = 0; i < snowflakes.Length; i++)
            {
                var s = snowflakes[i];
                var origin = new Vector2(s.Texture.Width / 2f, s.Texture.Height / 2f);
                float alpha = MathHelper.Lerp(0.5f, 1f, s.Layer);
                var size = (int)MathHelper.Lerp(12f, 38f, s.Layer); // дальние маленькие, ближние чуть крупнее
                Rectangle rect = new Rectangle(
                    (int)s.Position.X,
                    (int)s.Position.Y,
                    size,
                    size
                );
                spriteBatch.Draw(s.Texture, rect, null, Color.White * alpha);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // ---------- внутренний класс снежинки ----------
        struct Snowflake
        {
            public Texture2D Texture;
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Layer;
        }
    }
}
