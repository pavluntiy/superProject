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

    class Selection : State
    {

        protected void ButtonsInit()
        {

 
            List<Button> bl = new List<Button>();
            
            bl.Add(new Button(this, "Level 0 (Tutorial)", new Vector2(512, 250), 150, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Level 1", new Vector2(512, 300), 150, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Level 2", new Vector2(512, 340), 150, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Back", new Vector2(512, 380), 150, 20, Color.Yellow, Color.Gray));


            buttons = bl.ToArray();

            currentButton = 0;
            previousButtonColor = Color.Gray;
            buttons[0].backgroundColor = Color.Green;
        }

        public Selection(Game1 parent, ref GraphicsDeviceManager graphics, ref SpriteBatch spriteBatch, ref GraphicsDevice device)
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
                parentGame.Exit();
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
                        case "Level 0 (Tutorial)": parentGame.createGaming("Level0"); break;
                        case "Level 1": parentGame.createGaming("Level1"); break;
                        case "Level 2": parentGame.createGaming("Level2"); break;
                        case "Back": parentGame.exitToMenu(); break;
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

            writeMessage("Choose level, loser!!!!!!!!!!!!", new Vector2(20, 20), Color.Beige);
            foreach (var button in buttons)
            {
                button.Draw();
            }



            
            base.DrawAll(gameTime);
        }
    }
}