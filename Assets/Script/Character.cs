using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
    Idle,
    Move,
    Jump
}

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    public float accelerate;
    public float speed;
    public float deccelerate;
    public float rotate;
    public float jump;
    [Header("Slopes")]
    public float maxSlope;

    private bool busy;
    private bool grounded;
    private bool prevGrounded;
    private Rigidbody rig;
    
    private float currentSpeed;

    private int stateIndex;
    private CharacterState state;

    private void Awake()
    {
        this.busy = false;
        this.gameObject.layer = LayerMask.NameToLayer("Character");
        this.currentSpeed = 0.0f;
    }

    private void FixedUpdate()
    {
        bool groundHit;

        groundHit = Physics.Raycast(transform.position + transform.up.normalized * 2.0f, -transform.up, out RaycastHit hit, 2.0f, Mask);
        grounded = Physics.CheckSphere(transform.position, 0.1f, Character.Mask);
        if (Grounded && groundHit)
        {
            if (grounded && !prevGrounded)
            {
                this.Land(hit.point, hit.normal);
            }
            switch (State)
            {
                case CharacterState.Idle:
                    Ground(hit.point, hit.normal);
                    break;
                case CharacterState.Move:
                    Ground(hit.point, hit.normal);
                    break;
                case CharacterState.Jump:
                    Vector3 velocity;

                    velocity = this.Rig.velocity;
                    velocity.y = jump;
                    this.Rig.velocity = velocity;
                    break;
                default:
                    break;
            }
        }
        else
        {
            Air();
        }
        prevGrounded = Grounded && groundHit;
    }

    private void LateUpdate()
    {
        if (stateIndex > 0)
        {
            stateIndex--;
        }
        else
        {
            State = CharacterState.Idle;
        }
    }

    private void Ground(Vector3 point, Vector3 normal)
    {
        Quaternion rotation;
        Vector3 forward;
        float angle;

        if (Vector3.Angle(Vector3.up, normal) > 90.0f)
        {
            return;
        }

        forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;
        rotation = Quaternion.LookRotation(forward, normal);
        this.transform.position = point;
        this.transform.rotation = rotation;
        this.Rig.velocity = transform.forward * currentSpeed;

        angle = Vector3.Angle(Vector3.up, normal);
        if (Vector3.Angle(Vector3.up, forward) < 90)
        {
            angle *= -1.0f;
        }

        if (-angle > maxSlope)
        {
            Vector3 velocity;

            normal.y = 0;
            velocity = Vector3.ProjectOnPlane(Rig.velocity, normal.normalized);
            Rig.velocity = velocity;
            return;
        }

        this.currentSpeed += angle / 180.0f;
        if (this.currentSpeed < 0.0f)
        {
            this.currentSpeed = 0.0f;
            this.Flip();
        }
        if (this.currentSpeed > this.speed)
        {
            this.currentSpeed = this.speed;
        }
    }

    private void Air()
    {
        Vector3 current;
        Vector3 prev;
        Vector3 normal;
        Vector3 velocity;
        Vector3 direction;
        float distance;

        velocity = Rig.velocity;
        current = transform.position;
        for (int i = 0; i < 100; i++)
        {
            prev = current;
            current += velocity * Time.fixedDeltaTime;
            direction = current - prev;
            velocity.y -= Time.fixedDeltaTime;
            distance = Vector3.Distance(current, prev);
            if (Physics.Raycast(current, direction, out RaycastHit hit, distance))
            {
                normal = Vector3.Lerp(transform.up, hit.normal, 0.1f);
                transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, normal), normal);
                break;
            }
        }
    }

    private void Land(Vector3 point, Vector3 normal)
    {
        Quaternion rotation;
        Vector3 forward;

        forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;
        if (Vector3.Angle(forward, Vector3.ProjectOnPlane(Rig.velocity, normal)) > 90.0f)
        {
            forward = -forward;
        }
        rotation = Quaternion.LookRotation(forward, normal);
        this.transform.position = point;
        this.transform.rotation = rotation;
    }

    private void Flip()
    {
        this.transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up);
    }

    public bool Busy
    {
        get
        {
            return (this.busy);
        }
    }

    public bool Grounded
    {
        get
        {
            return (this.grounded);
        }
    }

    public Rigidbody Rig
    {
        get
        {
            if (this.rig == null)
            {
                this.rig = this.GetComponent<Rigidbody>();
            }
            return (this.rig);
        }
    }

    public CharacterState State
    {
        get
        {
            return (this.state);
        }
        set
        {
            switch (value)
            {
                case CharacterState.Idle:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Move:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Jump:
                    this.stateIndex = 10;
                    break;
                default:
                    break;
            }
            this.state = value;
        }
    }

    public void Push()
    {
        if (Grounded)
        {
            currentSpeed += accelerate;
        }
    }

    public void Rotate(float amount)
    {
        amount = rotate * Mathf.Clamp(amount, -1.0f, 1.0f) * Time.deltaTime;
        if (!Grounded)
        {
            amount *= 2;
        }
        this.transform.Rotate(0.0f, amount, 0.0f, Space.Self);
    }

    public void Jump()
    {
        if (Grounded)
        {
            State = CharacterState.Jump;
        }
    }

    private static bool maskCheck = false;
    private static LayerMask mask;

    public static LayerMask Mask
    {
        get
        {
            if (!maskCheck)
            {
                mask = ~(1 << LayerMask.NameToLayer("Character"));
                maskCheck = true;
            }
            return (mask);
        }
    }
}
