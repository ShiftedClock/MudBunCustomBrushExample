MudBun must be installed from the Package Manager for this repo to work. Currently this repo has only been tested with URP.
 
Start by copying the CustomBrushes directory from this repository into your project. Ignore all the other files in this repo besides that folder, they're just images and stuff for the readme.

You may get some errors saying "Unsafe code may only appear if compiling with /unsafe. Enable "Allow 'unsafe' code" in Player Settings to fix this error."

If you do get those errors, and you're not okay with allowing unsafe code, instead you can remove the unsafe code in MudLetter and MudEllipsoid, since it's only used for CPU raycasts and such. The lines that need to be removed are labeled in those files. Otherwise do what the error says.

MudEllipsoid is just a copy of MudSphere that uses a Vector3 field called `dimensions` to set it's size instead of the object's scale. It's meant to be a simple example for how to make a custom brush on the C# side since it doesn't require any custom shader code.

In the right-click menu under MudBun/Custom you'll find Ellipsoid and Letter.

![custom brushes](custom_brush.bmp)

The Ellipsoid will render fine because it's using the same code as the existing Sphere brush, but the Letter won't render at all.

To get the letter to render, you need to edit the file "MudBun/Customization/CustomBrush.cginc".

In that file, under line 14 where it says `#include "../Shader/SDF/Primitives.cginc"`, paste this:

`#include "Assets/CustomBrushes/SdfLetter.cginc"`

So the first section of CustomBrush.cginc should look like this:

![include custom brush](include.PNG)

Then go down to line 96 in the same file, and copy/paste the following code:

```
case kLetter:
{
  res = sdf_letter(pRel, h, brush.data0.x, brush.data0.yz, brush.data0.w, brush.data1.x) - brush.radius;
  break;
}
```

It should look like this, paying close attention to the closing curly braces:

![add switch statement for custom brush](case.PNG)

Right now it is necessary to add a new case to this switch statement whenever a custom brush is added. The dev of MudBun is looking at ways of automating this process, since it is error prone.

Whenever you update MudBun, you will have add these lines back to CustomBrush.cginc.

After adding these lines, and the shaders are done re-compiling (which can take a few minutes), the letter *should* show up. If you get an error saying `Compute shader (MarchingCubes): Property (triTable) at kernel index (1) is not set`, try switching the MudRenderer to Surface Nets or some other Meshing Mode. I don't know what causes this error to appear, it doesn't happen all the time.

If the letter still doesn't show up, I'm sorry but there isn't much else I can do, it's a finnicky system. But hopefully you can use these examples to make your own custom brushes.
