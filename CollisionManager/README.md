**- MMF Collider Manager -**
Designed by: **MiniatVRe Studio**
Written by: *ChatGPT*

There are several advantages to using the MMFCollisionManager and MMFCollisionHelper scripts in a Unity project:

- **Centralized management**: The MMFCollisionManager script serves as a central location for managing all of the collision-based feedback effects in the project. This can make it easier to organize and manage these effects, especially if there are many of them.

- **Reusability**: The MMFCollisionHelper script can be attached to any game object in the project, allowing you to reuse the same collision-based feedback effects on multiple objects.

- **Customization**: The MMFCollisionManager script allows you to specify different feedback effects and layers for each game object, giving you the flexibility to customize the behavior of the collision-based feedback effects.

- **Modularity**: The MMFCollisionManager and MMFCollisionHelper scripts are modular, meaning that they can be easily added or removed from a project without affecting the rest of the code. This can make it easier to add or remove collision-based feedback effects as needed.

Overall, using the MMFCollisionManager and MMFCollisionHelper scripts can help you to efficiently and effectively manage and trigger collision-based feedback effects in your Unity project.

--------------

Here is a step-by-step guide on how to use the MMFCollisionManager and MMFCollisionHelper scripts in a Unity project:

1. Add the MMFCollisionManager script to an empty game object in your Unity project. This will be the object that manages the collision-based feedback effects.

3. In the MMFCollisionManager script's Collision Lists, create a new element. Set the Name field to a descriptive string.

5. In the Feedback Objects list of the new element, create a new MMFObjects element. Set the Name field to a descriptive string.

7. Drag a game object from your scene into the Game Object field of the MMFObjects element. This will be the object that the feedback effect is triggered on when it collides with another object.

9. Drag an MMF Player component from the project hierarchy into the MMF Player field of the MMFObjects element. This component will handle playing the feedback effect.

11. Set the LayerMask field of the MMFObjects element to the layer or layers that the colliding object must be in for the feedback effect to be triggered.

13. Repeat steps 3-6 for each game object that you want to trigger a feedback effect when it collides with another object.

15. Add the MMFCollisionHelper script to the game object that you want to trigger the collision-based feedback effects when it collides with another object.

17. Drag the game object with the MMFCollisionManager script attached to it into the MMFCollisionManager field of the MMFCollisionHelper script.

19. Set the MMF Collision List Name field of the MMFCollisionHelper script to the name of the List Name element that you want to use for this game object.

Repeat steps 8-10 for each game object that you want to trigger collision-based feedback effects when it collides with another object.

Now, when the game objects with the MMFCollisionHelper script attached to them collide with other objects that are in the specified layer(s), the MMFCollisionManager script will trigger the appropriate feedback effect(s).

------------------------------------------------

**MMFCollisionManager.cs**

This is a script for managing collisions in Unity. The MMFCollisionManager class has a list of MMFCollisionLists objects, which in turn have a list of MMFObjects objects. The MMFObjects struct includes a GameObject field, which represents the game object that the collision is being checked for, and a MMF Player field, which is a component that plays a series of feedbacks when activated. The CheckAndPlay method iterates through each CollisionLists object in the list and finds the MMFObjects object that has a GameObject field that matches the collidingObject parameter. If the collidingObject is in the specified layer specified in the LayerMask field of the MMFObjects object, the feedbacks are initialized and played.

**The MMFCollisionManager script is a component that can be added to a game object in Unity. It is used to manage collisions between game objects and play feedbacks when those collisions occur.**

The MMFCollisionManager script has the following fields:

- CollisionLists: This is a list of MMFCollisionLists objects. Each CollisionLists object represents a set of game objects and feedbacks that will be checked and played when a collision occurs.
- The MMFCollisionLists class has the following fields:

- Object Name: This is the name of the CollisionLists object. It can be used to identify the object and to specify which list to use in the MMFCollisionHelper component.
- Feedback Objects: This is a list of MMFObjects objects. Each MMFObjects object represents a game object and feedbacks that will be checked and played when a collision occurs.

The MMFObjects struct has the following fields:

- **Object Name**: This is the name of the MMFObjects object. It can be used to identify the object.
- **Game Object**: This is the game object that will be checked for collisions.
- **MMF Player**: This is the MMFPlayer component that contains the feedbacks that will be played when a collision occurs.
- **LayerMask**: This is the layer that the game object must be in for the feedbacks to be played when a collision occurs.
  To add a new Collision Lists object to the MMFCollisionManager component, click the "+" button next to the Collision Lists field. This will create a new list. You can then add game objects and feedbacks to this object by adding new MMFObjects objects to the Feedback Objects list and specifying the game object, feedbacks, and layer mask for each object.

To trigger the feedbacks, you can call the CheckAndPlay method of the MMFCollisionManager component with a game object as a parameter. This will check the game object against the MMFCollisionLists objects and MMFObjects objects in the MMFCollisionManager component to see if it is in the specified layer and if so, it will play the feedbacks.

-----------------------------------------------------------------------------------------------------------------------------------------------------------------------

**MMFCollisionHelper.cs**

This is a script for detecting collisions in Unity using the OnTriggerEnter method. When a collision is detected, it finds the MMFCollisionLists object in the MMFCollisionManager with a matching name specified in the MMFCollisionListName field, and then it finds the MMFObjects object within that MMFCollisionLists object that has a GameObject field matching the colliding object. If the MMFCollisionLists object exists and the colliding object is in the specified layer, it calls the CheckAndPlay method of the MMFCollisionManager class with the colliding object as a parameter. The CheckAndPlay method will then check the layer of the colliding object and play the feedbacks if it is in the specified layer.

The MMFCollisionHelper script is a component that can be added to a game object in Unity. It is used to detect collisions between game objects and trigger the MMFCollisionManager component to check and play feedbacks when those collisions occur.

**The MMFCollisionHelper script has the following fields:**

- **MMFCollisionManager**: This is a reference to the MMFCollisionManager component that will be used to check and play feedbacks when a collision occurs.
- **MMFCollisionListName**: This is the name of the MMFCollisionLists object in the MMFCollisionManager component that you want to use to check and play feedbacks when a collision occurs.

To set up the MMFCollisionHelper component, you will need to drag the game object with the MMFCollisionManager component onto the MMFCollisionManager field. This will link the MMFCollisionHelper component to the MMFCollisionManager component. Then, specify the name of the MMFCollisionLists object that you want to use in the MMFCollisionListName field.

When a collision occurs, the MMFCollisionHelper component will call the CheckAndPlay method of the MMFCollisionManager component with the colliding object as a parameter. The CheckAndPlay method will then check the colliding object against the MMFCollisionLists objects and MMFObjects objects in the MMFCollisionManager component to see if it is in the specified layer and if so, it will play the feedbacks.

----------------------------------------------------------

**Here is a list of basic use cases for the MMFCollisionManager and MMFCollisionHelper scripts in C#:**

Playing sound effects when two game objects collide: You can use the MMFCollisionManager and MMFCollisionHelper scripts to play sound effects when two game objects collide. For example, you might set up a "Sword" MMFCollisionLists object with a "Sword" game object and a "Metal Clang" sound effect. When the "Sword" game object collides with another game object, the "Metal Clang" sound effect will be played.

Playing visual effects when two game objects collide: You can use the MMFCollisionManager and MMFCollisionHelper scripts to play visual effects when two game objects collide. For example, you might set up a "Metal Objects" MMFCollisionLists object with a "Metal Sword" game object and a "Sparks" visual effect. When the "Metal Sword" game object collides with another game object, the "Sparks" visual effect will be played.

Playing haptic feedbacks when two game objects collide: You can use the MMFCollisionManager and MMFCollisionHelper scripts to play haptic feedbacks when two game objects collide. For example, you might set up a "Sword" MMFCollisionLists object with a "Sword" game object and a "Vibration" haptic feedback. When the "Sword" game object collides with another game object, the "Vibration" haptic feedback will be played.

Playing gameplay mechanics when two game objects collide: You can use the MMFCollisionManager and MMFCollisionHelper scripts to play gameplay mechanics when two game objects collide. For example, you might set up a "Player" MMFCollisionLists object with a "Player" game object and a "Health Decrease" gameplay mechanic. When the "Player" game object collides with an enemy game object, the player's health will be decreased.

These are just a few examples of the types of feedbacks and gameplay mechanics that can be triggered using the MMFCollisionManager and MMFCollisionHelper scripts. There are many other possible use cases and configurations that you can set up depending on your game's needs.

The optimal way to organize your game objects with this script would depend on the specific needs and goals of your project. Here are a few general tips that might be helpful:

Group similar objects together: If you have multiple objects that perform similar functions or are used in similar contexts, you might want to consider grouping them together in the same MMFCollisionList. This can help you manage and organize your objects more efficiently.

Use meaningful names: Giving your MMFCollisionList and MMFObjects structures descriptive names can make it easier to understand what each group of objects is being used for and how they interact with each other.

Consider the size of your lists: If you have a large number of objects that need to be organized, it might be more efficient to break them up into smaller lists. This can make it easier to manage and work with your objects, especially if you need to make changes or updates to specific groups of objects.

Consider the structure of your game: You might want to consider organizing your objects based on the structure or hierarchy of your game. For example, you might want to create separate lists for objects that are used in specific levels, or objects that are used in specific game modes.

Ultimately, the best way to organize your objects will depend on the specific needs and goals of your project. It's always a good idea to take some time to think about how you want to structure your objects and what will work best for your project.

Enjoy!
