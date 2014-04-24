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

    class Ball
    {
        protected Gaming parentGaming; //Used to report the game, that it should change the state.
        public Model model;
        public Quaternion rotationQuaternion;
        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public BoundingSphere boundingSphere;
        public Vector3 Home;
        public Vector3 rotation;
        public float angle;
        public float radius = 1.0f; //radius of the bounding sphere.
        

        public enum Material {Marble, Plastic, Stone, Idle};//Materials, which are available for ball. Idle is used only in switches to create nice default branches.
        public Dictionary<Material, Texture2D> textures;
        public Material currentMaterial;
        float maxVelocity = 60.0f;


       public int lives;
       public int score;

       public int keysLeft = 0;
        public bool justDied;
        public float minHeight; //After getting lower minHeight, ball dies

        
        public Ball(Model model, Gaming parentGaming)
        {
            this.parentGaming = parentGaming;
            this.model = model;

            this.boundingSphere = new BoundingSphere(this.position, this.radius);
            
            this.rotationQuaternion = Quaternion.Identity;
            this.velocity = new Vector3(0, 0, 0);
            this.textures = new Dictionary<Material,Texture2D>();

            setMaterial(Material.Marble);


        }

        public void setData(int lives, int score, float minHeight)
        {
            this.lives = lives; 
            this.score = score;
            this.minHeight = minHeight;
        }

        public void setPosition(Vector3 position){
            this.Home = this.position = position;
        }

        public bool applyBonus(Bonus.BonusType type, Vector3 bonusPosition)
        {

            if (type == Bonus.BonusType.End)
            {
                if (keysLeft > 0)
                {
                    return false;
                }
                parentGaming.levelCleared();
                
            }


            if (type == Bonus.BonusType.Key)
            {
                this.keysLeft--;
            }
            if (type == Bonus.BonusType.Live)
            {
                this.lives++;
            }

            if (type == Bonus.BonusType.Score)
            {
                this.score += 1000;
            }


            if (type == Bonus.BonusType.Save)
            {
                this.Home = bonusPosition;
            }

            return true;
        }
        public void setMaterial(Material material)
        {
            this.currentMaterial = material;
            if (material == Material.Marble)
            {
                this.mass = 1f;
            }

            if (material == Material.Plastic)
            {
                this.mass = 0.5f;
            }

            if (material == Material.Stone)
            {
                this.mass = 5.0f;
            }
        }

  

        public void applyForce(Vector3 force, GameTime time)
        {

            if (this.currentMaterial == Ball.Material.Marble)
            {
                force.Y = Math.Min(2.5f, force.Y); //Limitations for Marble ball. It shouldn't fly in a usual way.
            }
            var previousVelocity = this.velocity;
            var timePassed = (float)time.ElapsedGameTime.TotalSeconds;

            this.velocity += force / mass * timePassed;


            if (this.currentMaterial == Ball.Material.Marble && this.velocity.Y > 0.5f && this.velocity.Y - previousVelocity.Y > 0)
            {
                this.velocity.Y = previousVelocity.Y;
            }

            if (this.velocity.Length() > maxVelocity ) //Limitation for max velocity.
            {
                this.velocity = previousVelocity;
            }
        }

        public void applyImpuls(Vector3 impuls)
        {
            this.velocity = impuls / this.mass; //Applying impuls back after interacting with world elements.
        }

        
        public void Reset()//Is called after death or pressing Home key.
        {
            this.position = this.Home;
            this.stop(); 
            this.boundingSphere.Center = this.position;
        }
        public void update(GameTime time)
        {
            this.position += this.velocity * (float)time.ElapsedGameTime.TotalSeconds; 
            this.boundingSphere.Center = this.position;

            this.rotation = this.velocity * MathHelper.ToRadians(-1.5f);
            this.angle += this.velocity.Length() /(-100.0f / this.radius);
            Vector3 axis = Vector3.Cross(this.velocity, Vector3.Up);
            if (axis != Vector3.Zero)
            {
                axis.Normalize();
            }
            rotationQuaternion = Quaternion.CreateFromAxisAngle(axis, angle);

            this.score -= 1;
            if (this.position.Y < minHeight)
            {
                this.die();
            }
           
        }

        public void die()
        {
            this.Reset();
            this.justDied = true;
            this.lives--;
        }

        public bool isDead() //Prevents parentGaming from updating ball, which has just died. It is necessary, because influences and deaths are calculated simultaneously, 
            //so we could unintentionally apply impuls (which was reflected or something like this) to a dead ball.
        {
            if (this.justDied)
            {
                this.justDied = false;
                return true;
            }

            return false;
        }

        public void stop()
        {
            this.velocity = new Vector3(0, 0, 0);
        }
    }
}
