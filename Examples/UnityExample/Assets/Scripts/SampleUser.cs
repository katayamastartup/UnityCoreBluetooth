using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR_OSX || UNITY_IOS
using UnityCoreBluetooth;

using BLEService;

/// <summary>
/// BLEデバイスのの設定値
/// </summary>
static class BleDevice
{
    public const string SERVICE_UUID        = "55725ac1-066c-48b5-8700-2d9fb3603c5e";
    public const string CHARACTERISTIC_UUID = "69ddb59c-d601-4ea4-ba83-44f679a670ba";
    public const string DEVICE_NAME         = "MyBLEDevice";
}

public class SampleUser : MonoBehaviour
{

    public Text text;
    public Text txtMessageLength;

    public RawImage cameraImage;

    public Texture2D sampleEarth;
    public Texture2D sampleText;

    // private CoreBluetoothManager manager;
    // private CoreBluetoothCharacteristic characteristic;

    private CBLEManager bleManager;

    private CImageReceiver imageReceiver;

    // Use this for initialization
    void Start()
    {
        // manager = CoreBluetoothManager.Shared;
        bleManager = new CBLEManager(CameraMountPosition.UNKNOWN);

        bleManager.StartBLE((byte[] data) => {
            this.value = data;
            this.flag = true;

            this.command = System.Text.Encoding.ASCII.GetString(data).TrimEnd('\0');
            Debug.Log("update value => " + this.command);

            this.txtMessageLength.text = $"Length: {data.Length}";

            this.isText = (data.Length < 32);

            imageReceiver.commandReceive(data, (byte[] imgData, ushort imageDataLen) =>{
                Debug.Log(
                    String.Format("image data recv complite length=>{0}", imageDataLen));
            });
        });

        imageReceiver = new CImageReceiver();

        // this.txtMessageLength = GameObject.Find("MessageLength") as Text;
    }

    private bool flag = false;
    private byte[] value = new byte[20];
    private string command = "";
    private bool isText = false;

    private float vy = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (this.transform.position.y < 0)
        {
            vy = 0.0f;
            transform.position = new Vector3(0, 0, 0);
        }
        else
        {
            vy -= 0.006f;
            transform.position += new Vector3(0, vy, 0);
        }
        this.transform.Rotate(2, -3, 4);

        if (flag == false) return;

        flag = false;
        // text.text = $"Notify: {BitConverter.ToInt32(value, 0)}";
        text.text = $"Notify: {this.command}";

        byte[] imgBytes =  (this.isText == true)?
            this.sampleEarth.EncodeToJPG():
            this.sampleText.EncodeToJPG();

        Texture2D texture = new Texture2D(100, 100, TextureFormat.ARGB32, false);
        texture.LoadImage(imgBytes);
        this.cameraImage.texture = texture;

        vy += 0.1f;
        transform.position += new Vector3(0, vy, 0);
    }

    void OnDestroy()
    {
        bleManager.StopBLE();
    }

    private int counter = 0;

    public void Write()
    {
        bleManager.WriteBLE(System.Text.Encoding.UTF8.GetBytes($"{counter}"));
        counter++;
    }
}
#endif
