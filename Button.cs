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
    class Button
    {
        State parent;
        public String text;

        Vector2 position;
        int xSize, ySize;
        public Color textColor, backgroundColor;

        public Button(State parent, String text, Vector2 position, int xSize, int ySize, Color textColor, Color backgroundColor)
        {
            this.text = text;
            this.parent = parent;
            this.position = position;
            this.xSize = xSize;
            this.ySize = ySize;
            this.textColor = textColor;
            this.backgroundColor = backgroundColor;
        }

        public void Draw()
        {
            parent.drawRectangle(backgroundColor, xSize, ySize, position);
            parent.writeMessage(text, position, textColor);    
        }

        public bool checkClick(MouseState ms)
        {
            
            if (position.X <= ms.X && ms.X <= position.X + xSize)
            {
                if (position.Y - ySize<= ms.Y && ms.Y <= position.Y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
