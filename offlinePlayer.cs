using SimpleInputNamespace;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class offlinePlayer : MonoBehaviour
{
    public InputActionReference move;
    public SteeringWheel gas;
    public AxisInputUIArrows rul;
    public Rigidbody rb;
    public wheelControllers colliders;
    public wheelMeshes meshes;
    public float breakPower;
    public float motorpower;
    private float slipAngle;
    public float speed;
    public Camera _camera;
    Vector2 inputVector;
    public float gasInsput,steeringInput,breakeInput;
    public GameObject keyboard,mobile;
    public GameObject mobileUI;
    bool UseKeyboard;
    public Vector3 centerOfmass;
    Vector3 lastPos;
    Quaternion lastRot;
    void Start()
    {
        _camera.enabled = true;
        _camera.gameObject.SetActive(true);

        rb.centerOfMass = centerOfmass; // defoult(0.00, 0.92, -0.18)

        ChangeKeyboard();
    }


    void Update()
    {
        speed = rb.linearVelocity.magnitude;
        UpdateWheels();
        checkInput();
        ApplyMotor();
        ApplySteering();
        applyBreak();

        inputVector = move.action.ReadValue<Vector2>();
    }
    public void onglash()
    {
        transform.rotation = lastRot;
        transform.position = lastPos;
    }
    void ApplySteering()
    {
        float angle = steeringInput * 45;
        colliders.FRwheel.steerAngle = angle;
        colliders.FLwheel.steerAngle = angle;
    }
    void ApplyMotor()
    {
        colliders.RRwheel.motorTorque = motorpower * -gasInsput;
        colliders.RLwheel.motorTorque = motorpower * -gasInsput;
    }
    public void ChangeKeyboard()
    {
        UseKeyboard = !UseKeyboard;
        mobile.SetActive(!UseKeyboard);
        keyboard.SetActive(UseKeyboard);
        if (UseKeyboard)
        {
            mobileUI.SetActive(false);
            move.action.Enable();
        }
        else
        {
            mobileUI.SetActive(true);
            move.action.Disable();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Respawn")
        {
            transform.position = lastPos;
            transform.rotation = lastRot;
            // rb.
        }
        else if (other.gameObject.tag == "updatePos")
        {
            lastPos = transform.position;
            lastRot = transform.rotation;
            Destroy(other.gameObject);
        }
    }
    void checkInput()
    {
        if (UseKeyboard)
        {
            gasInsput = inputVector.y;
            steeringInput = inputVector.x;
        }
        else
        {
            gasInsput = rul.Value.x;
            steeringInput = gas.Value;
        }

        slipAngle = Vector3.Angle(transform.forward, rb.linearVelocity-transform.forward);
        if (slipAngle < 120)
        {
            if (gasInsput < 0)
            {
                // breakeInput = Mathf.Abs(gasInsput);
                gasInsput = 0;
            }
            else
            {
                breakeInput = 0;
            }
        }
        else
        {
            breakeInput = 0;
        }
    }


    void applyBreak()
    {
        colliders.FRwheel.brakeTorque = breakPower * breakeInput * 0.7f;
        colliders.FLwheel.brakeTorque = breakPower * breakeInput * 0.7f;
        colliders.RRwheel.brakeTorque = breakPower * breakeInput * 0.3f;
        colliders.RLwheel.brakeTorque = breakPower * breakeInput * 0.3f;
    }
    void UpdateWheels()
    {
        UpdateWheel(colliders.FRwheel, meshes.FRwheel);
        UpdateWheel(colliders.FLwheel, meshes.FLwheel);
        UpdateWheel(colliders.RRwheel, meshes.RRwheel);
        UpdateWheel(colliders.RLwheel, meshes.RLwheel);
    }
    void UpdateWheel(WheelCollider collider, GameObject mesh)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        mesh.transform.position = position;
        mesh.transform.rotation = rotation;
    }
}