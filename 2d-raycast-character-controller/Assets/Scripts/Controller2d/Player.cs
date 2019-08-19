using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Player : MonoBehaviour
{
    [Range(0, 50)]
    public float MoveSpeed = 6;
    [Range(0, 1)]
    public float accelerationTimeAirborne = .2f;
    [Range(0, 1)]
    public float accelerationTimeGrounded = .1f;
    [Range(0, 50)]
    public float MaxJumpHeight = 4;
    [Range(0, 50)]
    public float minJumpHeight = 1;
    [Range(0, 10)]
    public float TimeToJumpApex = .4f;
    [Range(0, 4)]
    public int ExtraJumps = 1;
    [Range(2, 4)]
    public float fallMultiplier = 2.5f;
    public bool VariableJumpHeight;
    [Range(0, 10)]
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    float timeToWallUnstick;
    int jumps;
    float maxJumpVelocity;
    float minJumpVelocity;
    float gravity;
    float velocityXSmoothing;
    Vector3 velocity;
    Controller controller;
    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    void Start()
    {
        controller = GetComponent<Controller>();
        gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * TimeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        jumps = ExtraJumps;
        print("Gravity: " + gravity + "Max Jump Velocity " + maxJumpVelocity);
    }

    void FixedUpdate()
    {
        CalculateVelocity();
        HandleWallSliding();

        wallDirX = (controller.collisions.left) ? -1 : 1;

        if (controller.collisions.below || controller.collisions.left || controller.collisions.right)
        {
            jumps = ExtraJumps;
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below)
        {
            if (directionalInput.y != -1)
            {
                velocity.y = maxJumpVelocity;
            }
        }
        if (jumps > 0 && directionalInput.y != -1)
        {
            velocity.y = maxJumpVelocity;
            jumps--;
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }


    void HandleWallSliding()
    {
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick -= wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * MoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
        if (velocity.y < 0)
        {
            velocity += Vector3.up * gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
    }
}
