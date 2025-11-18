//using UnityEngine;

//public class PlayerLedgeHandler
//{
//    //private PlayerController player;
//    //private CharacterController controller;
//    //private PlayerInputHandler input;

//    //public bool IsGrabbingLedge { get; private set; } = false;
//    //private bool wantsToDrop;

//    //public PlayerLedgeHandler(PlayerController player, CharacterController controller, PlayerInputHandler input)
//    //{
//    //    this.player = player;
//    //    this.controller = controller;
//    //    this.input = input;
//    //}
//    //public bool WantsToDrop() => wantsToDrop;
//    //public void ResetDropIntent() => wantsToDrop = false;

//    //public bool CheckForLedge(out Vector3 ledgePoint)
//    //{
//    //    ledgePoint = Vector3.zero;
//    //    RaycastHit hit;
//    //    IsGrabbingLedge = false;

//    //    Vector3 origin = player.transform.position + Vector3.up * 1.5f;
//    //    if (Physics.Raycast(origin, player.transform.forward, out hit, 0.5f))
//    //    {
//    //        Vector3 topCheck = hit.point + Vector3.up * 1f;
//    //        if (!Physics.Raycast(topCheck, Vector3.down, 1f))
//    //        {
//    //            ledgePoint = hit.point;
//    //            IsGrabbingLedge = true;
//    //        }
//    //    }
//    //    return IsGrabbingLedge;
//    //}

//    //public void HandleLedgeGrab(Vector3 ledgePoint)
//    //{
//    //    player.Velocity = Vector3.zero;

//    //    Vector3 grabPosition = ledgePoint - player.transform.forward * 0.3f;
//    //    grabPosition.y = ledgePoint.y - controller.height * 0.5f;
//    //    player.transform.position = grabPosition;

//    //    if (input.IsJumping)
//    //    {
//    //        ClimbUp(ledgePoint);
//    //    }
//    //    else if (input.IsCrouching)
//    //    {
//    //        DropDown();
//    //    }
//    //}

//    //private void ClimbUp(Vector3 ledgePoint)
//    //{
//    //    Vector3 climbPosition = ledgePoint + player.transform.forward * 0.5f;
//    //    climbPosition.y += controller.height;
//    //    player.transform.position = Vector3.Lerp(player.transform.position, climbPosition, Time.deltaTime * 10f);
//    //    IsGrabbingLedge = false;
//    //}

//    //private void DropDown()
//    //{
//    //    wantsToDrop = true;
//    //    IsGrabbingLedge = false;
//    //}
//}
