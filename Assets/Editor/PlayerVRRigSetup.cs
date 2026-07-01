using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public static class PlayerVRRigSetup
{
    const string PrefabPath = "Assets/Prefabs/Players/Player.prefab";
    const string InputActionsPath = "Assets/InputActions/VRControls.inputactions";

    public static void Run()
    {
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath).OfType<InputActionReference>().ToList();
        Debug.Log($"PlayerVRRigSetup: found {subAssets.Count} InputActionReference sub-assets in {InputActionsPath}");
        foreach (var a in subAssets)
            Debug.Log($"PlayerVRRigSetup: sub-asset action = {a.action.name}");

        InputActionReference FindRef(string name) => subAssets.FirstOrDefault(a => a.action.name == name);

        var moveRef = FindRef("Move");
        var headPosRef = FindRef("HeadPosition");
        var headRotRef = FindRef("HeadRotation");

        if (moveRef == null || headPosRef == null || headRotRef == null)
        {
            Debug.LogError("PlayerVRRigSetup: FAILED to resolve one or more InputActionReferences. Aborting.");
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var cameraTransform = root.transform.Find("Main Camera");
            if (cameraTransform == null)
            {
                Debug.LogError("PlayerVRRigSetup: 'Main Camera' child not found on Player prefab. Aborting.");
                return;
            }

            var poseDriver = cameraTransform.GetComponent<TrackedPoseDriver>();
            if (poseDriver == null)
                poseDriver = cameraTransform.gameObject.AddComponent<TrackedPoseDriver>();

            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.positionInput = new InputActionProperty(headPosRef);
            poseDriver.rotationInput = new InputActionProperty(headRotRef);
            EditorUtility.SetDirty(poseDriver);
            Debug.Log("PlayerVRRigSetup: TrackedPoseDriver added/updated on Main Camera");

            var playerMovement = root.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("PlayerVRRigSetup: PlayerMovement component not found on Player root. Aborting.");
                return;
            }

            var so = new SerializedObject(playerMovement);
            so.FindProperty("vrMoveAction").objectReferenceValue = moveRef;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("PlayerVRRigSetup: wired vrMoveAction on PlayerMovement");

            bool saved = false;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out saved);
            Debug.Log($"PlayerVRRigSetup: SaveAsPrefabAsset success = {saved}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
