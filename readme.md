
#Description of project

##Aims and intentions

I was going to make something like Balance, an old game by Atatri. For this I decided to familiarize with basics of 3D Graphics. I used XNA for .NET platform instead of adhering to my initial intention to use Qt wrappers for OpenGl, because they seemed  inefficient and I thought it is a great idea to study a new programing language (C#).

I didn't manage to clone Balance completely because it has many quite time-consuming features, but I believe that the plot is correctly transferred and impressions are not so dreadful as I could have expected.

###About XNA
XNA support quite nice 3D graphics. The main its advantage is that it allows drawing textured triangles in a very simple way, loading models (actually, the only model I have used is model of a sphere) and it also provides checking for collisions of two bounding spheres. Unfortunately, XNA was not created to draw beautiful GUIs, so menus of my game were painted programmatically.

##Used algorithms
As it was announced in the begining, my main intention wasn't to familiarize to any powerful algorithms. I was going to dive into 3D graphics and creating nice-looking (relatively) games.

I used algorithms of intersection of ball and a triangle. I found in the Internet one that served an example for me, but I had to change it drastically. I have also created some algos (functions) for calculating interactions between static blocks and the ball. You could have noticed some displays of its imperfectness: ball behaves in a very strange way on edges of blocks. In order to create such algorithms I used physics (mechanics) like law of impuls conservation and second Newton's law. I also used conception of state machine to organize my menus and selections.

Ball in my game obeys laws of impuls conservation. If it hits appropriate surfaces (metal), it reflects properly. Wood and other types of materials extinguish complitly normal component of impuls vector.

Second Newton's law is also performed properly. When you apply force (or gravitation does) ball gets acceleration according to widely known formula.

I have also added a rotation effect. When the ball is rolling, its surface also rotates in an appropriate way. 

I created several classes for organization of my game. Most of them are designed to provide menus and selection screens. I also used structure, that stores position, normal and texture.

I also have to confess that conception of encapsulation was deeply offended during creating this project.

##User's guide

###Rules
You are able to control a ball by applying forces to it in all three dimensions, so that it would get proper acceleration and, consequently, velocity.

Your aim is to collect all silver spheres (they are keys) in order to finish level by touching black sphere. You also can pick up patchy colored balls, they add time to your timer. Blue spheres are lives and coral ones are checkpoints; if you gather a checkpoint ball, you will start your next attempt (if you have extra lives) from the last checkpoint.

You should avoid falling down and touching some types of blocks (like lava).
In the upper-left corner of the screen you can see counters of time, lives and keys left before you can leave the level.

World consists of blocks, which consist of different materials.

There are four types of block:
* Parallelepipeds
* Pyramids
* Wedges
* Plains

There exist several types of materials:
* Wood. Collision is perfectly inelastic, exists friction.
* Metal. Collision is perfectly elastic, no friction.
* Lava. Lethal for all types of balls except the stone ones.
* Slime. It has the biggest measure of friction, it absorbs balls which get to it. Fatal for plastic balls.

There are also different possibilities for player's ball for changing type of material, which lead to changes of its manner of physical interactions.
* Marble. Slightly orange. It has medium weight and a tiny flying skill. The most easy controlled ball.
* Stone. It has bloody red color. The heaviest one. It has ability to pass through lava without interacting with it.
* Plastic. It is white and it has the smallest weight, so you are able to fly with it without limitations.

###Controls
* Arrows are used to apply force in horizontal plane in the respecting direction.
* PageUp and PageDown keys are used to control vertical behavior
* Home key is used to teleport instantly to the location of the last checkpoint (without any fines for usage).
* End key is used for instant stop. (Velocity is assigned to zero). This key wasn't expected to survive up to the release version.
* Control keys are used to rotate camera.
* Escape has different meanings which depend on current state of game. During gaming it pauses the process.

###Menu system
When the game is launched you can see a pretty well-designed menu. It is the main menu of the game. You are able to choose desired item by using arrows, when you select an item it is highlited, now you are able to press enter key to confirm your selection. By default, the exit item is selected. In the main menu escape button will have the same effect as selecting exit.
Other menus have similar structure. 

When you are playing you can pause your game by pressing escape. Second hit to escape button (as well as selecting the correspoding item) will return you back to the game.
When you won you can see screen with your results.
When you loose, you are informed about this fact.

##Programmer's guide
You are able to extend the game.
You can add new levels and modify menus in a very simple way.

###Adding new level
You should create file names "LevelX levelData.txt". where LevelX is name of your level.
This file contains information about world.
It has the following structure:
ball:
material:  _initial_material_of_ball_ position: _initial_position_ lives: _initial_quantity_of_lives_ score: _initial_score_ minHeight: _minimal_save height_
level:
gravity:  _acceleration_of_gravity_ force_coef: _absolute_values_of_forces_apllied_by_player_
_type_of_block_ _coordinates_of_block_ _material_

You can choose from following block types: 
* "cube". Parrallelipiped, actually.
* "wedge"
* "pyramid"
* "plain"
_coordinates_of_block_ is enumeration of four different point in space. Each point is given by three coordinates in space (X, Y, Z) without any delimeters between points or their coordinates! You also shouldn't use additional newlines and spaces.

The second file that has to be created is bonus data file. It has name "LevelX bonusesData.txt".  It has a quite similar structure.
	...
_bonus_type_ _coordinates_
	...
Bonus types are {Save, Live, Score, End, Key}. You can add them to game without any limitations.
Coordinates are (X, Y, Z) enumerated without any delimiters or additional spaces. You need't specify how many bonuses you are going to add.

You can define coordinates in floating-point format.

###Defining a button.
You should also add a button into Selection.cs file as you add a new level.
In order to do this go to function ButtonsInit() and add a button like it is stated there. You should understand, that buttons are selected by arrows in order of their enumeration in that function.
Then you go to function UpdateAll() and add a new brach to the swicth operator.
These metodics are appropriate for all classes inherited from State class.


###Some words about code
	
My projects consists of twelve classes. One of them is main class of project, one (Game1) is the main class of ideology, particularily it contains the "state machine" which is responsible for switching between menus and gaming. The last interesting class is Ball, which maintains playing entity. Other classes are virtual class State, from which I derive other classes of state such as:

* Menu. It is the main menu of the game
* Gaming. It is the state of gaming. It contains the main plot of game. All intersections are processed here, and everything (during gaming process) is drawn by this class itself. 
* Selection. Menu for level selection.
* Loss. Appears when you lose.
* Cleared. Appears when you win.
* If you have read up to this place, please report me.
* Pause. It has a very eloquent name.

All these classes communicate with their parent (Game1) by callbacks, and they have references to their parent's fields, which are used for performing drawing routines.

Important note. Yes, classes Game1 and Program were initially pre-generated during creating the project. But now they do not have anything from their original condition.

Let us look up to several functions.

<div style = "font-family: courier">calculateCollisions</div> can be calles one of the most important functions of the class Gaming:
````csharp
	        private void calculateCollisions(ref WorldTriangle[] triangles, ref Ball ball, ref Vector3 force, ref Vector3 impuls){

            Dictionary<Vector3, int> usedNormals = new Dictionary<Vector3, int>(); 
            Vector3 remainingForce = Vector3.Zero;
            Vector3  impulsResult = new Vector3(0, 0, 0), forceResult = new Vector3(0, 0, 0), remainingImpuls = impuls;
            foreach(var triangle in triangles){ //we check if we intersect with each triangle of the world.
                Vector3[] ct = new Vector3[]{triangle.A, triangle.B, triangle.C};
                Vector3 normal = triangle.normal;
                bool onEdge; //It is very important to process edges accurately
                

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
````

Functions interactWithSomething are also very important, it was quite a hard work to make these functions work plausibly. As you can have already seen, there are still some strange events on the edges, but they appear in a very irregular way.

Let us look at this bunch of crutches!

````csharp
	private void interactWithWood(ref Vector3 force, ref Vector3 impuls, 
            ref Vector3 forceResult, ref Vector3 impulsResult,
            ref Vector3 remainingForce, ref Vector3 remainingImpuls,
            ref Dictionary<Vector3, int> usedNormals,
            ref bool onEdge, ref Vector3 normal)
        {

                   
                    float cos = Vector3.Dot(Vector3.Normalize(normal), Vector3.Normalize(force)); //We calculate cosine between the normal vector of the 
                                                                                                  //triangle and the force vector in order to determine 
                    if (force == Vector3.Zero)                                                    //if we need to apply this force
                    {
                        cos = 0.0f;
                    }
                    var currentForceComponent = -cos * force.Length() * normal;

                    //Here we make some magic. We have to process situation of seam of two triangles in one plain.
                    //If we do not check it, our ball will either fall through the seam or get double acceleration or impuls
                    if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1 && cos <= 0)
                    {
                        forceResult += currentForceComponent;
                    }
                    //Simultaniously we check if we have to apply this force, so the ball wouldn't be absorbed.
               
                    //We impuls is zero, we exit, because we have alreadey applied force (it is a reference variable), and we don't need to reflect zero impuls.
                    if (impuls == Vector3.Zero)
                    {
                        return;
                    }
                    cos = Vector3.Dot(Vector3.Normalize(impuls), normal);
                    var currentImpulsComponent = -cos * impuls.Length() * normal; 


                    remainingImpuls = 0.999f * remainingImpuls; //It is friction. I was going do make a changeable coefficient, but even with such a small (0.001) measure of frcition the ball slows down drastically.

                    if (!usedNormals.ContainsKey(normal) && cos <= 0 && !onEdge || onEdge && usedNormals.ContainsKey(normal) && usedNormals[normal] == 1 && cos <= 0)//Here we do the same thing, as we did before with force, with impuls.
                    {
                        remainingImpuls += currentImpulsComponent;
                    }

                    
                   if (Math.Abs(remainingImpuls.Y) < 10E-5)//It is necessary because the ball starts unprovoked moving due to imperfectness of floating-point operations
                    {
                        remainingImpuls.Y = 0;
                    }
            

                               
                    if(!usedNormals.ContainsKey(normal)){//We add used normals to make our magic work.
                        usedNormals.Add(normal, 1);
                    }
                    else {
                        usedNormals[normal]++;
                    }

                    
                
        }
````

So, now you are able to make your impression about ideology of collision processing.

The most interesting class, Gaming, is also derived from class State, so it implements its virtual methods as UpdateAll and draw all.

UpdateAll:

````csharp

	        public override void UpdateAll(GameTime gameTime)
        {
 
            enoughTimePassed(gameTime); //It is necessary to provide acceptable spans between two user's actions, like hitting a button of rotation

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
````

So, in UpdateAll() (which was designed to replace Update(), which is virtual method of Game class (XNA class)) processes keys and calls methods that update Ball state.

DrawAll:

````csharp
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
````
As we can see here, DrawAll sets all 3D drawing options and calls methods, which draw corresponding parts of world.

Now let us have a look to a class, which is inherited by most classes in the game:

````csharp
	namespace superProject
{
    class State : Game
    {
        protected int currentButton; //Stores number of selected button, if you press Enter key, corresponing action will happen.
        protected Color previousButtonColor; //Used for restoring color of the button after setting it to inactive state.
        protected Game1 parentGame; //Gives access to drawing devices, which are able to interact with the window of the game.
        protected GraphicsDeviceManager graphics;//Reference to one of drawing devices
        protected SpriteBatch spriteBatch;//The same
        protected GraphicsDevice device;//The same

        protected SpriteFont font; //Font for printing text on the screen. Fonts in XNA are not scalable.

        protected Button[] buttons; //Buttons of menu
        public Dictionary<String, String> data;//Some data, that has to be stored for some reasons. Used, for instance, in Cleared class for storing results of the level
        public MouseState previousMouseState; 

        public String name; //Used to store name of current level.


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

        protected void switchButtons(int d) //Used to swithc buttons. We pass them one-after-another in order of adding them to ButtonsList.
        {
            buttons[currentButton].backgroundColor = previousButtonColor;
            currentButton = (currentButton + d + buttons.Length) % buttons.Length;

            previousButtonColor = buttons[currentButton].backgroundColor;
            buttons[currentButton].backgroundColor = Color.Green;
        }

        float timeSpan = 0;

        protected bool enoughTimePassed(GameTime gameTime)//It is a fucntion, which allows us to maintain proper time span between player's actions.
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
````

The other interesting place in code is Ball class:
````csharp
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
````
	

##Sources

Working on this project I used:
* Riemer's XNA tutorial: http://www.riemers.net/
* Stackoverflow and other thematical forums
* Official Microsoft documentation: http://msdn.microsoft.com/en-us/library
* C# 5.0 in a Nutshell by Joseph Albahari












 

