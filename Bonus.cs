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
    class Bonus
    {
        public Model model;
        public Vector3 position;
        public BoundingSphere boundingSphere;

        public enum BonusType {Live, Score, Save, End, Key, Idle};

        public BonusType type;

        public BonusType stringToType(string typeName){
            if (typeName == "Live")
            {
                return BonusType.Live;
            }

            if(typeName == "Score"){
                return BonusType.Score;
            }

            if (typeName == "Save")
            {
                return BonusType.Save;
            }


            if(typeName == "End"){
                return BonusType.End;
            }

            if (typeName == "Key")
            {
                return BonusType.Key;
            }


            return BonusType.Idle;
        }
        public Bonus(Vector3 position, string typeName)
        {
            this.type = stringToType(typeName);
            this.position = position;
            this.boundingSphere = new BoundingSphere(this.position, 1);
        }
    }
}
