using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonWeapon : WeaponController
{
    // Melee offsets
    // x north: .4
    // x south: .4
    // y north: 1.5
    // y south: 1.5

    public float maxTimePlayingDead = 5f;
    public float playDeadRegenRate = 0.5f;

    [SerializeField]
    private float playDeadTimeAvailable;

    private PlayerController player;

    protected override void Start()
    {
        base.Start();

        player = GetComponent<PlayerController>();

        // Magic bar is used for showing how long you can play dead
        UIHealthBar.instance.EnableMagicBar();
        UIHealthBar.instance.SetMagicValue(1f);

        if (!loadedData) playDeadTimeAvailable = maxTimePlayingDead;
    }

    public override void Attack(Vector2 lookDirection)
    {
        this.lookDirection = lookDirection;

        if (CanAttack()) Melee();
    }

    private void Melee()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        animator.SetTrigger("Melee Attack");

        StartCoroutine(DisableMovementDuringAttack());
    }


    public override void SpecialAttack(Vector2 lookDirection)
    {
        // play dead
        this.lookDirection = lookDirection;

        PlayDead();
    }

    protected override bool CanAttack()
    {
        return (!animator.GetBool("Attacking") && !animator.GetBool("Playing Dead"));
    }

    private void PlayDead()
    {
        if (!animator.GetBool("Attacking"))
        {
            if (!animator.GetBool("Playing Dead") && playDeadTimeAvailable > 0.01f)
            {
                var audioClip = player.deathClips[Random.Range(0, player.deathClips.Length - 1)];
                PlaySound(audioClip);

                StartCoroutine(PlayingDead());
                animator.SetTrigger("Play Dead");
            }
            else if (animator.GetBool("Invincible")) animator.SetTrigger("Stop Playing Dead");

        }
    }

    private IEnumerator PlayingDead()
    {
        yield return null;

        while (animator.GetBool("Playing Dead") )
        {
            player.SetInvincible(animator.GetBool("Invincible"));

            yield return null;
        }

        player.SetInvincible(false);
    }

    private void Update()
    {
        if (animator.GetBool("Invincible"))
        {
            playDeadTimeAvailable -= Time.deltaTime;
            if (playDeadTimeAvailable <= 0) animator.SetTrigger("Stop Playing Dead");
        }
        else if (playDeadTimeAvailable < maxTimePlayingDead)
        {
            playDeadTimeAvailable += playDeadRegenRate * Time.deltaTime;

            if (playDeadTimeAvailable > maxTimePlayingDead) playDeadTimeAvailable = maxTimePlayingDead;
        }

        UIHealthBar.instance.SetMagicValue(playDeadTimeAvailable / maxTimePlayingDead);

    }

    public override void LoadPlayerData(PlayerLoadData data)
    {
        playDeadTimeAvailable = data.currentMagic;

        loadedData = true;
    }

    public override PlayerLoadData GetPlayerLoadData()
    {
        PlayerLoadData data = new PlayerLoadData();

        data.numProjectiles = 0;
        data.currentMagic = (int)playDeadTimeAvailable;

        return data;
    }

    public override void RestAtShrine()
    {
        playDeadTimeAvailable = maxTimePlayingDead;
    }
}
