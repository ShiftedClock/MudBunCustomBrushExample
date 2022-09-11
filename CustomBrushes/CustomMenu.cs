using UnityEditor;
using UnityEngine;
using MudBun;

public class CustomMenu : CreationMenu
{
  // You can copy/paste an entry from this list and modify the values for your own brushes
  [MenuItem("GameObject/Mud Bun/Custom/Ellipsoid", priority = 4)]
  public static GameObject CreateEllipsoid()
  {
    // The string is the default name of the object after being created
    var go = CreateGameObject("Mud Ellipsoid");
    // Add your custom Mud solid component to the game object
    go.AddComponent<MudEllipsoid>();

    return OnBrushCreated(go);
  }
  
  [MenuItem("GameObject/Mud Bun/Custom/Letter", priority = 4)]
  public static GameObject CreateLetter()
  {
    var go = CreateGameObject("Mud Letter");
    go.AddComponent<MudLetter>();

    return OnBrushCreated(go);
  }
}

