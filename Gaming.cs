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
    class Gaming : State
    {


        BasicEffect effect;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Ball ball;
        Effect skyBoxEffect;
        float gravityAcceleration;
        float forceCoef;


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
       

            Ray ray = new Ray();
            onEdge = true;
            float? length;
            Vector3 A = triangle[0], B = triangle[1], C = triangle[2];
            Vector3 side = B - A;
            ray = new Ray(A, Vector3.Normalize(side));
            float distSq = 0;
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    return true;
                }
            }

            side = C - A;
            ray.Direction = Vector3.Normalize(side);
            length = null;
            sphere.Intersects(ref ray, out length);
            if (length != null)
            {
                distSq = (float)length * (float)length;
                if (length > 0 && distSq < side.LengthSquared())
                {
                    return true;
                }
            }

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
                    return true;
                }
            }
            if (sphere.Contains(A) != ContainmentType.Disjoint ||
                sphere.Contains(B) != ContainmentType.Disjoint ||
                sphere.Contains(C) != ContainmentType.Disjoint)
            {
                return true;
            }
 
            onEdge = false;
            ray.Position = sphere.Center;

            ray.Direction = -getNormalToTriangle(ref triangle);


            Intersects(ref ray, ref triangle, out length);
            if (length != null && length > 0 && length < sphere.Radius)
            {
                return true;
            }
            
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

            effect = new BasicEffect(device);
           textures = new Dictionary<Material,Texture2D>();
            textures[Material.Wood] = parentGame.Content.Load<Texture2D>("wood");
            textures[Material.Metal] = parentGame.Content.Load<Texture2D>("metal");
            textures[Material.Slime] = parentGame.Content.Load<Texture2D>("slime");
            textures[Material.Lava] = parentGame.Content.Load<Texture2D>("lava");


            skyBoxEffect = parentGame.Content.Load<Effect>("skyBoxEffect");
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

            setBonuses();
            SetUpCamera();
            SetModel();
            SetUpVertices();
            DrawStaticWorld();

        }

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
                            effect.View = viewMatrix;
                            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
                        }
                        mesh.Draw();
                    }


                }
            }
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


         if (strings[0] == "pyramid")
         {
                    
                list.Add(new WorldTriangle(V1, V2, V3, currentMaterial));

                list.Add(new WorldTriangle(V2, V4, V3, currentMaterial));

                list.Add(new WorldTriangle(V2, V1, V4, currentMaterial));

                list.Add(new WorldTriangle(V1, V3, V4, currentMaterial));

         }

         if (strings[0] == "plain")
         {

             list.Add(new WorldTriangle(V1, V2, V3, currentMaterial));

             list.Add(new WorldTriangle(V1, V4, V3, currentMaterial));

             list.Add(new WorldTriangle(V3, V2, V1, currentMaterial));

             list.Add(new WorldTriangle(V3, V4, V1, currentMaterial));

         }
        
         
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
                    effect.World = ballTransforms[mesh.ParentBone.Index] * Matrix.CreateFromQuaternion(ball.rotationQuaternion) * Matrix.CreateTranslation(ball.position);
                    effect.View = viewMatrix;
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
                }
                mesh.Draw();
            }


        }
        //Setting world stuff:

        WorldTriangle[] staticTriangles;

        protected void setBall(String str)
        {
            string[] strings = str.Split(new Char[] { '\n', ' ' });

            ball.setMaterial(convertToBallMaterials(getMaterial(strings[1])));
            Vector3 position = new Vector3((float)Convert.ToDouble(strings[3]), (float)Convert.ToDouble(strings[4]), (float)Convert.ToDouble(strings[5]));
            ball.setData(lives: Convert.ToInt32(strings[7]), score: Convert.ToInt32(strings[9]), minHeight: (float)Convert.ToDouble(strings[11])); 
            ball.setPosition(position);
        }
        private void setWorldConstants(String str)
        {
            string[] strings = str.Split(new Char[] { '\n', ' ' });
            this.gravityAcceleration = (float)Convert.ToDouble(strings[1]);
            this.forceCoef = (float)Convert.ToDouble(strings[3]);
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
                line = sr.ReadLine();
                setWorldConstants(line);
               while (!sr.EndOfStream)
               {
                   line = sr.ReadLine();

                   if (line == "")
                   {
                       continue;
                   }
                   WorldTriangle[] triangles = getPositions(line);

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

                       triangleList.AddRange(triangles);
                      
                   }
               }
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
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
        }

        Vector3 up, forward, right;
        private void setUpVectors(){
            this.up = new Vector3(0, 1, 0);
            this.forward = ball.position - this.cameraPosition;
            forward.Y  = 0; 
            forward.Normalize();
            this.right = Vector3.Cross(this.forward, this.up);
            this.right.Normalize();
        }

        private void UpdateCamera()
        {
            cameraPosition = ball.position + currentCameraDelta;
            setUpVectors();
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
                    var currentForceComponent = -cos * force.Length() * normal;

                    bool tmp = usedNormals.ContainsKey(normal);
                    if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1 && cos <= 0)
                    {
                        forceResult += currentForceComponent;
                    }
               
                    
                    if (impuls == Vector3.Zero)
                    {
                        return;
                    }
                    cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
                    var currentImpulsComponent = -cos * impuls.Length() * normal; //<--WTF?!
                    //WTF?!

                    remainingImpuls = 0.999f * remainingImpuls;

                    if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1 && cos <= 0)
                    {
                        remainingImpuls += currentImpulsComponent;
                    }

                    
                    if (Math.Abs(remainingImpuls.Y) < 10E-5)
                    {
                        remainingImpuls.Y = 0;
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

            var currentForceComponent = -cos * force.Length() * normal;


            bool tmp = usedNormals.ContainsKey(normal);
            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1 && cos <= 0)
            {
                forceResult += currentForceComponent;
            }



            if (impuls == Vector3.Zero)
            {
                return;
            }
            cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
            var currentImpulsComponent = -cos * impuls.Length() * normal; //<--WTF?!
            //WTF?!

            remainingImpuls = 0.999f * remainingImpuls;

             if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1 && cos <= 0)
            {
                remainingImpuls += currentImpulsComponent;
            }

            if (Math.Abs(remainingImpuls.Y) < 10E-5)
            {
                remainingImpuls.Y = 0;
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
            var currentForceComponent = -cos * force.Length() * normal;
            bool tmp = usedNormals.ContainsKey(normal);
            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                forceResult += currentForceComponent;
            }


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
            

            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                impulsResult += currentImpulsComponent;
                remainingImpuls += currentImpulsComponent;

            }


            if (Math.Abs(remainingImpuls.Y) < 10E-5)
            {
                remainingImpuls.Y = 0;
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

            if (ball.currentMaterial == Ball.Material.Plastic)
            {
                ball.die();
            }
            float cos = Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(force));
            if (force == Vector3.Zero)
            {
                cos = 0.0f;
            }
            var currentForceComponent = -cos * force.Length() * normal;


            normal = -normal;
            bool tmp = usedNormals.ContainsKey(normal);
            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                forceResult += 0.8f * currentForceComponent;
            }

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

              remainingImpuls = 0.9f * remainingImpuls;

            if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1)
            {
                remainingImpuls += currentImpulsComponent;

            }

            if (Math.Abs(remainingImpuls.Y) < 10E-5)
            {
                remainingImpuls.Y = 0;
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
           
            impuls = impulsResult + remainingImpuls;
            force += forceResult;
        }

       

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

            Vector3 gravity = new Vector3(0, gravityAcceleration, 0);
            Vector3 impuls = ball.velocity * ball.mass;
            force +=  gravity* ball.mass; 
            
            calculateCollisions(ref staticTriangles, ref ball, ref force, ref impuls);

            if (ball.position.Y <= ball.minHeight)
            {
                ball.die();
                
            }

            checkBonusCollisions();
            if (ball.lives <= 0)
            {
                ExitLevel();
            }

            applyAirResistance(ref force);
            if (!ball.isDead())
            {
                ball.applyImpuls(impuls);
                ball.applyForce(force, gameTime);
                ball.update(gameTime);
            }
            base.Update(gameTime);
        }

        protected void DrawStaticWorld()
        {
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
                }
            }
        }

        protected void DrawSky()
        {
            foreach (EffectPass pass in skyBoxEffect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in skyBoxModel.Meshes)
                {
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
                    mesh.Draw();
                }
            }
        }
 

        protected void DrawBallData()
        {
   
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
            rs.FillMode = FillMode.Solid;
            device.RasterizerState = rs;

            effect.TextureEnabled = true;

            effect.World = Matrix.Identity;
            
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

            effect.LightingEnabled = true;
            effect.EnableDefaultLighting();

            device.SetVertexBuffer(vertexBuffer);
            
            UpdateCamera();

            
            DrawBall();
            DrawStaticWorld();
            DrawSky();
            
            
            drawBonuses();
            DrawBallData();
            
            base.Draw(gameTime);
        }
    }
    }

