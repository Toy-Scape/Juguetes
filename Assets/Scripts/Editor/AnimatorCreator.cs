using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Assets.Scripts.PlayerController.Editor
{
    public class AnimatorCreator : EditorWindow
    {
        private AnimationClip idleClip;
        private AnimationClip walkClip;
        private AnimationClip runClip;
        private AnimationClip jumpClip;
        private AnimationClip fallClip;
        private AnimationClip crouchIdleClip;
        private AnimationClip crouchWalkClip;
        private AnimationClip hangClip;
        private AnimationClip standClip;

        private AnimationClip grabIdleClip;
        private AnimationClip grabPullClip;
        private AnimationClip grabPushClip;

        private AnimationClip pickIdleClip;
        private AnimationClip pickWalkClip;
        private AnimationClip pickRunClip;

        private AnimationClip ledgeClimbClip;

        private AnimationClip climbIdleClip;
        private AnimationClip climbUpClip;
        private AnimationClip climbDownClip;
        private AnimationClip climbLeftClip;
        private AnimationClip climbRightClip;

        private string controllerName = "PlayerAnimator";
        private string savePath = "Assets";
        private RuntimeAnimatorController existingController;

        [MenuItem("Tools/Create Animator")]
        public static void ShowWindow ()
        {
            GetWindow<AnimatorCreator>("Animator Creator");
        }

        private void OnGUI ()
        {
            GUILayout.Label("Create Animator Controller", EditorStyles.boldLabel);

            existingController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Existing Controller", existingController, typeof(RuntimeAnimatorController), false);
            if (GUILayout.Button("Load from Controller") && existingController != null)
                LoadFromController();

            controllerName = EditorGUILayout.TextField("Controller Name", controllerName);

            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string absPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(absPath))
                {
                    if (absPath.StartsWith(Application.dataPath))
                        savePath = "Assets" + absPath.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Grounded (Blend Tree)", EditorStyles.boldLabel);
            idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle", idleClip, typeof(AnimationClip), false);
            walkClip = (AnimationClip)EditorGUILayout.ObjectField("Walk", walkClip, typeof(AnimationClip), false);
            runClip = (AnimationClip)EditorGUILayout.ObjectField("Run", runClip, typeof(AnimationClip), false);

            GUILayout.Label("Crouch (Blend Tree)", EditorStyles.boldLabel);
            crouchIdleClip = (AnimationClip)EditorGUILayout.ObjectField("Crouch Idle", crouchIdleClip, typeof(AnimationClip), false);
            crouchWalkClip = (AnimationClip)EditorGUILayout.ObjectField("Crouch Walk", crouchWalkClip, typeof(AnimationClip), false);

            GUILayout.Label("Air", EditorStyles.boldLabel);
            jumpClip = (AnimationClip)EditorGUILayout.ObjectField("Jump", jumpClip, typeof(AnimationClip), false);
            fallClip = (AnimationClip)EditorGUILayout.ObjectField("Fall", fallClip, typeof(AnimationClip), false);

            GUILayout.Label("Ledge", EditorStyles.boldLabel);
            hangClip = (AnimationClip)EditorGUILayout.ObjectField("Hanging", hangClip, typeof(AnimationClip), false);
            ledgeClimbClip = (AnimationClip)EditorGUILayout.ObjectField("Ledge Climb", ledgeClimbClip, typeof(AnimationClip), false);
            standClip = (AnimationClip)EditorGUILayout.ObjectField("Standing", standClip, typeof(AnimationClip), false);

            GUILayout.Label("Wall Climb (Blend Tree 2D)", EditorStyles.boldLabel);
            climbIdleClip = (AnimationClip)EditorGUILayout.ObjectField("Climb Idle", climbIdleClip, typeof(AnimationClip), false);
            climbUpClip = (AnimationClip)EditorGUILayout.ObjectField("Climb Up", climbUpClip, typeof(AnimationClip), false);
            climbDownClip = (AnimationClip)EditorGUILayout.ObjectField("Climb Down", climbDownClip, typeof(AnimationClip), false);
            climbLeftClip = (AnimationClip)EditorGUILayout.ObjectField("Climb Left", climbLeftClip, typeof(AnimationClip), false);
            climbRightClip = (AnimationClip)EditorGUILayout.ObjectField("Climb Right", climbRightClip, typeof(AnimationClip), false);

            GUILayout.Label("Grab (Blend Tree)", EditorStyles.boldLabel);
            grabIdleClip = (AnimationClip)EditorGUILayout.ObjectField("Grab Idle", grabIdleClip, typeof(AnimationClip), false);
            grabPullClip = (AnimationClip)EditorGUILayout.ObjectField("Grab Pull", grabPullClip, typeof(AnimationClip), false);
            grabPushClip = (AnimationClip)EditorGUILayout.ObjectField("Grab Push", grabPushClip, typeof(AnimationClip), false);

            GUILayout.Label("Pick (Blend Tree)", EditorStyles.boldLabel);
            pickIdleClip = (AnimationClip)EditorGUILayout.ObjectField("Pick Idle", pickIdleClip, typeof(AnimationClip), false);
            pickWalkClip = (AnimationClip)EditorGUILayout.ObjectField("Pick Walk", pickWalkClip, typeof(AnimationClip), false);
            pickRunClip = (AnimationClip)EditorGUILayout.ObjectField("Pick Run", pickRunClip, typeof(AnimationClip), false);

            if (GUILayout.Button("Generate Controller"))
                CreateController();
        }

        private void LoadFromController ()
        {
            AnimatorController ac = existingController as AnimatorController;
            if (ac == null) return;

            controllerName = ac.name;
            if (ac.layers.Length == 0) return;

            AnimatorStateMachine sm = ac.layers[0].stateMachine;

            foreach (var s in sm.states)
            {
                string name = s.state.name;
                Motion m = s.state.motion;

                if (name == "Jump") jumpClip = m as AnimationClip;
                else if (name == "Fall") fallClip = m as AnimationClip;
                else if (name == "Hanging") hangClip = m as AnimationClip;
                else if (name == "Standing") standClip = m as AnimationClip;
                else if (name == "LedgeClimb") ledgeClimbClip = m as AnimationClip;

                else if (name == "Grounded" && m is BlendTree gt)
                {
                    var c = gt.children;
                    if (c.Length > 0) idleClip = c[0].motion as AnimationClip;
                    if (c.Length > 1) walkClip = c[1].motion as AnimationClip;
                    if (c.Length > 2) runClip = c[2].motion as AnimationClip;
                }

                else if (name == "Crouch" && m is BlendTree ct)
                {
                    var c = ct.children;
                    if (c.Length > 0) crouchIdleClip = c[0].motion as AnimationClip;
                    if (c.Length > 1) crouchWalkClip = c[1].motion as AnimationClip;
                }

                else if (name == "Grab" && m is BlendTree grabTree)
                {
                    foreach (var child in grabTree.children)
                    {
                        if (Mathf.Approximately(child.threshold, 0f)) grabIdleClip = child.motion as AnimationClip;
                        else if (child.threshold < 0f) grabPullClip = child.motion as AnimationClip;
                        else if (child.threshold > 0f) grabPushClip = child.motion as AnimationClip;
                    }
                }

                else if (name == "Pick" && m is BlendTree pickTree)
                {
                    foreach (var child in pickTree.children)
                    {
                        if (Mathf.Approximately(child.threshold, 0f)) pickIdleClip = child.motion as AnimationClip;
                        else if (Mathf.Approximately(child.threshold, 0.5f)) pickWalkClip = child.motion as AnimationClip;
                        else if (Mathf.Approximately(child.threshold, 1f)) pickRunClip = child.motion as AnimationClip;
                    }
                }

                else if (name == "WallClimb" && m is BlendTree climbTree)
                {
                    foreach (var child in climbTree.children)
                    {
                        Vector2 pos = child.position;

                        if (pos == Vector2.zero) climbIdleClip = child.motion as AnimationClip;
                        else if (pos == new Vector2(0f, 1f)) climbUpClip = child.motion as AnimationClip;
                        else if (pos == new Vector2(0f, -1f)) climbDownClip = child.motion as AnimationClip;
                        else if (pos == new Vector2(1f, 0f)) climbRightClip = child.motion as AnimationClip;
                        else if (pos == new Vector2(-1f, 0f)) climbLeftClip = child.motion as AnimationClip;
                    }
                }
            }
        }

        private void CreateController ()
        {
            string path = $"{savePath}/{controllerName}.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("CarrySpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("GrabSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsFalling", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsHanging", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsGrabbing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsPicking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Climb", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("WallClimb", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Stand", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ClimbVertical", AnimatorControllerParameterType.Float);
            controller.AddParameter("ClimbHorizontal", AnimatorControllerParameterType.Float);

            AnimatorStateMachine root = controller.layers[0].stateMachine;

            BlendTree groundedTree;
            AnimatorState groundedState = controller.CreateBlendTreeInController("Grounded", out groundedTree);
            groundedTree.blendParameter = "Speed";
            groundedTree.useAutomaticThresholds = false;
            groundedTree.AddChild(idleClip, 0f);
            groundedTree.AddChild(walkClip, 4f);
            groundedTree.AddChild(runClip, 8f);

            BlendTree crouchTree;
            AnimatorState crouchState = controller.CreateBlendTreeInController("Crouch", out crouchTree);
            crouchTree.blendParameter = "Speed";
            crouchTree.useAutomaticThresholds = false;
            crouchTree.AddChild(crouchIdleClip, 0f);
            crouchTree.AddChild(crouchWalkClip, 2f);

            AnimatorState jumpState = root.AddState("Jump");
            jumpState.motion = jumpClip;

            AnimatorState fallState = root.AddState("Fall");
            fallState.motion = fallClip;

            AnimatorState hangState = root.AddState("Hanging");
            hangState.motion = hangClip;

            AnimatorState ledgeClimbState = root.AddState("LedgeClimb");
            ledgeClimbState.motion = ledgeClimbClip;

            BlendTree wallClimbTree;
            AnimatorState wallClimbState = controller.CreateBlendTreeInController("WallClimb", out wallClimbTree);
            wallClimbTree.blendType = BlendTreeType.FreeformDirectional2D;
            wallClimbTree.blendParameter = "ClimbHorizontal";
            wallClimbTree.blendParameterY = "ClimbVertical";
            wallClimbTree.useAutomaticThresholds = false;
            wallClimbTree.AddChild(climbIdleClip, new Vector2(0f, 0f));
            wallClimbTree.AddChild(climbUpClip, new Vector2(0f, 1f));
            wallClimbTree.AddChild(climbDownClip, new Vector2(0f, -1f));
            wallClimbTree.AddChild(climbRightClip, new Vector2(1f, 0f));
            wallClimbTree.AddChild(climbLeftClip, new Vector2(-1f, 0f));

            AnimatorState standState = root.AddState("Standing");
            standState.motion = standClip;

            BlendTree grabTree;
            AnimatorState grabState = controller.CreateBlendTreeInController("Grab", out grabTree);
            grabTree.blendParameter = "GrabSpeed";
            grabTree.useAutomaticThresholds = false;
            grabTree.AddChild(grabPullClip, -1f);
            grabTree.AddChild(grabIdleClip, 0f);
            grabTree.AddChild(grabPushClip, 1f);

            BlendTree pickTree;
            AnimatorState pickState = controller.CreateBlendTreeInController("Pick", out pickTree);
            pickTree.blendParameter = "CarrySpeed";
            pickTree.AddChild(pickIdleClip, 0f);
            pickTree.AddChild(pickWalkClip, 0.5f);
            pickTree.AddChild(pickRunClip, 1f);

            AddTransition(groundedState, crouchState, "IsCrouching", true, 0.1f);
            AddTransition(crouchState, groundedState, "IsCrouching", false, 0.1f);

            AddTransition(groundedState, jumpState, "IsJumping", true, 0.1f);
            AddTransition(groundedState, fallState, "IsGrounded", false, 0.1f);

            AddTransition(jumpState, fallState, "IsFalling", true, 0.1f);
            AddTransition(fallState, groundedState, "IsGrounded", true, 0.1f);

            AddTransition(jumpState, hangState, "IsHanging", true, 0.1f);
            AddTransition(fallState, hangState, "IsHanging", true, 0.1f);

            AddTransitionWithTrigger(hangState, ledgeClimbState, "Climb", 0.1f);

            AddTransitionWithTrigger(jumpState, wallClimbState, "WallClimb", 0.1f);
            AddTransitionWithTrigger(fallState, wallClimbState, "WallClimb", 0.1f);
            AddTransitionWithTrigger(groundedState, wallClimbState, "WallClimb", 0.1f);
            AddTransitionWithTrigger(crouchState, wallClimbState, "WallClimb", 0.1f);


            var drop = hangState.AddTransition(fallState);
            drop.AddCondition(AnimatorConditionMode.IfNot, 0, "IsHanging");
            drop.duration = 0.1f;
            drop.hasExitTime = false;

            AddTransitionWithTrigger(ledgeClimbState, standState, "Stand", 0.1f);

            var standToGrounded = standState.AddTransition(groundedState);
            standToGrounded.hasExitTime = true;
            standToGrounded.exitTime = 0.9f;
            standToGrounded.duration = 0.1f;

            AddTransition(groundedState, grabState, "IsGrabbing", true, 0.1f);
            AddTransition(grabState, groundedState, "IsGrabbing", false, 0.1f);

            AddTransition(groundedState, pickState, "IsPicking", true, 0.1f);
            AddTransition(pickState, groundedState, "IsPicking", false, 0.1f);

            root.defaultState = groundedState;

            AssetDatabase.SaveAssets();
        }

        private void AddTransition (AnimatorState from, AnimatorState to, string param, bool value, float duration)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
            t.duration = duration;
            t.hasExitTime = false;
        }

        private void AddTransitionWithTrigger (AnimatorState from, AnimatorState to, string triggerParam, float duration)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.AddCondition(AnimatorConditionMode.If, 0, triggerParam);
            t.duration = duration;
            t.hasExitTime = false;
        }
    }
}
