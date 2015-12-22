# Source Resources Loader
Importer resources from Source Engine in Unity3D (4.x - 5.x).

The current process of implementation:

- Load Resources:
    - BSP (Ver. 19 or more) - Meshes, Entities, Lightmaps
    - MDL / VTX / VVD - Skin Meshes, Bones
    - VTF / VMT - Textures, Materials

- Bugs:
    - Displacements don't has ligtmaps.
    - Wrong rotation at the dynamic props.
