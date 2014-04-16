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


//using DigitalRune.Game;

namespace superProject
{

    class Ball
    {
        protected Gaming parentGaming;
        public Model model;

        public Quaternion rotationQuaternion;
        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public BoundingSphere boundingSphere;
        public Vector3 Home;
        public Vector3 rotation;
        public float angle;
        public float radius = 1.0f;
        

        public enum Material {Marble, Plastic, Stone, Idle};
        public Dictionary<Material, Texture2D> textures;
        public Material currentMaterial;



       public int lives = 10;
       public int score = 10000;

        public int keysLeft = 0;


        public bool justDied;
        float minHeight;

        
        public Ball(Model model, Gaming parentGaming)
        {
            this.parentGaming = parentGaming;
            this.model = model;

            this.Home = this.position = new Vector3(2.5f, 11f, 5);
            this.boundingSphere = new BoundingSphere(this.position, this.radius);
            
            this.rotationQuaternion = Quaternion.Identity;
            this.velocity = new Vector3(0, 0, 0);
    //        this.force = new Vector3(0, 0, 0);
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
                force.Y = Math.Min(2.5f, force.Y);
            }
            var previousVelocity = this.velocity;
            var timePassed = (float)time.ElapsedGameTime.TotalSeconds;

            this.velocity += force / mass * timePassed;


            if (this.currentMaterial == Ball.Material.Marble && this.velocity.Y > 0.5f && this.velocity.Y - previousVelocity.Y > 0)
            {
                this.velocity.Y = previousVelocity.Y;
            }

            if (this.velocity.Length() > 60.0f )
            {
                this.velocity = previousVelocity;
            }
            if (this.currentMaterial == Ball.Material.Marble)
            {
        //        velocity.Y = Math.Min(0.5f, velocity.Y);
            }
        }

        public void applyImpuls(Vector3 impuls)
        {
            this.velocity = impuls / this.mass;
        }

        
        public void Reset()
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
 
    //        Vector3 axis = Vector3.Cross(this.velocity, Vector3.Up);
  //          float angle = this.velocity.Length();//factor by delta time if neccesary

//            Quaternion rotationThisFrame = Quaternion.CreateFromAxisAngle(axis, angle * (1 / this.radius));

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

        public bool isDead()
        {
            if (this.justDied)
            {
                this.justDied = false;
                return true;
            }

            return false;
        }
     /*   public void applyGravity(Vector3 acceleration, GameTime time)
        {

            this.velocity += acceleration* (float)time.ElapsedGameTime.TotalSeconds;
        }
*/
        public void stop()
        {
            this.velocity = new Vector3(0, 0, 0);
        }
    }
}
