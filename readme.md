
###Description of project

##Aims and intentions

I was going to make something like Balance, an old game by Atatri. For this I decided to familiarize with basics of 3D Graphics. I used XNA for .NET platform instead of adhering to my initial intention to use Qt wrappers for OpenGl, because they seemed  inefficient and I thought it is a great idea to study a new programing language (C#).

I didn't manage to clone Balance completely because it has many quite time-consuming features, but I believe that the plot is correctly transferred and impressions are not so dreadful as I could have expected.

#About XNA
XNA support quite nice 3D graphics. The main its advantage is that it allows drawing textured triangles in a very simple way, loading models (actually, the only model I have used is model of a sphere) and it also provides checking for collisions of two bounding spheres. Unfortunately, XNA was not created to draw beautiful GUIs, so menus of my game were painted programmatically.

##Used algorithms
As it was announce in the begining, my main intention wasn't to familiarize to any powerful algorithms. I was going to dive into 3D graphics and creating nice-looking (relatively) games.

I used algorithms of intersection of ball and a triangle. I found in the Internet one that served an example for me, but I had to change it drastically. I have also created some algos (functions) for calculating interactions between static blocks and the ball. You could have noticed some displays of its imperfectness: ball behaves in a very strange way on edges of blocks. In order to create such algorithms I used physics (mechanics) like law of impuls conservation and second Newton's law. I also used conception of state machine to organize my menus and selections.

Ball in my game obeys laws of impuls conservation. If it hits appropriate surfaces (metal), it reflects properly. Wood and other types of materials extinguish complitly normal component of impuls vector.

Second Newton's law is also performed properly. When you apply force (or gravitation does) ball gets acceleration according to widely known formula.

I have also added a rotation effect. When the ball is rolling, its surface also rotates in an appropriate way. 

I created several classes for organization of my game. Most of them are designed to provide menus and selection screens. I also used structure, that stores position, normal and texture.

I also have to confess that conception of encapsulation was deeply offended during creating this project.

##User's guide

#Rules
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

#Controls
* Arrows are used to apply force in horizontal plane in the respecting direction.
* PageUp and PageDown keys are used to control vertical behavior
* Home key is used to teleport instantly to the location of the last checkpoint (without any fines for usage).
* End key is used for instant stop. (Velocity is assigned to zero). This key wasn't expected to survive up to the release version.
* Control keys are used to rotate camera.
* Escape has different meanings which depend on current state of game. During gaming it pauses the process.

#Menu system
When the game is launched you can see a pretty well-designed menu. It is the main menu of the game. You are able to choose desired item by using arrows, when you select an item it is highlited, now you are able to press enter key to confirm your selection. By default, the exit item is selected. In the main menu escape button will have the same effect as selecting exit.
Other menus have similar structure. 

When you are playing you can pause your game by pressing escape. Second hit to escape button (as well as selecting the correspoding item) will return you back to the game.
When you won you can see screen with your results.
When you loose, you are informed about this fact.

##Programmer's guide
You are able to extend the game.
You can add new levels and modify menus in a very simple way.
#Adding new level
You should create file names "LevelX levelData.txt". where LevelX is name of your level.
This file contains information about world.
It has the following structure:
ball:
material:  <initial material of ball> position: <initial position> lives: <initial quantity of lives> score: <initial score> minHeight: <minimal save height>
level:
gravity:  <acceleration of gravity> force_coef: <absolute values of forces apllied by player>
<type of block> <coordinates of block> <material>

You can choose from following block types: 
* "cube". Parrallelipiped, actually.
* "wedge"
* "pyramid"
* "plain"
<coordinates of block> is enumeration of four different point in space. Each point is given by three coordinates in space (X, Y, Z) without any delimeters between points or their coordinates! You also shouldn't use additional newlines and spaces.

The second file that has to be created is bonus data file. It has name "LevelX bonusesData.txt".  It has a quite similar structure.
	...
<bonus type> <coordinates>
...
Bonus types are {Save, Live, Score, End, Key}. You can add them to game without any limitations.
Coordinates are (X, Y, Z) enumerated without any delimiters or additional spaces.
You can define coordinates in floating-point format.

#Defining a button.
You should also add a button into Selection.cs file as you add a new level.
In order to do this go to function ButtonsInit() and add a button like it is stated there. You should understand, that buttons are selected by arrows in order of their enumeration in that function.
Then you go to function UpdateAll() and add a new brach to the swicth operator.
These metodics are appropriate for all classes inherited from State class.

##Sources

Working on this project I used:
* Riemer's XNA tutorial: http://www.riemers.net/
* Stackoverflow and 
* Official Microsoft documentation: http://msdn.microsoft.com/en-us/library
* C# 5.0 in a Nutshell by Joseph Albahari
* 












 

