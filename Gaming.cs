﻿using System;
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
    class Gaming : State
    {


        BasicEffect effect;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Ball ball;
        Effect skyBoxEffect;

        enum Material {Wood, Metal, Slime, Lava, Marble, Plastic, Stone, Idle};
        struct WorldTriangle {

           public Vector3 A, B, C;
           public Vector3 normal;
           public Material material;
           public Vector2 textureA, textureB, textureC;
           public int id;

            public WorldTriangle(Vector3 A, Vector3 B, Vector3 C, Material material = Material.Wood){
                this.A = A;
                this.B = B;
                this.C = C;
                this.normal = Vector3.Zero;
                this.material = material;
                this.textureA = this.textureB = this.textureC = Vector2.Zero;
                this.id = 0;
            }

        }


        VertexBuffer vertexBuffer;
        Dictionary<Material, Texture2D> textures;
        Dictionary<Bonus.BonusType, Texture2D> bonusTextures;
        Model skyBoxModel;
        TextureCube skyBoxTexture;



        protected void setup(String name)
        {

            StreamReader sr = new StreamReader(name + ".lvl");

              
        }

        public Gaming(String name, Game1 parent, ref GraphicsDeviceManager graphics, ref SpriteBatch spriteBatch, ref GraphicsDevice device):base()
        {

            this.name = name;
            this.parentGame = parent;
            this.graphics = graphics;
            this.spriteBatch = spriteBatch;
            this.device = device;
            LoadContent();
        }

            public void Intersects(ref Ray ray, ref Vector3[] triangle, out float? distance)
        {
            distance = null;
            Vector3 edge1 = triangle[2] - triangle[1], edge2 = triangle[0] - triangle[1];

            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            Vector3 distanceVector = ray.Position - triangle[1];

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            if (triangleU < 0 || triangleU > 1)
            {
                return;
            }

            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                return;
            }

            float length = 0;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out length);
            distance = length * inverseDeterminant;
        }

        public bool Intersects(ref BoundingSphere sphere, ref Vector3[] triangle, out bool onEdge){
        
        //    bool result = false;
            onEdge = false;
            Vector3 A = triangle[0], B = triangle[1], C = triangle[2];
            // First check if any corner point is inside the sphere
            // This is necessary because the other tests can easily miss
            // small triangles that are fully inside the sphere.
            if (sphere.Contains(A) != ContainmentType.Disjoint ||
                sphere.Contains(B) != ContainmentType.Disjoint ||
                sphere.Contains(C) != ContainmentType.Disjoint)
            {
                // A point is inside the sphere
                
                return true;
            }

            // If we get this far we are not touching the edges of the triangle

            // Calculate the InverseNormal of the triangle from the centre of the sphere
            // Do a ray intersection from the centre of the sphere to the triangle.
            // If the triangle is too small the ray could miss a small triangle inside
            // the sphere hence why the points were tested above.
            Ray ray = new Ray();
            onEdge = false;
            float? length;
            ray.Position = sphere.Center;
            // This will always create a vector facing towards the triangle from the 
            // ray starting point.

            ray.Direction = -getNormalToTriangle(ref triangle);


            Intersects(ref ray, ref triangle, out length);
            if (length != null && length > 0 && length < sphere.Radius)
            {
                // Hit the surface of the triangle
                return true;
            }
            // Test the edges of the triangle using a ray
            // If any hit then check the distance to the hit is less than the length of the side
            // The distance from a point of a small triangle inside the sphere coule be longer
            // than the edge of the small triangle, hence the test for points inside above.
            Vector3 side = B - A;
            // Important:  The direction of the ray MUST
            // be normalised otherwise the resulting length 
            // of any intersect is wrong!
            onEdge = true;
            ray = new Ray(A, Vector3.Normalize(side));
            float distSq = 0;
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    return true;
                }
            }
            // Stay at A and change the direction to C
            side = C - A;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    return true;
                }
            }
            // Change to corner B and edge to C
            side = C - B;
            ray.Position = B;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    // Hit edge
                    return true;
                }
            }
 
            // Only if we get this far have we missed the triangle
            onEdge = false;
            return false;
        }


        protected override void Initialize()
        {
            parentGame.graphics.IsFullScreen = false;
            parentGame.IsMouseVisible = false;
            base.Initialize();
        }

        public Model loadModel(String fileName)
        {
            Model newModel = parentGame.Content.Load<Model>(fileName);
            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect.Clone();
                }
            }

            return newModel;
        }

        public void levelCleared()
        {
            this.parentGame.levelCleared(ball.score, ball.lives);
        }

        protected override void LoadContent()
        {

        //    spriteBatch = new SpriteBatch(GraphicsDevice);
       //     device = graphics.GraphicsDevice;
     //       device = GraphicsDevice;

            effect = new BasicEffect(device);
           textures = new Dictionary<Material,Texture2D>();
            textures[Material.Wood] = parentGame.Content.Load<Texture2D>("wood");
            textures[Material.Metal] = parentGame.Content.Load<Texture2D>("metal");
            textures[Material.Slime] = parentGame.Content.Load<Texture2D>("slime");
            textures[Material.Lava] = parentGame.Content.Load<Texture2D>("lava");


            skyBoxEffect = parentGame.Content.Load<Effect>("skyBoxEffect");
   //         device.Textures[0] = textures[Material.Wood] as Texture;
            ball = new Ball(loadModel("ball"), this);
            textures[Material.Marble] = ball.textures[Ball.Material.Marble] = parentGame.Content.Load<Texture2D>("marble");
            textures[Material.Plastic] = ball.textures[Ball.Material.Plastic] = parentGame.Content.Load<Texture2D>("plastic");
            textures[Material.Stone] = ball.textures[Ball.Material.Stone] = parentGame.Content.Load<Texture2D>("stone");

            bonusTextures = new Dictionary<Bonus.BonusType, Texture2D>();
            bonusTextures[Bonus.BonusType.Live] = parentGame.Content.Load<Texture2D>("live_texture");
            bonusTextures[Bonus.BonusType.Score] = parentGame.Content.Load<Texture2D>("score_texture");
            bonusTextures[Bonus.BonusType.Save] = parentGame.Content.Load<Texture2D>("save_texture");
            bonusTextures[Bonus.BonusType.End] = parentGame.Content.Load<Texture2D>("end_texture");
            bonusTextures[Bonus.BonusType.Key] = parentGame.Content.Load<Texture2D>("key_texture");

            font = parentGame.Content.Load<SpriteFont>("myFont");
            skyBoxTexture = parentGame.Content.Load<TextureCube>(this.name + " " + "sky");


            skyBoxModel = loadModel("SkySphereModel");
         //   skyBoxEffect.Parameters = new EffectParameterCollection();
      //      skyBoxEffect.Parameters["SkyboxTexture"].SetValue(skyBoxTexture);

//            skyBoxTexture = parentGame.Content.Load<TextureCube>(this.name + ' ' + "sky");


            //, out skyboxTextures);
            setBonuses();
            SetUpCamera();
            SetModel();
            SetUpVertices();
            DrawStaticWorld();

   //         var a = GraphicsDevice;
        }




       // List<Bonus> bonuses;
        Dictionary<int, Bonus> bonuses;
        protected void setBonuses()
        {
           

           
             StreamReader sr = new StreamReader(this.name + " " + "bonusesData.txt");
              bonuses = new Dictionary<int, Bonus>();

              int i = 1;
               while (!sr.EndOfStream)
               {
                   String line = sr.ReadLine();
                   String[] properties = line.Split(new Char[] { '\n', ' ' });
                   Vector3 position = new Vector3(
                     (float)Convert.ToDouble(properties[1]),
                     (float)Convert.ToDouble(properties[2]),
                     (float)Convert.ToDouble(properties[3])
                     );

 
                
                   Bonus newBonus = new Bonus(position, properties[0]);
                   newBonus.model = loadModel(properties[0]);
                   if (newBonus.type == Bonus.BonusType.Key)
                   {
                       this.ball.keysLeft++;
                   }

                   bonuses[i] = newBonus;
                   i++;
               }
        }


        protected void drawBonuses()
        {
                    foreach (var currentBonus in bonuses)
                    {
                               Matrix[] currentBonusTransforms = new Matrix[currentBonus.Value.model.Bones.Count];
                               currentBonus.Value.model.CopyAbsoluteBoneTransformsTo(currentBonusTransforms);
                    foreach (ModelMesh mesh in currentBonus.Value.model.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.TextureEnabled = true;
                            effect.Texture = bonusTextures[currentBonus.Value.type];
                            effect.EnableDefaultLighting();
                            effect.CurrentTechnique = effect.Techniques[0];
                            effect.World = currentBonusTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(currentBonus.Value.position);
                            effect.View = viewMatrix;// Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
                        }
                        mesh.Draw();
                    }


                }
            }
/*
        public override void writeMessage(String message, Vector2 where, Color color)
        {
            spriteBatch.Begin();
                spriteBatch.DrawString(font, message,  where, color);
            spriteBatch.End();
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }
 */ 
        private void checkBonusCollisions()
        {
            List<int> toErase = new List<int>();
            foreach(var currentBonusWithId in bonuses)
            {
                int i = currentBonusWithId.Key;
                var currentBonus = currentBonusWithId.Value;
                if (currentBonus.boundingSphere.Intersects(ball.boundingSphere))
                {
                    if (ball.applyBonus(currentBonus.type, currentBonus.position))
                        toErase.Add(i);
                }
                ++i;
            }

            foreach(var currentIndex in toErase){
                bonuses.Remove(currentIndex);
            }
        }
        
        
        private Material getMaterial(string name){
            if (name == "Marble")
            {
                return Material.Marble;
            }
            if (name == "Plastic")
            {
                return Material.Plastic;
            }
            if (name == "Stone")
            {
                return Material.Stone;
            }
            if (name == "Lava")
            {
                return Material.Lava;
            }
            if (name == "Wood")
            {
                return Material.Wood;
            }

            if (name == "Metal")
            {
                return Material.Metal;
            }

            if (name == "Slime")
            {
                return Material.Slime;
            }
            return Material.Wood;
        }

        private WorldTriangle[] getPositions(string input){
            List<WorldTriangle> list = new List<WorldTriangle>();

          //  char separators[] = {'\n', ' '};
            string[] strings = input.Split(new Char[] { '\n', ' ' });

            Vector3 V1 = new Vector3((float)Convert.ToDouble(strings[1]), (float)Convert.ToDouble(strings[2]), (float)Convert.ToDouble(strings[3]));
            Vector3 V2 = new Vector3((float)Convert.ToDouble(strings[4]), (float)Convert.ToDouble(strings[5]), (float)Convert.ToDouble(strings[6]));
            Vector3 V3 = new Vector3((float)Convert.ToDouble(strings[7]), (float)Convert.ToDouble(strings[8]), (float)Convert.ToDouble(strings[9]));
            Vector3 V4 = new Vector3((float)Convert.ToDouble(strings[10]), (float)Convert.ToDouble(strings[11]), (float)Convert.ToDouble(strings[12]));

            Material currentMaterial = getMaterial(strings[13]);
            #region
            if (strings[0] == "cube"){
              
                list.Add(new WorldTriangle(V1 + V3 - V2, V3, V4, currentMaterial));
                list.Add(new WorldTriangle(V2 + V4 - V1, V4, V3, currentMaterial));

                list.Add(new WorldTriangle(V4, V2, V1, currentMaterial));
                list.Add(new WorldTriangle(V2, V4, V2 + V4 - V1, currentMaterial));

               list.Add(new WorldTriangle(V2, V2 + V4 - V1, V3 + V1 - V4, currentMaterial));
               list.Add(new WorldTriangle(V3, V3 + V1 - V4, V2 + V4 - V1, currentMaterial));

                list.Add(new WorldTriangle(V1, V3 + 2 * V1 - V2 - V4, V4, currentMaterial));
                list.Add(new WorldTriangle(V1 + V3 - V2, V4, V3 + 2 * V1 - V2 - V4, currentMaterial));

                list.Add(new WorldTriangle(V1 + V3 - V2, V3 + V1 - V4, V3, currentMaterial));
                list.Add(new WorldTriangle(V3 + V1 - V4, V1 + V3 - V2, 2 * V1 + V3 - V2 - V4, currentMaterial));

                list.Add(new WorldTriangle(V1, V2, V3 + 2 * V1 - V2 - V4, currentMaterial));
                list.Add(new WorldTriangle(V3 + 2 * V1 - V2 - V4, V2, V3 + V1 - V4, currentMaterial));


            }
            #endregion

            if (strings[0] == "wedge")
            {

                list.Add(new WorldTriangle(V1, V3 + V1 - V2, V1 + V4 - V2, currentMaterial));

                list.Add(new WorldTriangle(V2, V4, V3, currentMaterial));

                list.Add(new WorldTriangle(V4, V2, V1, currentMaterial));
                list.Add(new WorldTriangle(V1, V1 + V4 - V2, V4, currentMaterial));

                list.Add(new WorldTriangle(V3, V3 + V1 - V2, V1, currentMaterial));
                list.Add(new WorldTriangle(V1, V2, V3, currentMaterial));

                list.Add(new WorldTriangle(V4, V1 + V4 - V2, V3 + V1 - V2, currentMaterial));
                list.Add(new WorldTriangle(V3 + V1 - V2, V3, V4, currentMaterial));



            }
            #region
            /*     if (strings[0] == "w")
         a{
             Vector3 V1 = new Vector3((float)Convert.ToDouble(strings[1]), (float)Convert.ToDouble(strings[2]), (float)Convert.ToDouble(strings[3]));
             Vector3 V2 = new Vector3((float)Convert.ToDouble(strings[4]), (float)Convert.ToDouble(strings[5]), (float)Convert.ToDouble(strings[6]));
             Vector3 V3 = new Vector3((float)Convert.ToDouble(strings[7]), (float)Convert.ToDouble(strings[8]), (float)Convert.ToDouble(strings[9]));
             Vector3 V4 = new Vector3((float)Convert.ToDouble(strings[10]), (float)Convert.ToDouble(strings[11]), (float)Convert.ToDouble(strings[12]));


             
             
             
             list.Add(V2);
             list.Add(V1);
             list.Add(V4);
             list.Add(V1 + V4 - V2);

            
             
             
             list.Add(V2);
             list.Add(V1);
             list.Add(V3);
             list.Add(V3 + V1 - V2);

             
             
             
             list.Add(V3);
             list.Add(V3 + V1 - V2);
             list.Add(V4);
             list.Add(V1 + V4 - V2);
                      
             list.Add(V3);
             list.Add(V2);
             list.Add(V4);
             list.Add(V2);

             
             
             
             list.Add(V3 + V1 - V2);
             list.Add(V1);
             list.Add(V1 + V4 - V2);
             list.Add(V1);

             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));

             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));


         }

         if (strings[0] == "p")
         {
             Vector3 V1 = new Vector3((float)Convert.ToDouble(strings[1]), (float)Convert.ToDouble(strings[2]), (float)Convert.ToDouble(strings[3]));
             Vector3 V2 = new Vector3((float)Convert.ToDouble(strings[4]), (float)Convert.ToDouble(strings[5]), (float)Convert.ToDouble(strings[6]));
             Vector3 V3 = new Vector3((float)Convert.ToDouble(strings[7]), (float)Convert.ToDouble(strings[8]), (float)Convert.ToDouble(strings[9]));
             Vector3 V4 = new Vector3((float)Convert.ToDouble(strings[10]), (float)Convert.ToDouble(strings[11]), (float)Convert.ToDouble(strings[12]));


             
             list.Add(V1);
             list.Add(V4);
                
             list.Add(V1);
             list.Add(V2);
             
             
             


             list.Add(V3);
             list.Add(V2);
             list.Add(V4);
             list.Add(V3);
             
             
             
            
             



            
             list.Add(V1);
             list.Add(V3);
             list.Add(V4);
             list.Add(V3);
            
             
             
             



             list.Add(V1);
             list.Add(V3);
             list.Add(V1);
             list.Add(V2);
             
             

             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));

             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));
             list.Add(new Vector3(0f, 0f, 0f));

         }
        
            */
#endregion
         return list.ToArray();
        }


        private Vector3 normalize(Vector3 v)
        {
            if (v == Vector3.Zero)
            {
                return Vector3.Zero;
            }
            v.Normalize();
            return v;

        }
        private Vector3 normalize(Vector3 a, Vector3 b){
            var tmp = Vector3.Cross(b, a);
            tmp.Normalize();
            if (tmp == Vector3.Zero)
            {
                return Vector3.Zero;
            }
            return tmp;
        }


        private void SetModel(){
           
        }

        float angle = 0;

        private void DrawBall()
        {
      //      worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(ballRotation) * Matrix.CreateTranslation(ballPosition);
        //    worldMatrix *= Matrix.CreateScale(0.005f);

            Matrix[] ballTransforms = new Matrix[ball.model.Bones.Count];
            ball.model.CopyAbsoluteBoneTransformsTo(ballTransforms);
            foreach (ModelMesh mesh in ball.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.Texture = ball.textures[ball.currentMaterial];
                    effect.EnableDefaultLighting();
                    effect.CurrentTechnique = effect.Techniques[0];
                    effect.World = ballTransforms[mesh.ParentBone.Index] * Matrix.CreateFromQuaternion(ball.rotationQuaternion) * Matrix.CreateTranslation(ball.position);//Matrix.CreateFromAxisAngle(rotAxis, angle); 
                    effect.View = viewMatrix;// Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
                }
                mesh.Draw();
            }


        }

        WorldTriangle[] staticTriangles;

        protected void setBall(String str)
        {
            string[] strings = str.Split(new Char[] { '\n', ' ' });

            ball.setMaterial(convertToBallMaterials(getMaterial(strings[1])));
            Vector3 position = new Vector3((float)Convert.ToDouble(strings[3]), (float)Convert.ToDouble(strings[4]), (float)Convert.ToDouble(strings[5]));
            ball.setData(lives: Convert.ToInt32(strings[7]), score: Convert.ToInt32(strings[9]), minHeight: (float)Convert.ToDouble(strings[11])); 
            ball.setPosition(position);
        }
        private void SetUpVertices()
        {
           
          
            
           
               StreamReader sr = new StreamReader(name + ' ' + "levelData.txt");
               List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();
                List<WorldTriangle> triangleList = new List<WorldTriangle>();
                String line = sr.ReadLine();
                line = sr.ReadLine();
                setBall(line);
                line = sr.ReadLine();
               while (!sr.EndOfStream)
               {
                   line = sr.ReadLine();

                   if (line == "")
                   {
                       continue;
                   }
                   WorldTriangle[] triangles = getPositions(line);

            //       Vector3[] currentTriangle = new Vector3[3];
                   for (var i = 0; i < triangles.Length; ++i )
                   {
                       ;
                       Vector3 normal = normalize(triangles[i].C - triangles[i].A, triangles[i].A - triangles[i].B);
                       triangles[i].normal = normal;
                       verticesList.Add(new VertexPositionNormalTexture(triangles[i].A, normal, new Vector2(10, 0)));
                       verticesList.Add(new VertexPositionNormalTexture(triangles[i].B, normal, new Vector2(0, 0)));
                       verticesList.Add(new VertexPositionNormalTexture(triangles[i].C, normal, new Vector2(0, 10)));
                       triangles[i].textureA = new Vector2(1f, 0 );
                       triangles[i].textureB = new Vector2(0, 0);
                       triangles[i].textureC = new Vector2(0, 1f);
                       //         currentTriangle[0] = triangle.A;
                       //       currentTriangle[1] = triangle.B;
                       //     currentTriangle[2] = triangle.C ;

                       //     triangleList.Add(new Vector3[] {triangle.A, triangle.B, triangle.C});
                       triangleList.AddRange(triangles);
                       /*                    currentTriangle = new Vector3[3];

                                           verticesList.Add(new VertexPositionNormalTexture(vertexes[4 * i + 2 - 1], normal, new Vector2(5, 0)));
                                           verticesList.Add(new VertexPositionNormalTexture(vertexes[4 * i + 4 - 1], normal, new Vector2(5, 0)));
                                           verticesList.Add(new VertexPositionNormalTexture(vertexes[4 * i + 3 - 1], normal, new Vector2(0, 0)));

                       
                                           currentTriangle[0] = vertexes[4 * i + 2 - 1];
                                           currentTriangle[1] = vertexes[4 * i + 4 - 1];
                                           currentTriangle[2] = vertexes[4 * i + 3 - 1];
                       

                                           triangleList.Add(currentTriangle);

                    */
                   }
               }

               #region
               //front wall
               /*                        verticesList.Add(new VertexPositionNormalTexture(vertexes[5], normalize(vertexes[6], vertexes[7]), new Vector2(0, 0)));
                                   verticesList.Add(new VertexPositionNormalTexture(vertexes[6],  normalize(vertexes[5], vertexes[7]), new Vector2(0, 0)));
                                   verticesList.Add(new VertexPositionNormalTexture(vertexes[7], normalize(vertexes[6], vertexes[5]), new Vector2(0, 0)));

                                   verticesList.Add(new VertexPositionNormalTexture(vertexes[6], normalize(vertexes[7], vertexes[8]), new Vector2(0, 0)));
                                   verticesList.Add(new VertexPositionNormalTexture(vertexes[8], normalize(vertexes[6], vertexes[7]), new Vector2(0, 0)));
                                   verticesList.Add(new VertexPositionNormalTexture(vertexes[7], normalize(vertexes[6], vertexes[8]), new Vector2(0, 0)));

                                       //back wall
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, buildingHeights[currentbuilding], -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));

                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, buildingHeights[currentbuilding], -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, buildingHeights[currentbuilding], -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));

                                       //left wall
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, buildingHeights[currentbuilding], -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));

                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, buildingHeights[currentbuilding], -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, buildingHeights[currentbuilding], -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));

                                       //right wall
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, buildingHeights[currentbuilding], -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));

                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, buildingHeights[currentbuilding], -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                                       verticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, buildingHeights[currentbuilding], -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                * 
                *
                           }
                    */
               #endregion

               vertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, verticesList.Count, BufferUsage.WriteOnly);

            vertexBuffer.SetData<VertexPositionNormalTexture>(verticesList.ToArray());
           staticTriangles = triangleList.ToArray();
        }
        Vector3 cameraPosition;
        Vector3 currentCameraDelta;
        private void rotateCamera(GameTime gameTime)
        {
            if (!enoughTimePassed(gameTime))
            {
                return;
            }
            if (currentCameraDelta == new Vector3(-10.0f, 10.0f, 0.0f))
            {
                currentCameraDelta = new Vector3(0.0f, 10.0f, 10.0f);
                return;
            }

            if (currentCameraDelta == new Vector3(0.0f, 10.0f, 10.0f))
            {
                currentCameraDelta = new Vector3(10.0f, 10.0f, 0.0f);
                return;
            }

            if (currentCameraDelta == new Vector3(10.0f, 10.0f, 0.0f))
            {
                currentCameraDelta = new Vector3(0.0f, 10.0f, -10.0f);
                return;
            }

            if (currentCameraDelta == new Vector3(0.0f, 10.0f, -10.0f))
            {
                currentCameraDelta = new Vector3(-10.0f, 10.0f, 0.0f);
                return;
            }
            
        }
        private void SetUpCamera()
        {
            setUpVectors();
            currentCameraDelta = new Vector3(-10.0f, 10.0f, 0.0f);
            cameraPosition = ball.position + currentCameraDelta;
//            viewMatrix = Matrix.CreateLookAt(new Vector3(30, 20, -5), new Vector3(8, 0, -7), new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
        }

        Vector3 up, forward, right;
        private void setUpVectors(){
            this.up = new Vector3(0, 1, 0);
            this.forward = ball.position - this.cameraPosition;
           // this.forward = -this.currentCameraDelta;
            forward.Y  = 0; 
            forward.Normalize();

            this.right = Vector3.Cross(this.forward, this.up);
            //Mystery happens here!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //If forward == (-1, 0, 0)
    /*        if (currentCameraDelta == new Vector3(10.0f, 10.0f, 0.0f))
            {
                this.right = new Vector3(0, 0, 1);
            }
     */ 
            this.right.Normalize();
       //     this.right.Z = Math.Abs(this.right.Z);
        }
        private void UpdateCamera()
        {
      //      cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateFromQuaternion(ballRotation));
            cameraPosition = ball.position + currentCameraDelta;


            setUpVectors();
         //   Vector3 upVectorOfBall = Vector3.Transform(new Vector3(0, 1, 0), Matrix.CreateFromQuaternion(ballRotation));
      //      viewMatrix = Matrix.CreateLookAt(new Vector3(30, 20, -5), new Vector3(8, 0, -7), new Vector3(0, 1, 0));
            viewMatrix = Matrix.CreateLookAt(cameraPosition, ball.position, up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
        }


        protected Vector3 getNormalToTriangle(ref Vector3[] triangle)
        {
            var tmp = Vector3.Cross(triangle[2] - triangle[0], triangle[1] - triangle[0]);
            if (tmp == new Vector3(0, 0, 0))
            {
                return new Vector3(0, 0, 0); 
            }
            tmp.Normalize();
            return tmp;
        }
        Vector3 gravity = new Vector3(0, -10, 0);

        private void interactWithWood(ref Vector3 force, ref Vector3 impuls, 
            ref Vector3 forceResult, ref Vector3 impulsResult,
            ref Vector3 remainingForce, ref Vector3 remainingImpuls,
            ref Dictionary<Vector3, int> usedNormals,
            ref bool onEdge, ref Vector3 normal)
        {

                   
                    float cos = Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(force));
                    if (force == Vector3.Zero)
                    {
                        cos = 0.0f;
                    }
             //       forceResult += -gravity.Length() * ball.mass * triangleNormal * cos;
                    var currentForceComponent = -cos * force.Length() * normal;
                    if (onEdge)
                    {
                        int a = 0;
                    }

      /*              if (usedNormals.ContainsKey(triangleNormal))
                    {
                        continue;
                    }
*/
                    bool tmp = usedNormals.ContainsKey(normal);// && usedNormals[triangleNormal] == 1;
                    if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
                    {
                        forceResult += currentForceComponent;
      //                  forces.Add(currentForceComponent);
                    }
                    else
                    {
                        int a = 0;
                    }
          
                  //  impulsResult += Vector3.Reflect(impuls, triangleNormal);
                    
                    if (impuls == Vector3.Zero)
                    {
                        return;
                    }
                    cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
                    var currentImpulsComponent = -cos * impuls.Length() * normal; //<--WTF?!
                    //WTF?!
                  //  impulsResult += currentComponent;
                    /*if (!usedNormals.Contains(triangleNormal) || cos)
                    {
                     */

                    remainingImpuls = 0.999f * remainingImpuls;

                    if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
                    {
                        //               impulsResult += currentImpulsComponent;
                        remainingImpuls += currentImpulsComponent;

                    }
                    else
                    {
                        int a = 0;
                    }
                    
                    if (Math.Abs(remainingImpuls.Y) < 10E-5)
                    {
                        remainingImpuls.Y = 0;
                    }

                    if (forceResult.Y <= 0 || remainingImpuls.Y < 0)
                    {
                        int a = 0;
                    }
                    
                    
                    if(!usedNormals.ContainsKey(normal)){
                        usedNormals.Add(normal, 1);
                    }
                    else {
                        usedNormals[normal]++;
                    }

                    
                
        }

        private void interactWithTransformer(Material material, ref Vector3 force, ref Vector3 impuls,
         ref Vector3 forceResult, ref Vector3 impulsResult,
         ref Vector3 remainingForce, ref Vector3 remainingImpuls,
         ref Dictionary<Vector3, int> usedNormals,
         ref bool onEdge, ref Vector3 normal)
        {

            float cos = Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(force));
            if (force == Vector3.Zero)
            {
                cos = 0.0f;
            }
            //       forceResult += -gravity.Length() * ball.mass * triangleNormal * cos;
            var currentForceComponent = -cos * force.Length() * normal;
            if (onEdge)
            {
                int a = 0;
            }

            /*              if (usedNormals.ContainsKey(triangleNormal))
                          {
                              continue;
                          }
      */
            bool tmp = usedNormals.ContainsKey(normal);// && usedNormals[triangleNormal] == 1;
            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                forceResult += currentForceComponent;
                //                  forces.Add(currentForceComponent);
            }
            else
            {
                int a = 0;
            }

            //  impulsResult += Vector3.Reflect(impuls, triangleNormal);

            if (impuls == Vector3.Zero)
            {
                return;
            }
            cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
            var currentImpulsComponent = -cos * impuls.Length() * normal; //<--WTF?!
            //WTF?!
            //  impulsResult += currentComponent;
            /*if (!usedNormals.Contains(triangleNormal) || cos)
            {
             */

            remainingImpuls = 0.999f * remainingImpuls;

            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                //               impulsResult += currentImpulsComponent;
                remainingImpuls += currentImpulsComponent;

            }
            else
            {
                int a = 0;
            }

            if (Math.Abs(remainingImpuls.Y) < 10E-5)
            {
                remainingImpuls.Y = 0;
            }

            if (forceResult.Y <= 0 || remainingImpuls.Y < 0)
            {
                int a = 0;
            }


            if (!usedNormals.ContainsKey(normal))
            {
                usedNormals.Add(normal, 1);
            }
            else
            {
                usedNormals[normal]++;
            }

            ball.setMaterial(convertToBallMaterials(material));

        }

        Ball.Material convertToBallMaterials(Material material)
        {
            if (material == Material.Marble)
            {
                return Ball.Material.Marble;
            }

            if (material == Material.Plastic)
            {
                return Ball.Material.Plastic;
            }

            if (material == Material.Stone)
            {
                return Ball.Material.Stone;
            }

            return Ball.Material.Idle;
        }

        private void interactWithMetal(ref Vector3 force, ref Vector3 impuls,
          ref Vector3 forceResult, ref Vector3 impulsResult,
          ref Vector3 remainingForce, ref Vector3 remainingImpuls,
          ref Dictionary<Vector3, int> usedNormals,
          ref bool onEdge, ref Vector3 normal)
        {
  

            float cos = Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(force));
            if (force == Vector3.Zero)
            {
                cos = 0.0f;
            }
            //       forceResult += -gravity.Length() * ball.mass * triangleNormal * cos;
            var currentForceComponent = -cos * force.Length() * normal;
            if (onEdge)
            {
                int a = 0;
            }

            /*              if (usedNormals.ContainsKey(triangleNormal))
                          {
                              continue;
                          }
      */
            bool tmp = usedNormals.ContainsKey(normal);// && usedNormals[triangleNormal] == 1;
            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                forceResult += currentForceComponent;
                //                  forces.Add(currentForceComponent);
            }
            else
            {
                int a = 0;
            }

            //  impulsResult += Vector3.Reflect(impuls, triangleNormal);

            if (impuls == Vector3.Zero)
            {
                return;
            }
            cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
            if (impuls == Vector3.Zero)
            {
                cos = 0.0f;
            }
            var currentImpulsComponent = -cos * impuls.Length() * normal; 
            
            //  impulsResult += currentComponent;
            /*if (!usedNormals.Contains(triangleNormal) || cos)
            {
             */

        //    remainingImpuls = 0.999f * remainingImpuls;

            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                impulsResult += currentImpulsComponent;
                remainingImpuls += currentImpulsComponent;

            }
            else
            {
                int a = 0;
            }

            if (Math.Abs(remainingImpuls.Y) < 10E-5)
            {
                remainingImpuls.Y = 0;
            }

            if (forceResult.Y <= 0 || remainingImpuls.Y < 0)
            {
                int a = 0;
            }


            if (!usedNormals.ContainsKey(normal))
            {
                usedNormals.Add(normal, 1);
            }
            else
            {
                usedNormals[normal]++;
            }



        }

        private void interactWithLava(ref Vector3 force, ref Vector3 impuls,
       ref Vector3 forceResult, ref Vector3 impulsResult,
       ref Vector3 remainingForce, ref Vector3 remainingImpuls,
       ref Dictionary<Vector3, int> usedNormals,
       ref bool onEdge, ref Vector3 normal)
        {

            if (ball.currentMaterial != Ball.Material.Stone)
            {
                ball.die();
            }

        }




        private void interactWithSlime(ref Vector3 force, ref Vector3 impuls,
        ref Vector3 forceResult, ref Vector3 impulsResult,
        ref Vector3 remainingForce, ref Vector3 remainingImpuls,
        ref Dictionary<Vector3, int> usedNormals,
        ref bool onEdge, ref Vector3 normal)
        {
        //    normal = -normal;

            if (ball.currentMaterial == Ball.Material.Plastic)
            {
                ball.die();
            }
            float cos = Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(force));
            if (force == Vector3.Zero)
            {
                cos = 0.0f;
            }
            //       forceResult += -gravity.Length() * ball.mass * triangleNormal * cos;
            var currentForceComponent = -cos * force.Length() * normal;


            normal = -normal;
            bool tmp = usedNormals.ContainsKey(normal);// && usedNormals[triangleNormal] == 1;
            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                forceResult += 0.8f * currentForceComponent;
                //                  forces.Add(currentForceComponent);
            }
            else
            {
                int a = 0;
            }

            //  impulsResult += Vector3.Reflect(impuls, triangleNormal);

            if (impuls == Vector3.Zero)
            {
                return;
            }
            cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
            if (impuls == Vector3.Zero)
            {
                cos = 0.0f;
            }
            var currentImpulsComponent = -cos * impuls.Length() * normal; //<--WTF?!
            //WTF?!
            //  impulsResult += currentComponent;
            /*if (!usedNormals.Contains(triangleNormal) || cos)
            {
             */

              remainingImpuls = 0.9f * remainingImpuls;

            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
           //     impulsResult += currentImpulsComponent;
                remainingImpuls += currentImpulsComponent;

            }
            else
            {
                int a = 0;
            }

            if (Math.Abs(remainingImpuls.Y) < 10E-5)
            {
                remainingImpuls.Y = 0;
            }

            if (forceResult.Y <= 0 || remainingImpuls.Y < 0)
            {
                int a = 0;
            }


            if (!usedNormals.ContainsKey(normal))
            {
                usedNormals.Add(normal, 1);
            }
            else
            {
                usedNormals[normal]++;
            }



        }
        private void calculateCollisions(ref WorldTriangle[] triangles, ref Ball ball, ref Vector3 force, ref Vector3 impuls){

            Dictionary<Vector3, int> usedNormals = new Dictionary<Vector3, int>();
            HashSet<Vector3> forces = new HashSet<Vector3>();
            bool interactionExpired = false;
            Vector3 remainingForce = Vector3.Zero;
            Vector3  impulsResult = new Vector3(0, 0, 0), forceResult = new Vector3(0, 0, 0), remainingImpuls = impuls;
            foreach(var triangle in triangles){
                Vector3[] ct = new Vector3[]{triangle.A, triangle.B, triangle.C};
                Vector3 normal = triangle.normal;
                bool onEdge;
                

                if (Intersects(ref ball.boundingSphere, ref ct, out onEdge))
                {
                    if (normal == Vector3.Zero)
                    {
                        continue;
                    }
                    if (triangle.material == Material.Wood)
                    {
                        interactWithWood(ref force, ref impuls, ref forceResult, ref impulsResult, ref remainingForce, ref remainingImpuls, ref usedNormals, ref onEdge, ref normal);
                    }
                    if (triangle.material == Material.Metal)
                    {
                        interactWithMetal(ref force, ref impuls, ref forceResult, ref impulsResult, ref remainingForce, ref remainingImpuls, ref usedNormals, ref onEdge, ref normal);
                    }

                    if (triangle.material == Material.Lava)
                    {
                        interactWithLava(ref force, ref impuls, ref forceResult, ref impulsResult, ref remainingForce, ref remainingImpuls, ref usedNormals, ref onEdge, ref normal);
                    }
                    if (triangle.material == Material.Slime)
                    {
                        interactWithSlime(ref force, ref impuls, ref forceResult, ref impulsResult, ref remainingForce, ref remainingImpuls, ref usedNormals, ref onEdge, ref normal);
                    }

                    if (triangle.material == Material.Marble || 
                        triangle.material == Material.Plastic ||
                        triangle.material == Material.Stone)
                    {
                        interactWithTransformer(triangle.material, ref force, ref impuls, ref forceResult, ref impulsResult, ref remainingForce, ref remainingImpuls, ref usedNormals, ref onEdge, ref normal);
                    }
                }
                   
            }
            if (impuls.Z != 0)
            {
                int b = 0;
            }
            if (interactionExpired)
            {
                
            }

           

            impuls = impulsResult + remainingImpuls;
            force += forceResult;
        }

        float forceCoef = 50f;

        protected void ExitLevel()
        {
            parentGame.exitGaming_Loss();
        }

        protected void applyAirResistance(ref Vector3 force){
            force += normalize(-ball.velocity) * 0.2f * ball.velocity.Length();
        }
        public override void UpdateAll(GameTime gameTime)
        {
            enoughTimePassed(gameTime);

            KeyboardState keyState = Keyboard.GetState();

      //      Vector3 rotVect = new Vector3(1, 2, 3);
    //        ball.rotation += 0.005f * rotVect;
            Vector3 force = new Vector3(0, 0, 0);

            if(keyState.IsKeyDown(Keys.Escape))
            {
               parentGame.setPause();
            }
            if (keyState.IsKeyDown(Keys.RightControl) || keyState.IsKeyDown(Keys.LeftControl))
            {
                rotateCamera(gameTime);
                setUpVectors();
            }
           

            if (keyState.IsKeyDown(Keys.PageUp))
                force += forceCoef * this.up;
            if (keyState.IsKeyDown(Keys.PageDown))
                force -= forceCoef * this.up;

            
            if (keyState.IsKeyDown(Keys.Up))
                force += forceCoef * this.forward;
            if (keyState.IsKeyDown(Keys.Down))
                force -= forceCoef * this.forward;

            if (keyState.IsKeyDown(Keys.Right))
                force += forceCoef * this.right;
            if (keyState.IsKeyDown(Keys.Left))
                force -= forceCoef * this.right;

            if(keyState.IsKeyDown(Keys.End)){
                ball.stop();
            }

            if (keyState.IsKeyDown(Keys.Home))
            {
                ball.Reset();
            }


            Vector3 impuls = ball.velocity * ball.mass;
            force += gravity * ball.mass; 
            
            calculateCollisions(ref staticTriangles, ref ball, ref force, ref impuls);

            if (ball.position.Y <= -60.0f)
            {
                ball.die();
                
            }

            checkBonusCollisions();
            if (ball.lives <= 0)
            {
                ExitLevel();
            }

        //    rotAxis = ball.position;//new Vector3(impuls.Z, 0.0f, impuls.X);
         //   rotAxis.Normalize();
 //           angle = impuls.Length() / 1000;
            applyAirResistance(ref force);
            if (!ball.isDead())
            {
                ball.applyImpuls(impuls);
                ball.applyForce(force, gameTime);
                ball.update(gameTime);
            }
          //  base.Update(gameTime);
        }

        protected void DrawStaticWorld()
        {
        /*    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(vertexBuffer);
                effect.Texture = textures[Material.Wood];
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
            }
            */
           foreach (var triangle in staticTriangles)
            {

                effect.Texture = textures[triangle.material];  

                VertexBuffer vertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, 3, BufferUsage.WriteOnly);
                vertexBuffer.SetData(new VertexPositionNormalTexture[] {
                    new VertexPositionNormalTexture( triangle.A, triangle.normal, triangle.textureA), 
                    new VertexPositionNormalTexture( triangle.B, triangle.normal, triangle.textureB), 
                     new VertexPositionNormalTexture( triangle.C, triangle.normal, triangle.textureC), 
                });

               VertexPositionNormalTexture[] vertexes = new VertexPositionNormalTexture[]{
                    new VertexPositionNormalTexture( triangle.A, triangle.normal, triangle.textureA), 
                    new VertexPositionNormalTexture( triangle.B, triangle.normal, triangle.textureB), 
                     new VertexPositionNormalTexture( triangle.C, triangle.normal, triangle.textureC), 
                };

               foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.SetVertexBuffer(vertexBuffer);
                    effect.Texture = textures[triangle.material];
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertexes, 0, 1);
              //      device.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                }
            }



        }

        protected void DrawSky()
        {
          /* skyBoxEffect.Parameters["ViewMatrix"].SetValue(viewMatrix);
            skyBoxEffect.Parameters["ProjectionMatrix"].SetValue(projectionMatrix);
            // Draw the sphere model that the effect projects onto
            foreach (ModelMesh mesh in skyBoxModel.Meshes)
            {
                mesh.Draw();
            }
       */
       

            foreach (EffectPass pass in skyBoxEffect.CurrentTechnique.Passes)
            {
                // Draw all of the components of the mesh, but we know the cube really
                // only has one mesh
                foreach (ModelMesh mesh in skyBoxModel.Meshes)
                {
                    // Assign the appropriate values to each of the parameters
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = skyBoxEffect;
                        part.Effect.Parameters["World"].SetValue(
                            Matrix.CreateScale(300f) * Matrix.CreateTranslation(cameraPosition));
                        part.Effect.Parameters["View"].SetValue(viewMatrix);
                        part.Effect.Parameters["Projection"].SetValue(projectionMatrix);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }

                    // Draw the mesh with the skybox effect
                    mesh.Draw();
                }
            }
        }
        protected void DrawMenu(GameTime gameTime){
        }

        protected void DrawBallData()
        {
   
           /* spriteBatch.Begin();
               spriteBatch.DrawString(font, "Lives " + this.ball.lives.ToString(), new Vector2(20, 20), Color.Beige);
                spriteBatch.DrawString(font, "Score " + this.ball.score.ToString(), new Vector2(20, 60), Color.Beige);
            spriteBatch.End();
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            */

            this.writeMessage("Lives " + this.ball.lives.ToString(), new Vector2(20, 20), Color.Beige);
            this.writeMessage("Score " + this.ball.score.ToString(), new Vector2(20, 60), Color.Beige);
            this.writeMessage("Keys to collect " + this.ball.keysLeft.ToString(), new Vector2(20, 100), Color.Beige);
            if (this.ball.position.Y <= -35 && this.ball.velocity.Y <= -2.5)
            {
                this.writeMessage("Be careful!", new Vector2(150, 50), Color.Beige);
            }
        }

        public override void DrawAll(GameTime gameTime)
        {
            device.Clear(Color.DarkSlateBlue);

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            //   rs.FillMode = FillMode.WireFrame;
            rs.FillMode = FillMode.Solid;
            device.RasterizerState = rs;

            effect.TextureEnabled = true;
    //        Vector3 rotAxis = new Vector3(3 * angle, angle, 2 * angle);
  //          rotAxis.Normalize();
   //        Matrix worldMatrix = Matrix.CreateTranslation(-2.0f / 3.0f, -1.0f / 3.0f, 0) * Matrix.CreateFromAxisAngle(rotAxis, angle);
            effect.World = Matrix.Identity;
            
   //         effect.World = worldMatrix;
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

     //       effect.Texture = texture;
            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();

            device.SetVertexBuffer(vertexBuffer);
            
            UpdateCamera();

            
            DrawBall();
            DrawStaticWorld();
            DrawSky();
            
            
            drawBonuses();
            DrawBallData();
            
        //    base.Draw(gameTime);
        }
    }
    }

