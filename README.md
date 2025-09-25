# Simple-Maze-Creator

Developed by Hplus.

# Maze Pathfinding Demo (Unity)



\## How to Play in Unity Editor



\### 1. Open the project

\- Open \*\*Unity Hub\*\*, click \*\*Open project\*\*, and select the folder containing this source code.

\- In the Unity Editor, open the \*\*Scene\*\* named `SampleScene` (Assets/Scenes/SampleScene.unity).



\### 2. UI Elements

On the UI you will find:



\- \*\*Replay\*\*: generates a new map based on the width and height values.

\- \*\*Solve\*\*: finds a path from the NPC to the destination and displays it.

\- \*\*Width\*\* and \*\*Height\*\*: input fields for the map size.



\### 3. How to play

1\. \*\*Enter map size\*\* (Width × Height) in the two input fields:  

&nbsp;  - If you leave them empty, the default size will be \*\*40 × 40\*\*.

2\. Press \*\*Replay\*\*:  

&nbsp;  - A new maze map will be generated with the NPC placed at the top-left corner (1,1) and a Destination on the opposite side.

3\. Press \*\*Solve\*\*:  

&nbsp;  - The game will calculate the shortest path from the NPC to the destination.  

&nbsp;  - The path will be displayed with special tiles (solved path prefabs).  

&nbsp;  - The NPC will automatically move along this path until reaching the goal.



\### 4. Notes

\- The NPC only starts moving after you press \*\*Solve\*\*.  

\- If no valid path is found, the Console will log: `"No path found to Destination"`.  

\- You can press \*\*Replay\*\* multiple times to try different random maps.



---



\## Additional Info

\- The maze is generated randomly according to the chosen size.  

\- Pathfinding uses \*\*BFS (Breadth-First Search)\*\* to guarantee the shortest path when all step costs are equal.



