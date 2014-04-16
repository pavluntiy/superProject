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
    
    class Menu : State
    {

        protected  void ButtonsInit(){

            List<Button> bl = new List<Button>();
            bl.Add(new Button(this, "Exit", new Vector2(512, 256), 100, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Play", new Vector2(512, 296), 100, 20, Color.Yellow, Color.Gray));
            bl.Add(new Button(this, "Full Screen", new Vector2(512, 340), 100, 20, Color.Yellow, Color.Gray));


            buttons = bl.ToArray();

            currentButton = 0;
            previousButtonColor = Color.Gray;
            buttons[0].backgroundColor = Color.Green;
        }
        
        public Menu(Game1 parent, ref GraphicsDeviceManager graphics, ref SpriteBatch spriteBatch, ref GraphicsDevice device):base()
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



 /*           MouseState mouseState = Mouse.GetState();

            if (previousMouseState.LeftButton == ButtonState.Released
            && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (buttons[0].checkClick(mouseState))
                {
                    parentGame.Exit();
                }
            }
*/
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
                if (buttons[currentButton].text == "Exit")
                {
                    parentGame.Exit();
                }

                if (buttons[currentButton].text == "Play")
                {
                    parentGame.setLevelSelection();
                }

                if (buttons[currentButton].text == "Full Screen")
                {
                    parentGame.setFullScreen();
                }
            }

            previousMouseState = Mouse.GetState();
            base.UpdateAll(gameTime);
        }

        public override void DrawAll(GameTime gameTime)
        {


            device.Clear(Color.DarkSlateBlue);

            writeMessage("Best game ever created!!!! ", new Vector2(20, 20), Color.Beige);
            foreach (var button in buttons)
            {
                button.Draw();
            }

            base.DrawAll(gameTime);
        }
    }
}
