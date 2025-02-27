using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CarInteractable : Interactable
{
    [SerializeField] private GameObject camera;
    [SerializeField] private Transform seat;
    [SerializeField] private Transform exit;
    
    [Networked] public bool isPossessed { get; set; }

    private float timer;
    private NetworkedPlayer _playerController;
    [SerializeField] private TruckPhysics truck;
    
    public override string GetDescription()
    {
        return "Press <color=green>[E]</color> to drive the vehicule";
    }

    public override void Interact(PlayerInteraction interactor)
    {
        AskForOwnershipServerRpc();
    }

    private void Update()
    {
        if (Object == null) return;
        
        if (Object.HasStateAuthority)
        {
            if (timer > 0) timer -= Time.deltaTime;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    void AskForOwnershipServerRpc(RpcInfo info = default)
    {
        if (isPossessed) return;

        isPossessed = true;
        Object.AssignInputAuthority(info.Source);
        _playerController = Runner.GetPlayerObject(info.Source).gameObject.GetComponent<NetworkedPlayer>();
        _playerController.transform.SetParent(transform);
        _playerController.Possess(transform);
        ConfirmPossessionClientRpc();
        SetParentingClientRpc();
        timer = .2f;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void ConfirmPossessionClientRpc()
    {
        camera.SetActive(true);
        CanvasInGame.Instance.showTruck(true);
        GetComponent<TruckFuel>().ChangeFuel();
        NetworkedPlayer player = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<NetworkedPlayer>();
        player.ChangeInputHandler(PossessingType.CAR, gameObject);
        truck.StartTruck();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void SetParentingClientRpc()
    {
        var test = Runner.GetPlayerObject(Object.InputAuthority);
        Transform _playerTransform = test.transform;
        _playerTransform.SetParent(seat);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void AskForExitServerRpc(RpcInfo info = default)
    {
        if (!isPossessed) return;
        if (timer > 0) return;

        isPossessed = false;
        Object.RemoveInputAuthority();
        _playerController.transform.SetParent(null);
        _playerController.Unpossess(exit);
        _playerController = null;
        ConfirmExitClientRpc(info.Source);
        truck.Started = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void ConfirmExitClientRpc([RpcTarget] PlayerRef target)
    {
        camera.SetActive(false);
        CanvasInGame.Instance.showTruck(false);
        _playerController = Runner.GetPlayerObject(target).gameObject.GetComponent<NetworkedPlayer>();
        _playerController.gameObject.SetActive(true);
        _playerController.transform.SetParent(null);
        _playerController.ChangeInputHandler(PossessingType.CHARACTER, gameObject);
        _playerController = null;
        truck.onExit();
    }
}
