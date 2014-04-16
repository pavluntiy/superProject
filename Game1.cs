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



    public class Game1 : Microsoft.Xna.Framework.Game
    {


  //      bool isMenu;
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public GraphicsDevice device;

        public BasicEffect effect;
/*        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix worldMatrix;
        Ball ball;
        Effect skyBoxEffect;
*/
        State state, previousState;
   
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            device = graphics.GraphicsDevice;
     //       this.graphics.IsFullScreen = true;
            
            Content.RootDirectory = "Content";
        }

        public void createGaming(String level){
            this.state = new Gaming(level, this, ref graphics, ref spriteBatch, ref device);
        }

        protected override void Initialize()
        {      

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;
            effect = new BasicEffect(device);

            Content.RootDirectory = "Content";


        //    state = new Cleared(this, ref graphics, ref spriteBatch, ref device);
          //  state = new Menu(this, ref graphics, ref spriteBatch, ref device);
         //   createGaming();
            setMenu();
            base.LoadContent();
        }


        protected override void UnloadContent()
        {

        }

        public void setMenu()
        {
            this.state = new Menu(this, ref graphics, ref spriteBatch, ref device);
        }

        public void setLevelSelection()
        {
            this.state = new Selection(this, ref graphics, ref spriteBatch, ref device);
        }

        public void setFullScreen(){
            if(graphics.IsFullScreen){
                graphics.IsFullScreen = false;
            }
            else {
                graphics.IsFullScreen = true;
            }

            graphics.ApplyChanges();
        }

        public void exitGaming_Loss(){
            this.state = new Loss(this, ref graphics, ref spriteBatch, ref device);
        }

        public void resetGaming()
        {
            this.state = new   Gaming(previousState.name, this, ref graphics, ref spriteBatch, ref device);
        }

        public void restoreGaming()
        {
            this.state = previousState;
        }

        public void setPause()
        {
            this.previousState = this.state;
            this.state = new Pause(this, ref graphics, ref spriteBatch, ref device);
        }



        public void exitToMenu()
        {
            this.state = new Menu(this, ref graphics, ref spriteBatch, ref device);
        }

        public void levelCleared(int score, int lives)
        {
            this.state = new Cleared(this, ref graphics, ref spriteBatch, ref device);
            this.state.data["score"] = score.ToString();
            this.state.data["lives"] = lives.ToString();
            this.state.data["total"] = (score + lives * 1000).ToString();
        }



        protected override void Update(GameTime gameTime)
        {
            if(state != null)
                state.UpdateAll(gameTime);
            base.Update(gameTime);
        }

  
        protected override void Draw(GameTime gameTime)
        {
            if (state != null)
            state.DrawAll(gameTime);
         
            base.Draw(gameTime);
        }

    }
     
}
