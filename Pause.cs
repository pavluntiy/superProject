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

    class Pause : State
    {

        protected void ButtonsInit()
        {


            List<Button> bl = new List<Button>();
            
            bl.Add(new Button(this, "Full Screen", new Vector2(512, 220), 150, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Restart", new Vector2(512, 260), 150, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Exit", new Vector2(512, 300), 150, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Back", new Vector2(512, 340), 150, 20, Color.Yellow, Color.Gray));


            buttons = bl.ToArray();

            currentButton = 0;
            previousButtonColor = Color.Gray;
            buttons[0].backgroundColor = Color.Green;
        }

        public Pause(Game1 parent, ref GraphicsDeviceManager graphics, ref SpriteBatch spriteBatch, ref GraphicsDevice device)
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


        protected override void Initialize()
        {
            previousMouseState = Mouse.GetState();
            parentGame.IsMouseVisible = true;
            parentGame.graphics.ApplyChanges();

            base.Initialize();
        }

        public override void UpdateAll(GameTime gameTime)
        {

            if (!enoughTimePassed(gameTime))
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                parentGame.restoreGaming();
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
                        case "Back": parentGame.restoreGaming(); break;
                        case "Exit": parentGame.exitToMenu(); break;
                        case "Restart": parentGame.resetGaming(); break;
                        case "Full Screen": parentGame.setFullScreen(); break;
                    }
                

            }

            base.UpdateAll(gameTime);
        }

        protected override void LoadContent()
        {
            font = parentGame.Content.Load<SpriteFont>("myFont");
            base.LoadContent();
        }

        public override void DrawAll(GameTime gameTime)
        {


            device.Clear(Color.LightCoral);

            writeMessage("Game Paused", new Vector2(20, 20), Color.Beige);
            foreach (var button in buttons)
            {
                button.Draw();
            }




            base.DrawAll(gameTime);
        }
    }
}