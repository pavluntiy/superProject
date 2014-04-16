superProject
============
I think, it would be an appropriate discription of my super Project.

#Aims and intentions

I would like to create a game, which would copy gameplay of one old game, Ballance by Atari. In order to do that I was going to familiarize with basics of 3D graphics. I was adviced to use XNA for .NET platform. Initially, I was going to use Qt wrappers for OpenGl, but I understood, that studying another programing language in order to use a less complicated graphics framework would be a good idea.

I was not going to create anything too similar to Balance, because it has, actually, many features (like almost bugless intersection detection), which are very time-consuming.

##Rules and appearance

You are able to control a ball by applying forces in all three dimensions. You ought to collect all keys (silver spheres) in order to end game by picking a black sphere. You also may garner patchy balls (they add time to your timer), blue ones (they're lives) and coral balls, which are checkpoints.

Three different materials are available. You can be of Marble (light ball of slightly orange color), which is of medium weight, plastic(white, the most floaty, it can fly without limits) and, finally, of stone (the heaviest). Level blocks of same material change your current form.

You should avoid falling down and touching some kinds of blocks (e. g. lava). Each 'death' deprives you one live. When lives are depleted, you die. 

You could also denote timer in the upper-left corner of the screen. When time gets to zero, it just goes on! But you should take into account that scores for lives and time are summed up after finishing the level. Your shame will not be widely discussed, because I do not have table of records.

I also have different types of world blocks.
* Wood. It is the basic block. Collision is perfectly inelastic. Exists friction.
* Metal. No friction, perfectly elastic collision.
* Lava. Kills all kinds of balls except the stone one. Stone ball just passes through.
* Slime. It resorbs balls, it has the greatest measure of friction. Kills plastic balls.