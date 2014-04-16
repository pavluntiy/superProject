
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
    class State : Game
    {
        protected int currentButton;
        protected Color previousButtonColor;
        protected Game1 parentGame;
        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;
        protected GraphicsDevice device;

        protected SpriteFont font;

        protected Button[] buttons;
        public Dictionary<String, String> data;
        public MouseState previousMouseState;

        public String name;


        public virtual void drawRectangle(Color color, int xSize, int ySize, Vector2 position)
        {
            Texture2D rect = new Texture2D(graphics.GraphicsDevice, xSize, ySize);

            Color[] data = new Color[xSize * ySize];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = color;
            }
                rect.SetData(data);
             spriteBatch.Begin();
                spriteBatch.Draw(rect, position, Color.White);
             spriteBatch.End();
            
        }

        protected void switchButtons(int d)
        {
            buttons[currentButton].backgroundColor = previousButtonColor;
            currentButton = (currentButton + d + buttons.Length) % buttons.Length;

            previousButtonColor = buttons[currentButton].backgroundColor;
            buttons[currentButton].backgroundColor = Color.Green;
        }

        float timeSpan = 0;

        protected bool enoughTimePassed(GameTime gameTime)
        {
            timeSpan += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSpan < 200f)
            {
                return false;
            }
            timeSpan = 0;
            return true;
        }

        public virtual void writeMessage(String message, Vector2 where, Color color)
        {
            spriteBatch.Begin();
                spriteBatch.DrawString(font, message, where, color);
               spriteBatch.End();
                device.BlendState = BlendState.Opaque;
                device.DepthStencilState = DepthStencilState.Default;
        }
        public virtual void DrawAll(GameTime gameTime)
        {

        }
        public virtual void UpdateAll(GameTime gameTime)
        {

        }

    }
}
