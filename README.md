# Flight in Virtual Reality

## Introduction
The goal of this project was to explore a new way of navigating in virtual space by flying. We ended up with a seemingly okay way of navigating, one that at first glance is not completely untuitive and takes a few minutes of practice to get used to for most people. The project was demoed at a local fair with 11 other projects exploring different concepts for reality.
We release it now for anyone to try, and for those interested to continue develop the experience into something greater.

## Navigation
There are 3 in-air states. Flying, Gliding and Falling. More on these below.

**Important:** The trigger must be pressed at all times (either left or right hand trigger) to enable flight.

**Increasing Altitude:**
Flap your arms like a bird. Larger flaps will increase altitude more than smaller will.
It simply registers the Y-axis movement of the controllers.

**Decrease Altitude:**
By retracting your wings (moving the controllers next to your body and standing still) you will enter a falling state in which your altitude is decreased. This is visualized by a white downward pointing arrow.

**Adjusting Horizontal Speed:**
Perhaps the least intuitive part of the apparatus, entirely based on controller rotation (NOT MOVEMENT). If the controllers are rotated forward, a higher forward speed (and downward speed) will be achieved, and vice versa for backwards rotation but in the other direction.
A common mistake made here is that people intuitively want to move their arms in a rowing motion during which they don't notice how their hands are rotated which might reduce forward speed if they end up being rotated backwards.

**Adjusting Direction:**
A vector is drawn from between the controllers and forward, this is your direction. So in order to change direction, simply rotate your whole body and the direction will follow that rotation.

Tips on Navigation: 
The recommended way of navigating is by extending your arms straight, paralell with your body (imagine crucifixion). Stay like this at all times and flap up down to increase altitude, rotate your controllers for horizontal speed, and turn around to adjust direction.

## Contribute

You are most welcome to fork the project or simply pull it and start your own repo. If you do, we sure wouldn't mind if you give us some credit for the idea!

## Development 

We've used unity 5.6.0f3 and thus it isn't certain it will work using earlier version. 

The main script is `FlightHandlerPhys` located in the Scripts folder. With the scene that comes with the project extra features such as a virtual arrow has been added for a better experience, as well as wing models and a menu for toggling between two pre-defined profiles (Slow and Fast) that simply adjust the parameters of `FlightHandlerPhys`.

### FlightHandler Inspector Parameters

![Image of Inspector](https://github.com/JonasDe/flight/images/FlightHandlerInspector.png)

Most of these attributes should be self-explanatory, but for those that might not be, here are some instructions:

**Air drag:**
This Adjusts the air drag while in Flight or Glide mode. While in Falling, it is set to 0.

**Controller History Sample:** 
Size of the queue storing the controller position/rotation/thrust samples which is averaged over to determine vector magnitudes for thresholds, and vector directions for velocities.

**Gravity reduction:**
While in Flight / Glide, this is how much gravity is counteracted by constantly applying a negative gravity of this magnitude.

**Hand movement timer:**
If hands are still for this amount of time, Flight state will be exited (and falling / glide will be entered, based on conditions).

**Braking speed:**
Determines how fast you decellerate.

**Max rotation:**
At what angle is maximum speed achieved when rotating the controller forward/backward.

**Rotation mode:**
How is the speed forward/backward proportional to the rotation.

**Dive intensity:**
How much downward speed is gained when rotating the controllers.

**Increase fall speed:**
Must be turned on if dive intensity should be used.

**Tilt intensity:**
This is a highly experimental feature and we thought it was prone to introduce motion sickness. If you keep the controllers at different altitude you will also slightly move to the left or right (depending on which controller is higher than the other). We added this since we noticed a lot of people wanted to tilt their whole bodies to simulate a bird-like glide, and the value of this parameter determines how much of that tilt is added to the direction.
Currently it's a horizontal velocity addition, but it might be better to try a rotation change instead. 


**Max turn rate:**
A limit on how many degrees per frame the player can turn. This is good to prevent sudden changes in rotation.

**Flight direction:**
Flight Direction Averaged is really the only mode supported at this moment. But if you fancy trying out different modes, feel free to implement them.

**Engage mode:**
How to toggle flight

**Glide controller min distance:**
How far apart must the controllers be kept to stay out of falling mode.

## Credits & Contact

[Erik](https://github.com/erikbjare)
[Jonas](http://www.github.com/JonasDe)
[Valthor](https://github.com/vlthr)
[Simon](https://github.com/essenji)

## Disclaimer

It is not a perfect piece of code at this point. It is simply a proof of concept developed during a relatively short period of time. If we had the choice to redo it, we would implement things differently. There are still things that could be improved (the sound is one prime example) and some bugs are still in effect.

There are also a lot of assets that aren't used in the project, but comes with the repo. They are remnants from trying out different features and environments, so delete any as you see fit if you fork. Some if it has been cleaned up but since the project has reached its end and due to a lack of interest in fiddling with dependencies it was left in that state.


