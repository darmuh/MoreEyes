# MoreEyes

Customize your own player model however you like!

- Left Iris
- Right Iris
- Left Pupil
- Right Pupil
- Top and Bottom Eyelids no more since game devs implement them (in their own ways) YIPPEE
![](https://media.discordapp.net/attachments/1348729665879015565/1348829553048555551/image.png?ex=67dac629&is=67d974a9&hm=9bb4a8a71ad8e40dd3a50435ff33b298a1d1771a9dcdfca33405459ffac69139&=&format=webp&quality=lossless)
![](https://media.discordapp.net/attachments/1348729665879015565/1348802440740475091/image.png?ex=67db55a9&is=67da0429&hm=f6209b16742c8953a93050c75361dfba7c1030105deea0ce3d85975f3edd69e4&=&format=webp&quality=lossless)
![](https://media.discordapp.net/attachments/1348729665879015565/1348798930283921460/image.png?ex=67db5264&is=67da00e4&hm=aa44a8775a06b201eb2b09d78a8dd8e58fee1472920cf500e704bc3f2213e4a5&=&format=webp&quality=lossless)
![](https://media.discordapp.net/attachments/1348729665879015565/1348780867019083776/image.png?ex=67db4191&is=67d9f011&hm=970a1594e9900b40a43956b18829c124cdf7c1f85cf511c907ca408a243f2704&=&format=webp&quality=lossless)
![](https://media.discordapp.net/attachments/1348729665879015565/1348780866683277415/image.png?ex=67db4191&is=67d9f011&hm=fbb1c2e2fdc180bec36b08393ecd0876b9b5f2d7d09b292d1bd9dbeb2aa9139b&=&format=webp&quality=lossless)


## How To Add Your Own

1) You will need to follow [Installation Guide of Unity Repo Patcher](https://github.com/Kesomannen/unity-repo-project-patcher)
2) Put the `PlayerAvatar` in the scene and click on one of the pupils
3) Export `mesh_pupil_r` or `mesh_pupil_l` in FBX format
     a) Import `FBX Exporter` if you don't have it already [Window -> Package Manager -> On the top left of the popup "Unity Registry" and on the top right in the search bar write "FBX" it should come up
     b) Right click on one of the gameobject previously mentioned -> Export To FBX
     c) Give it an Export Path of your choosing, Export Format: `Binary`, Include: `Model(s) Only` - everything else can remain the same
4) Open Blender and Import the fbx file
5) Duplicate it, we want to keep the original for reference (You can make a bigger pupil, iris whatever its up to you)
6) Once you are done, you can remove the original model and export the model as an FBX file
     a) Create a folder in unity where you would have your pupils, irises and then Right Click -> Show In Explorer -> Copy the path
     b) Go back to blender -> File -> Export -> FBX -> Paste path, and then export ONLY the mesh (Object types: Meshes, nothing else)
7) In unity what you want to do is create a new Gameobject, have it named in this format: cat_pupil_right , diamond_iris_left
     a) Add components : Mesh Filter and Mesh Renderer.
     b) Add your mesh (inside the fbx) to the filter
     c) Apply `Player Avatar - Pupil` material to Mesh Renderer
     d) Save the GameObject as a Prefab by dragging it into your folder
8) Now you will need an assetbundle builder you can either use (add it in a similar way how you were taught in 1) guide
     a) [AssetBundle Browser](https://github.com/Unity-Technologies/AssetBundles-Browser) <- old unity assetbundle builder, has been deprecated but still works just fine, you wont have issues with this for these kind of projects
     b) [CR AssetBundle Builder](https://github.com/XuuXiaolan/CR-AssetBundle-Builder) <- newer assetbundle builder made by [Xu](https://github.com/XuuXiaolan) and has more features tailored to projects with several assetbundles
9) Put all your pupil and iris prefabs under the same assetbundle and then build your bundle.
10) Once you are done with that, follow [Thunderstore package format documentation](https://thunderstore.io/c/lethal-company/create/docs/), you can also use their [Markdown Preview](https://thunderstore.io/tools/markdown-preview/) and [Manifest Validator](https://thunderstore.io/tools/manifest-v1-validator/). One is a handy tool for constructing README and the other is a handy tool to check if you did your manifest correctly (this is self-evident BUT you need this mod as a dependency)

For more instructions, tips look into [Setup Tutorial](https://github.com/s1ckboii/MoreEyes/tree/master/SetupTutorial)


## Credits to
- [Darmuh](https://github.com/darmuh) for collaborating and teaching me at the same time 🥇
