# Maskable-Renderer-Features-URP
Base classes to inherit from for easily creating custom renderer features providing Layer Masking and further abstractions.

# How to use:
1. Create, as usual, an ScriptableRenderererFeature script (It is helpful to use the default template via the editor create dropdown)
2. In the PassFeature class inherit from [MaskableRendererFeature](https://github.com/ChichoRD/Maskable-Renderer-Features-URP/blob/main/Core/MaskableRendererPassFeature.cs "featureLink") and in its RenderPass class make it inherit from [MaskableRenderPass](https://github.com/ChichoRD/Maskable-Renderer-Features-URP/blob/main/Core/MaskableRenderPass.cs "passLink")
    1. Override the necessary methods as normal and the abstract method `ExecuteWithMask`
    2. Notice that by inheriting from this class you get access to `CameraColorBuffer`, from which you will commonly blit from and then to; and the parameters of the previous function.
3. **From the `ExecuteWithMask`** function, before blitting any necessary textures, you may **use the command `cmd.SetGlobalTexture` and pass to your specific screen-space shader the parameter `filteredBuffer`** which has been filtered to contain a pure black and white image of objects that meet the criteria for masking. You may add a new shader texture variable, set it with the value of said parameter and then work with the mask inside the fragment pass to lerp between non-affected screen colors and masked regions of your image.

All extra considerations (i.e. disposal, initialisation, boilerplate) are taken care of behind the scenes by the super-class so there is no need to dispose protected member textures or anything alike.
