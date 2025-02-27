using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Action
{
    public PlayerActionType action;
    public CamType fpsCam, tpsCam;

    [HideInInspector]
    public CamType _currCam;
}

[System.Serializable]
public struct CamType
{
    public bool allow;
    public bool startWith;
    public CameraViewType camView;
    public Transform camTransform;
}

[System.Serializable]
public class PlayerCamera
{
    //EXPERIMENTAL
    [HideInInspector]
    public PlayerCameraHandler player;

    public AnimationCurve moveSpeed;

    public Camera cam;

    public PlayerActionType startAction;

    public bool tpsCamFollowCar;
    public bool tpsCamInvertX;
    public bool tpsCamInvertY;
    int camInvertX, camInvertY;
    public float cameraAngularVelocity = 60f;
    float cameraDistance;
    public float cameraAngleY = 0;
    public float cameraAngleX = 0;
    public float maxNegAngleX = 0;

    [HideInInspector]
    public Action currAction;

    public Action walk, drive, shoot;

    bool _isMoving = false;

    private Rewired.Player rPlayer;

    public void Init()
    {
        cameraDistance = Vector3.Distance(player.transform.position, cam.transform.position);
        rPlayer = Rewired.ReInput.players.GetPlayer(0);
        switch (startAction)
        {
            case PlayerActionType.WALKING:
                currAction = walk;
                break;

            case PlayerActionType.DRIVING:
                currAction = drive;
                break;

            case PlayerActionType.SHOOTING:
                currAction = shoot;
                break;
        }

        if (currAction.fpsCam.startWith) currAction._currCam = currAction.fpsCam;
        else if (currAction.tpsCam.startWith) currAction._currCam = currAction.tpsCam;
        else currAction._currCam = currAction.tpsCam;
        cam.transform.position = currAction._currCam.camTransform.position;
        cam.transform.rotation = currAction._currCam.camTransform.rotation;
        cameraAngleX = 20f; //FIXME
        camInvertX = tpsCamInvertX ? -1 : 1;
        camInvertY = tpsCamInvertY ? 1 : -1;
    }


    public void ChangeCameraPosition()
    {
        if (_isMoving) return;

        switch (currAction._currCam.camView)
        {
            case CameraViewType.FPS:
                if (!currAction.tpsCam.allow) return;
                player.StartCamCoroutine(currAction._currCam.camTransform, currAction.tpsCam.camTransform);
                currAction._currCam = currAction.tpsCam;
                break;

            case CameraViewType.TPS:
                if (!currAction.fpsCam.allow) return;
                player.StartCamCoroutine(cam.transform, currAction.fpsCam.camTransform);
                currAction._currCam = currAction.fpsCam;
                //TEMPORARY
                cameraAngleX = 20f;
                cameraAngleY = 0f;
                break;
        }

        
        
    }

    public IEnumerator CameraChangeCoroutine(Transform start, Transform end)
    {
        _isMoving = true;
        float index = 0;
        while (index < 1)
        {
            cam.transform.position = Vector3.Lerp(start.position, end.position, index);
            cam.transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, index);
            index += Time.deltaTime * moveSpeed.Evaluate(index);
            yield return null;
        }
        _isMoving = false;
        yield return null;
    }

    
    public void UpdateCamera()
    {
        if (tpsCamFollowCar || currAction._currCam.camView == CameraViewType.FPS) return;

        float angleDelta = cameraAngularVelocity * Time.deltaTime;

        cameraAngleX += rPlayer.GetAxis("MoveCamY") * angleDelta * camInvertX;
        cameraAngleY += rPlayer.GetAxis("MoveCamX") * angleDelta * camInvertY;

        //Protections
        cameraAngleX = Mathf.Clamp(cameraAngleX, maxNegAngleX, 90f);
        cameraAngleY = Mathf.Repeat(cameraAngleY, 360f);

        Quaternion cameraRotation =
            Quaternion.AngleAxis(cameraAngleY, Vector3.up)
            * Quaternion.AngleAxis(cameraAngleX, Vector3.right);

        Vector3 cameraPosition =
            player.transform.position
            + cameraRotation * Vector3.back * cameraDistance;

        cam.transform.position = cameraPosition;
        cam.transform.rotation = cameraRotation;

    }
}
