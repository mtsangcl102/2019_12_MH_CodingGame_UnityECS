# 2019_12_MH_CodingGame_UnityECS

Hi guys, welcome back to coding game.
We are going to use Unity build-in ECS

This time, we are going to finish below things:
1. Create a 100 * 100 * 3 blocks terrian, each block jsut a cube 
- Some cube randomly higher as a "Tree" or a "Rock"
- Each cube have different color if height is different

2. Keep creating some capsule as a character and walk around in the terrian
- Can be any color, no limit
- Dont pass any "Rock", "Tree" or other "Character"

3. Distory "Character" in below cases
- Character born position have a "Rock" or "Tree"
- Character hit other character during walking

4. Below rules must follow
- Just 3 game objects in hierarchy : Main Camera, Light, GameManager(for init everything)
- After init terrian, FPS above 30( as high as you can )
- Just GameManager is MonoBehaviour script
- All terrian and character are entites

-----------------------------------------------------------------------------------------
Remark :
if watching Unity official tutorial of ECS, Unity version must be 2018.1
(Not Recommend)!!!
- https://learn.unity.com/tutorial/entity-component-system

Recommend Unity version after 2019.1 and link may be helpful
- https://docs.unity3d.com/Packages/com.unity.entities@0.0/manual/index.html
- https://www.youtube.com/channel/UCFK6NCbuCIVzA6Yj1G_ZqCg

