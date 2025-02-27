using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HP : NetworkBehaviour
{
    [SerializeField] protected float maxHP;

    [Networked(OnChanged = nameof(OnHPChanged))] protected float currentHP { get; set; }
    
    [SerializeField] protected UnityEvent OnDeath;

    public override void Spawned()
    {
        //reduceHPToServ(-maxHP);
        currentHP = maxHP;
    }

    public void InitializeHP(float _hp)
    {
        if (currentHP != 0 || _hp <= 0f) return;

        currentHP = _hp;
    }
    
    public static void OnHPChanged(Changed<HP> changed)
    {
        changed.Behaviour.ChangeHP();
    }

    private void ChangeHP()
    {
        //Debug.Log(currentHP + " : " + name);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    protected virtual void playDamage()
    {
        
    }

    public virtual void reduceHPToServ(float damage)
    {
        if (Object.HasInputAuthority) reduceHPRpc(damage);
    }

    public virtual void TrueReduceHP(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    protected void reduceHPRpc(float hpReduce)
    {
        TrueReduceHP(hpReduce);
    }
    
    
}

