using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAndForthEnemy : Damageable
{
    public float speed = 2.0f;
    public bool vertical;
    public float maxChangeTime = 3.0f;
    public float minChangeTime = 2.0f;

    public bool canChangeAxis = false;
    public float axisChangeChance = 0.25f;

    public int damage = 1;

    private float changeTime;

    private bool broken;

    new Rigidbody2D rigidbody2D;
    Animator animator;
    ParticleSystem smokeEffect;

    float timer;
    int direction = 1;

    protected override void Start()
    {
        base.Start();

        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        CalculateChangeTime();
        timer = changeTime;

        broken = true;

        smokeEffect = GetComponentInChildren<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if (!broken) return;

        timer -= Time.fixedDeltaTime;

        if (timer < 0)
        {
            direction *= -1;
            CalculateChangeTime();
            CheckForAxisChange();
            timer = changeTime;
        }

        Vector2 position = rigidbody2D.position;

        if (vertical)
        {
            position.y += Time.fixedDeltaTime * speed * direction;
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else
        {
            position.x += Time.fixedDeltaTime * speed * direction;
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }

        rigidbody2D.MovePosition(position);
    }

    private void CalculateChangeTime()
    {
        changeTime = Random.Range(minChangeTime, maxChangeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player) player.TakeDamage(damage);
        else
        {
            if (canChangeAxis)
            {
                bool axis = vertical;
                CheckForAxisChange();

                // If axis change failed, just go the other way
                if (axis == vertical) direction *= -1;
            }
            else direction *= -1; ;
        }
    }

    private void CheckForAxisChange()
    {
        if (canChangeAxis && Random.value <= axisChangeChance) vertical = !vertical;
    }

    public void Fix()
    {
        broken = false;
        rigidbody2D.simulated = false;
        animator.SetTrigger("Fixed");
        smokeEffect.Stop();
    }

    public override void ApplyDamage(float amount)
    {
        base.ApplyDamage(amount);

        print("robot took " + amount + " damage. " + currentHealth + "/" + maxHealth);
    }

    protected override void Death()
    {
        print("robot died");
    }
}
