using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

namespace Assets.Scripts.AntiGravityController.Editor
{
    public class AntiGravityAnimatorCreator : EditorWindow
    {
        private AnimationClip idleClip;
        private AnimationClip walkClip;
        private AnimationClip runClip;
        private AnimationClip jumpClip;
        private AnimationClip fallClip;
        private AnimationClip crouchIdleClip;
        private AnimationClip crouchWalkClip;
        private AnimationClip hangClip;
        private AnimationClip climbClip;
        private AnimationClip grabClip; // Push/Pull
        private AnimationClip pickClip; // Carry

        private string controllerName = "AntiGravityAnimator";
        private string savePath = "Assets";
        private RuntimeAnimatorController existingController;

        [MenuItem("Tools/Create AntiGravity Animator")]
        public static void ShowWindow()
        {
            GetWindow<AntiGravityAnimatorCreator>("Animator Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create AntiGravity Animator Controller", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            GUILayout.Label("Load Existing (Optional)", EditorStyles.boldLabel);
            existingController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Existing Controller", existingController, typeof(RuntimeAnimatorController), false);
            if (GUILayout.Button("Load from Controller") && existingController != null)
            {
                LoadFromController();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Controller Settings", EditorStyles.boldLabel);
            controllerName = EditorGUILayout.TextField("Controller Name", controllerName);

            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string absPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(absPath))
                {
                    if (absPath.StartsWith(Application.dataPath))
                    {
                        savePath = "Assets" + absPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogError("Please select a folder inside the Assets directory.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label("Grounded (Blend Tree)", EditorStyles.boldLabel);
            idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle", idleClip, typeof(AnimationClip), false);
            walkClip = (AnimationClip)EditorGUILayout.ObjectField("Walk", walkClip, typeof(AnimationClip), false);
            runClip = (AnimationClip)EditorGUILayout.ObjectField("Run", runClip, typeof(AnimationClip), false);

            EditorGUILayout.Space();
            GUILayout.Label("Crouch (Blend Tree)", EditorStyles.boldLabel);
            crouchIdleClip = (AnimationClip)EditorGUILayout.ObjectField("Crouch Idle", crouchIdleClip, typeof(AnimationClip), false);
            crouchWalkClip = (AnimationClip)EditorGUILayout.ObjectField("Crouch Walk", crouchWalkClip, typeof(AnimationClip), false);

            EditorGUILayout.Space();
            GUILayout.Label("Air", EditorStyles.boldLabel);
            jumpClip = (AnimationClip)EditorGUILayout.ObjectField("Jump", jumpClip, typeof(AnimationClip), false);
            fallClip = (AnimationClip)EditorGUILayout.ObjectField("Fall", fallClip, typeof(AnimationClip), false);

            EditorGUILayout.Space();
            GUILayout.Label("Ledge", EditorStyles.boldLabel);
            hangClip = (AnimationClip)EditorGUILayout.ObjectField("Hanging", hangClip, typeof(AnimationClip), false);
            climbClip = (AnimationClip)EditorGUILayout.ObjectField("Climb", climbClip, typeof(AnimationClip), false);

            EditorGUILayout.Space();
            GUILayout.Label("Interaction", EditorStyles.boldLabel);
            grabClip = (AnimationClip)EditorGUILayout.ObjectField("Grab (Push/Pull)", grabClip, typeof(AnimationClip), false);
            pickClip = (AnimationClip)EditorGUILayout.ObjectField("Pick (Carry)", pickClip, typeof(AnimationClip), false);

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Controller"))
            {
                CreateController();
            }
        }

        private void LoadFromController()
        {
            if (existingController == null) return;

            AnimatorController ac = existingController as AnimatorController;
            if (ac == null)
            {
                Debug.LogError("Selected controller is not an AnimatorController asset.");
                return;
            }

            controllerName = ac.name; // Auto-fill name
            
            // Assuming layer 0
            if (ac.layers.Length == 0) return;
            AnimatorStateMachine sm = ac.layers[0].stateMachine;

            foreach (var state in sm.states)
            {
                string name = state.state.name;
                Motion motion = state.state.motion;

                if (name == "Jump") jumpClip = motion as AnimationClip;
                else if (name == "Fall") fallClip = motion as AnimationClip;
                else if (name == "Hanging") hangClip = motion as AnimationClip;
                else if (name == "Climb") climbClip = motion as AnimationClip;
                else if (name == "Grab") grabClip = motion as AnimationClip;
                else if (name == "Pick") pickClip = motion as AnimationClip;
                else if (name == "Grounded" && motion is BlendTree groundedTree)
                {
                    var children = groundedTree.children;
                    if (children.Length > 0) idleClip = children[0].motion as AnimationClip;
                    if (children.Length > 1) walkClip = children[1].motion as AnimationClip;
                    if (children.Length > 2) runClip = children[2].motion as AnimationClip;
                }
                else if (name == "Crouch" && motion is BlendTree crouchTree)
                {
                    var children = crouchTree.children;
                    if (children.Length > 0) crouchIdleClip = children[0].motion as AnimationClip;
                    if (children.Length > 1) crouchWalkClip = children[1].motion as AnimationClip;
                }
            }

            Debug.Log("Loaded configuration from " + ac.name);
        }

        private void CreateController()
        {
            string path = $"{savePath}/{controllerName}.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsFalling", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsHanging", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsClimbing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsGrabbing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsPicking", AnimatorControllerParameterType.Bool);

            // State Machine
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            // 1. Grounded Blend Tree
            BlendTree groundedTree;
            AnimatorState groundedState = controller.CreateBlendTreeInController("Grounded", out groundedTree);
            groundedTree.blendParameter = "Speed";
            if (idleClip) groundedTree.AddChild(idleClip, 0);
            if (walkClip) groundedTree.AddChild(walkClip, 4);
            if (runClip) groundedTree.AddChild(runClip, 8);

            // 2. Crouch Blend Tree
            BlendTree crouchTree;
            AnimatorState crouchState = controller.CreateBlendTreeInController("Crouch", out crouchTree);
            crouchTree.blendParameter = "Speed";
            if (crouchIdleClip) crouchTree.AddChild(crouchIdleClip, 0);
            if (crouchWalkClip) crouchTree.AddChild(crouchWalkClip, 2);

            // 3. Air States
            AnimatorState jumpState = rootStateMachine.AddState("Jump");
            jumpState.motion = jumpClip;
            AnimatorState fallState = rootStateMachine.AddState("Fall");
            fallState.motion = fallClip;

            // 4. Ledge States
            AnimatorState hangState = rootStateMachine.AddState("Hanging");
            hangState.motion = hangClip;
            AnimatorState climbState = rootStateMachine.AddState("Climb");
            climbState.motion = climbClip;

            // 5. Interaction States
            AnimatorState grabState = rootStateMachine.AddState("Grab");
            grabState.motion = grabClip;
            
            AnimatorState pickState = rootStateMachine.AddState("Pick");
            pickState.motion = pickClip;

            // --- Transitions ---
            
            // Grounded <-> Crouch
            AddTransition(groundedState, crouchState, "IsCrouching", true, 0.1f);
            AddTransition(crouchState, groundedState, "IsCrouching", false, 0.1f);

            // Grounded -> Jump
            AddTransition(groundedState, jumpState, "IsJumping", true, 0.1f);
            
            // Grounded -> Fall (Walk off ledge)
            AddTransition(groundedState, fallState, "IsGrounded", false, 0.1f);

            // Jump -> Fall
            AddTransition(jumpState, fallState, "IsFalling", true, 0.1f);

            // Fall -> Grounded
            AddTransition(fallState, groundedState, "IsGrounded", true, 0.1f);
            
            // Any -> Hanging (Ledge Grab)
            AnimatorStateTransition toHang = rootStateMachine.AddAnyStateTransition(hangState);
            toHang.AddCondition(AnimatorConditionMode.If, 0, "IsHanging");
            toHang.duration = 0.1f;
            toHang.hasExitTime = false;

            // Hanging -> Climb
            AddTransition(hangState, climbState, "IsClimbing", true, 0.1f);

            // Hanging -> Fall (Drop)
            AnimatorStateTransition dropTrans = hangState.AddTransition(fallState);
            dropTrans.AddCondition(AnimatorConditionMode.IfNot, 0, "IsHanging");
            dropTrans.AddCondition(AnimatorConditionMode.IfNot, 0, "IsClimbing"); // Don't drop if we are climbing
            dropTrans.duration = 0.1f;
            dropTrans.hasExitTime = false;

            // Climb -> Grounded (End of climb)
            AddTransition(climbState, groundedState, "IsClimbing", false, 0.1f);

            // --- Interaction Transitions ---

            // Grounded <-> Grab (Push/Pull)
            AddTransition(groundedState, grabState, "IsGrabbing", true, 0.1f);
            AddTransition(grabState, groundedState, "IsGrabbing", false, 0.1f);

            // Grounded <-> Pick (Carry)
            // Note: If Pick is a single clip, walking will look weird unless it's an upper body mask.
            // But we implement the state transition here as requested.
            AddTransition(groundedState, pickState, "IsPicking", true, 0.1f);
            AddTransition(pickState, groundedState, "IsPicking", false, 0.1f);

            // Set Default
            rootStateMachine.defaultState = groundedState;

            AssetDatabase.SaveAssets();
            Debug.Log($"Animator Controller created at {path}");
        }

        private void AddTransition(AnimatorState from, AnimatorState to, string param, bool value, float duration)
        {
            AnimatorStateTransition trans = from.AddTransition(to);
            trans.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
            trans.duration = duration;
            trans.hasExitTime = false;
        }
    }
}
