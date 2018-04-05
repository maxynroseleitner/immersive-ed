1. If a shader error reports that unity can't find a ".cginc" file it's probably because unity didn't install it by default, go to https://unity3d.com/get-unity/download/archive, 
click on your corresponding version, select Built in shaders, download and extract the archive somewhere, navigate inside it to "\CGIncludes" and copy the file reported missing inside "VolumetricClouds/Scripts".


2. If it's another type of error, verify that the only files that are inside the "Scripts" folder are "VolumetricCloud.Shader" and "VolumetricCloudsCG.cginc", 
any other can be safely deleted since they either come from point 1 of this document or from an old version of this asset.

3. If none of the above worked, drop by the forum @ http://forum.unity3d.com/threads/released-volumetric-clouds.274943/ or send me a PM @ http://forum.unity3d.com/members/eideren.386164/