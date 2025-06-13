using SimpleInputNamespace;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            _int = 1,
            _bool = true,
            _string = ""
        },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public string _string;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _string);
        }
    }
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + " ; " + newValue._int + " ; " + newValue._bool + " ; " + newValue._string);
        };
    }

    [ServerRpc]
    private void ServerRPC()
    {
        Debug.Log("ServerRPC called by client: " + OwnerClientId);
    }
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
    void Start()
    {
        if (!IsOwner)
        {
            _camera.enabled = false;
            _camera.gameObject.SetActive(false);
        }
        else
        {
            _camera.enabled = true;
            _camera.gameObject.SetActive(true);
        }
        ChangeKeyboard();
    }


    void Update()
    {
        if (!IsOwner) return;

        speed = rb.linearVelocity.magnitude;
        UpdateWheels();
        checkInput();
        ApplyMotor();
        ApplySteering();
        applyBreak();

        inputVector = move.action.ReadValue<Vector2>();

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     ServerRPC();
        //     // randomNumber.Value = new MyCustomData
        //     // {
        //     //     _int = Random.Range(0, 100),
        //     //     _bool = false,
        //     //     _string = "Hello from " + OwnerClientId
        //     // };
        // }
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
        if (!IsOwner) return;
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
                breakeInput = Mathf.Abs(gasInsput);
                gasInsput = 0;
            }
        }
        else{
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


//------------------------------------------------------------------------------------------------------------
[System.Serializable]
public class wheelControllers
{
    public WheelCollider FRwheel;
    public WheelCollider FLwheel;
    public WheelCollider RRwheel;
    public WheelCollider RLwheel;
}
[System.Serializable]
public class wheelMeshes
{
    public GameObject FRwheel;
    public GameObject FLwheel;
    public GameObject RRwheel;
    public GameObject RLwheel;
}