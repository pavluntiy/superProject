using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace superProject
{
    class Cleared : State
    {

        public Cleared(Game1 parent, ref GraphicsDeviceManager graphics, ref SpriteBatch spriteBatch, ref GraphicsDevice device)
            : base()
        {
            this.parentGame = parent;
            this.graphics = graphics;
            this.spriteBatch = spriteBatch;
            this.device = device;
            ButtonsInit();
            Initialize();
            LoadContent();
        }

        protected void ButtonsInit()
        {
            List<Button> bl = new List<Button>();

            bl.Add(new Button(this, "Exit", new Vector2(250, 300), 150, 20, Color.Yellow, Color.Gray));

            currentButton = 0;
            previousButtonColor = Color.Gray;
            buttons = bl.ToArray();

            currentButton = 0;
            previousButtonColor = Color.Gray;
            buttons[0].backgroundColor = Color.Green;
        }

        protected override void Initialize()
        {
            data = new Dictionary<string, string>();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            font = parentGame.Content.Load<SpriteFont>("myFont");
            base.LoadContent();
        }
        public override void UpdateAll(GameTime gameTime)
        {
            if (!enoughTimePassed(gameTime))
            {
                return;
            }

            var currentKeyBoardState = Keyboard.GetState();
            if (currentKeyBoardState.IsKeyDown(Keys.Escape))
            {
                parentGame.exitToMenu();
            }


            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                switchButtons(1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                switchButtons(-1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                switch (buttons[currentButton].text)
                {
                    case "Exit": parentGame.exitToMenu(); break;
                }

            }
            base.UpdateAll(gameTime);
        }

        
        public override void DrawAll(GameTime gameTime)
        {
            device.Clear(Color.BlanchedAlmond);

            foreach (var button in buttons)
            {
                button.Draw();
            }

            writeMessage("You won!", new Vector2(512, 256), Color.Navy);
            writeMessage("You have scored:", new Vector2(512, 296), Color.Navy);
            writeMessage("Score " + this.data["score"], new Vector2(512, 360), Color.Navy);
            writeMessage("Lives " + this.data["lives"], new Vector2(512, 400), Color.Navy);
            writeMessage("Total " + this.data["total"], new Vector2(512, 440), Color.Navy);

            base.DrawAll(gameTime);
        }
    }
}

