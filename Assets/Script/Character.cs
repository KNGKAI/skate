using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
    Idle,
    Push,
    Olie,
    Brake,
    Land,
    Rotate_Left,
    Rotate_Right,
    Grind
}

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    public float accelerate;
    public float deccelerate;
    public float drag;
    public float speed;
    public float rotate;
    public float jump;
    public float airRotateMultiplier;
    [Header("Slopes")]
    public float maxSlope;
    public float minSlope;
    [Header("Grind")]
    public float grindDistance;
    public float grindRadius;
    public float grindMinSpeed;
    public float grindAngleDeadzone;
    public float magnetStrength;
    [Header("Debug")]
    public bool drawDetection;

    private bool busy;
    private bool grounded;
    private bool prevGrounded;
    private Vector3 prevGroundPoint;
    private Vector3 prevGroundNormal;
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
        grounded = IsGrounded(out RaycastHit hit);
        if (Grounded)
        {
            if (!prevGrounded)
            {
                this.State = CharacterState.Land;
            }
            Grind();
            Ground(hit.point, hit.normal);
        }
        else
        {
            Air();
            Magnet();
        }
        Wall();
        prevGrounded = Grounded;

        if (stateIndex > 0)
        {
            stateIndex--;
        }
        else
        {
            State = CharacterState.Idle;
        }
    }

    private void OnDrawGizmos()
    {
        if (drawDetection)
        {
            DrawGrindDetection();
        }
    }

    private bool IsGrounded(out RaycastHit hit)
    {
        return (Physics.Raycast(transform.position + transform.up.normalized * 2.0f, -transform.up, out hit, 2.0f, Character.Mask));
    }

    private void Ground(Vector3 point, Vector3 normal)
    {
        Quaternion rotation;
        Vector3 forward;
        float angle;

        forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;
        rotation = Quaternion.LookRotation(forward, normal);

        angle = Vector3.Angle(Vector3.up, normal);
        if (Vector3.Angle(Vector3.up, forward) < 90)
        {
            angle *= -1.0f;
        }

        switch (State)
        {
            case CharacterState.Idle:
                this.transform.position = point;
                this.transform.rotation = rotation;
                this.Rig.velocity = forward * currentSpeed;
                break;
            case CharacterState.Push:
                this.currentSpeed += accelerate;
                this.transform.position = point;
                this.transform.rotation = rotation;
                this.Rig.velocity = forward * currentSpeed;
                break;
            case CharacterState.Brake:
                currentSpeed -= deccelerate * Time.deltaTime;
                if (currentSpeed < 0.0f)
                {
                    currentSpeed = 0.0f;
                }
                this.transform.position = point;
                this.transform.rotation = rotation;
                this.Rig.velocity = forward * currentSpeed;
                break;
            case CharacterState.Olie:
                Vector3 velocity;

                velocity = this.Rig.velocity;
                if (-angle > minSlope)
                {
                    Vector3 n;

                    n = normal;
                    n.y = 0;
                    velocity = Vector3.ProjectOnPlane(Rig.velocity, n.normalized);
                }
                velocity.y += jump;
                this.Rig.velocity = velocity;
                break;
            case CharacterState.Land:
                this.Land(point, normal);
                this.State = CharacterState.Idle;
                break;
            case CharacterState.Rotate_Left:
                this.transform.position = point;
                this.transform.rotation = rotation;
                this.Rig.velocity = forward * currentSpeed;
                this.transform.Rotate(0.0f, -rotate * Time.fixedDeltaTime, 0.0f, Space.Self);
                break;
            case CharacterState.Rotate_Right:
                this.transform.position = point;
                this.transform.rotation = rotation;
                this.Rig.velocity = forward * currentSpeed;
                this.transform.Rotate(0.0f, rotate * Time.fixedDeltaTime, 0.0f, Space.Self);
                break;
            case CharacterState.Grind:
                this.transform.position = point;
                //this.transform.rotation = rotation;
                this.Rig.velocity = Rig.velocity.normalized * currentSpeed;
                break;
            default:
                break;
        }

        if (-angle > maxSlope)
        {
            Launch(normal);
            return;
        }

        this.currentSpeed += angle / 360.0f;
        if (this.currentSpeed > 0.0f)
        {
            this.currentSpeed -= drag * Time.fixedDeltaTime;
            if (currentSpeed < 0.0f)
            {
                currentSpeed = 0.0f;
            }
        }
        if (this.currentSpeed < 0.0f)
        {
            this.currentSpeed = Mathf.Abs(this.currentSpeed);
            this.Flip();
        }
        if (this.currentSpeed > this.speed)
        {
            this.currentSpeed = this.speed;
        }
        this.prevGroundPoint = point;
        this.prevGroundNormal = normal;
    }

    private void Air()
    {
        RaycastHit hit;
        Vector3 current;
        Vector3 prev;
        Vector3 normal;
        Vector3 velocity;
        Vector3 direction;
        int limit;

        velocity = Rig.velocity;
        current = transform.position;
        void Simulate()
        {
            prev = current;
            current += velocity * Time.fixedDeltaTime;
            direction = current - prev;
            velocity.y -= Time.fixedDeltaTime * 9.8f;
        }
        Simulate();
        limit = 1024;
        while (!Physics.Raycast(current, direction, out hit, velocity.magnitude, Character.Mask))
        {
            Simulate();
            limit--;
            if (limit <= 0)
            {
                break;
            }
        }

        if (Vector3.Angle(prevGroundNormal, hit.normal) > maxSlope)
        {
            hit.normal = prevGroundNormal;
        }
        normal = Vector3.Slerp(transform.up, hit.normal, 0.05f);// Vector3.Dot(transform.position - prevGroundPoint, Vector3.Normalize(hit.point - prevGroundPoint)));
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, normal), normal);

        switch (State)
        {
            case CharacterState.Rotate_Left:
                this.transform.Rotate(0.0f, -rotate * Time.fixedDeltaTime * airRotateMultiplier, 0.0f, Space.Self);
                break;
            case CharacterState.Rotate_Right:
                this.transform.Rotate(0.0f, rotate * Time.fixedDeltaTime * airRotateMultiplier, 0.0f, Space.Self);
                break;
            default:
                break;
        }
    }

    private void Magnet()
    {
        Vector3 current;
        Vector3 prev;
        Vector3 velocity;
        Vector3 direction;
        Vector3 push;
        Vector3 right;
        int it;
        float distance;
        float prevDistance;
        int count;

        right = Vector3.Cross(transform.up, Rig.velocity);
        void Simulate(float offset)
        {
            prev = current;
            velocity += right * offset * Time.fixedDeltaTime;
            velocity.y -= Time.fixedDeltaTime * 9.8f;
            current += velocity * Time.fixedDeltaTime;
            direction = current - prev;
        }
        push = Vector3.zero;
        it = 32;
        distance = 0;
        prevDistance = 0;
        count = 0;
        for (int z = -it; z <= it; z++)
        {
            velocity = Rig.velocity;
            current = transform.position;
            if (z > -it)
            {
                prevDistance = distance;
            }
            distance = 0;
            for (int i = 0; i < 128; i++)
            {
                Simulate(z/ 10.0f);
                if (Physics.Raycast(current, direction, out RaycastHit hit, direction.magnitude))
                {
                    break;
                }
                distance += direction.magnitude;
            }
            if (z > -it && Mathf.Abs(distance - prevDistance) > grindDistance)
            {
                int x;

                x = it - Mathf.Abs(z);
                if (distance - prevDistance > 0)
                {
                    x = -x;
                }
                push += transform.right * x / it * magnetStrength;
                count++;
            }
        }
        if (count > 0)
        {
            push /= count;
            this.Rig.velocity += push * Time.deltaTime;
        }
    }

    private void Land(Vector3 point, Vector3 normal)
    {
        Quaternion rotation;
        Vector3 velocity;
        Vector3 forward;

        velocity = Vector3.ProjectOnPlane(Rig.velocity, normal).normalized;
        forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;
        if (Vector3.Angle(velocity, forward) > 90.0f)
        {
            Vector3 scale;

            scale = this.transform.localScale;
            scale.x = -scale.x;
            scale.z = -scale.z;
            this.transform.localScale = scale;
            forward = -forward;
        }
        rotation = Quaternion.LookRotation(forward, normal);
        //this.currentSpeed = Vector3.ProjectOnPlane(Rig.velocity, transform.up).magnitude;
        this.currentSpeed *= Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(forward, velocity));
        this.transform.position = point;
        this.transform.rotation = rotation;
    }

    private void Wall()
    {
        Ray ray;

        ray = new Ray()
        {
            direction = transform.forward
        };
        bool CheckLayer(float height, out RaycastHit h)
        {
            ray.origin = transform.position + transform.up * height;
            return (Physics.Raycast(ray, out h, 0.5f, Character.Mask));
        };
        float step = 0.25f;
        for (float height = step; height <= 2.0f; height += step)
        {
            if (CheckLayer(height, out RaycastHit hit) && Vector3.Angle(hit.normal, transform.up) > maxSlope)
            {
                Vector3 forward;

                forward = new Vector3(
                    transform.forward.x - 2.0f * transform.forward.x * hit.normal.x * hit.normal.x,
                    transform.forward.y - 2.0f * transform.forward.y * hit.normal.y * hit.normal.y,
                    transform.forward.z - 2.0f * transform.forward.z * hit.normal.z * hit.normal.z
                    );
                this.transform.rotation = Quaternion.LookRotation(forward, transform.up);
                this.currentSpeed /= 2.0f;
                return;
            }
        }
    }

    private void Grind()
    {
        Vector3 root;
        Vector3 position;
        Vector3 direction;
        float angle;

        angle = 30.0f;
        root = transform.position - transform.up * 0.01f;
        for (float i = 0; i < 360.0f; i += angle)
        {
            position = new Vector3(-Mathf.Sin(i), 0.0f, Mathf.Cos(i));
            position = root - (transform.rotation * position) * grindRadius;
            direction = root - position;
            if (!Physics.Raycast(position + transform.up * grindRadius * 2.0f, -transform.up, grindRadius * 2.0f, Character.Mask))
            {
                if (Physics.Raycast(position, direction, out RaycastHit hit, grindRadius * 0.95f, Character.Mask))
                {
                    if (Mathf.Abs(Vector3.Angle(transform.up, hit.normal) - 90.0f) < grindAngleDeadzone &&
                        Mathf.Abs(Vector3.Angle(Vector3.ProjectOnPlane(Rig.velocity, transform.up), hit.normal) - 90.0f) < 30.0f)
                    {
                        if (this.currentSpeed < grindMinSpeed)
                        {
                            this.Rig.velocity = Vector3.ProjectOnPlane(hit.normal, transform.up);
                        }
                        else if (this.State == CharacterState.Olie)
                        {
                            this.Rig.velocity = Rig.velocity + hit.normal;
                        }
                        else
                        {
                            this.Rig.velocity = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(Rig.velocity, hit.normal), transform.up);
                            this.State = CharacterState.Grind;
                        }
                        return;
                    }
                }
            }
        }
    }

    private void DrawGrindDetection()
    {
        Vector3 current;
        Vector3 prev;
        Vector3 velocity;
        Vector3 direction;
        Vector3 right;
        int it;

        right = Vector3.Cross(transform.up, Rig.velocity).normalized;
        void Simulate(float offset)
        {
            prev = current;
            velocity += right * offset * Time.fixedDeltaTime;
            velocity.y -= Time.fixedDeltaTime * 9.8f;
            current += velocity * Time.fixedDeltaTime;
            direction = current - prev;
        }
        it = 32;
        for (int z = -it; z <= it; z++)
        {
            velocity = Rig.velocity;
            current = transform.position;
            for (int i = 0; i < 128; i++)
            {
                Simulate(z / 10.0f);
                if (Physics.Raycast(current, direction, out RaycastHit hit, direction.magnitude))
                {
                    break;
                }
                Gizmos.DrawLine(current, current + direction);
            }
        }
    }

    private void Launch(Vector3 normal)
    {
        normal.y = 0;
        Rig.velocity = Vector3.ProjectOnPlane(Rig.velocity, normal.normalized);
    }

    private void Flip()
    {
        Vector3 scale;

        scale = this.transform.localScale;
        scale.x = -scale.x;
        this.transform.localScale = scale;
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

    public float Speed
    {
        get
        {
            return (this.currentSpeed);
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
                case CharacterState.Push:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Brake:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Olie:
                    this.stateIndex = 5;
                    break;
                case CharacterState.Land:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Rotate_Left:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Rotate_Right:
                    this.stateIndex = 1;
                    break;
                case CharacterState.Grind:
                    this.stateIndex = 1;
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
            State = CharacterState.Push;
        }
    }

    public void Brake()
    {
        if (Grounded)
        {
            State = CharacterState.Brake;
        }
    }

    public void Rotate(bool right)
    {
        if (right)
        {
            this.State = CharacterState.Rotate_Right;
        }
        else
        {
            this.State = CharacterState.Rotate_Left;
        }
    }

    public void Olie()
    {
        if (Grounded)
        {
            State = CharacterState.Olie;
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
