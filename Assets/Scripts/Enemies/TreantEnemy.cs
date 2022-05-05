using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreantEnemy : Enemy
{
    [Header("Treant Movement Stuff")]
    public float speed = 2.0f;
    public bool vertical;
    public float maxChangeTime = 3.0f;
    public float minChangeTime = 2.0f;
    public float chanceToPause = 0.3f;
    public float minPauseTime = 2f;
    public float maxPauseTime = 5f;

    public bool canChangeAxis = false;
    public float axisChangeChance = 0.25f;

    private float changeTime;
    private float pauseTime;
    private bool isPaused;

    float timer;
    int direction = 1;

    protected override void Start()
    {
        base.Start();

        CalculateChangeTime();
        timer = changeTime;
    }

    private void FixedUpdate()
    {
        if (currentHealth <= 0) return;

        if (isPaused)
        {
            pauseTime -= Time.fixedDeltaTime;

            if (pauseTime <= 0) isPaused = false;

            direction *= -1;
            CalculateChangeTime();
            CheckForAxisChange();
            timer = changeTime;
        }
        else
        {
            timer -= Time.fixedDeltaTime;

            if (timer < 0)
            {
                // Pause for a time
                if (Random.value <= chanceToPause)
                {
                    CalculatePauseTime();
                    isPaused = true;
                    animator.SetFloat("Speed", 0);
                    rigidbody.velocity = Vector2.zero;

                    return;
                }
                else // or change directions
                {
                    direction *= -1;
                    CalculateChangeTime();
                    CheckForAxisChange();
                    timer = changeTime;
                }
            }

            Vector2 position = rigidbody.position;

            if (vertical)
            {
                position.y += Time.fixedDeltaTime * speed * direction;
                animator.SetFloat("Look X", 0);
                animator.SetFloat("Look Y", direction);
            }
            else
            {
                position.x += Time.fixedDeltaTime * speed * direction;
                animator.SetFloat("Look X", direction);
                animator.SetFloat("Look Y", 0);

                sprite.flipX = (direction == -1);
            }

            animator.SetFloat("Speed", 1);

            rigidbody.MovePosition(position);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player) player.TakeDamage(damage, damageType);
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

    public override void ApplyDamage(float amount)
    {
        //print("Treant started with " + currentHealth + " health, but took " + amount + " damage and is down to " + (currentHealth - amount) + " health");

        base.ApplyDamage(amount);

        rigidbody.velocity = Vector2.zero;
    }

    protected override void Death()
    {
        animator.SetTrigger("Death");

        rigidbody.simulated = false;

        AudioClip deathSound = deathSounds[Random.Range(0, deathSounds.Length)];

        audioSource.PlayOneShot(deathSound);

        Destroy(gameObject, deathSound.length);
    }

    private void CalculateChangeTime()
    {
        changeTime = Random.Range(minChangeTime, maxChangeTime);
    }

    private void CalculatePauseTime()
    {
        pauseTime = Random.Range(minPauseTime, maxPauseTime);
    }

    private void CheckForAxisChange()
    {
        if (canChangeAxis && Random.value <= axisChangeChance) vertical = !vertical;
    }
}
