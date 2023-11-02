using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR_OSX || UNITY_IOS
using UnityCoreBluetooth;

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

    private CoreBluetoothManager manager;
    private CoreBluetoothCharacteristic characteristic;

    // Use this for initialization
    void Start()
    {
        manager = CoreBluetoothManager.Shared;

        manager.OnUpdateState((string state) =>
        {
            Debug.Log("state: " + state);
            if (state != "poweredOn") return;
            manager.StartScan();
        });

        manager.OnDiscoverPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            if (peripheral.name != "")
                Debug.Log("discover peripheral name: " + peripheral.name);
            if ((peripheral.name != BleDevice.DEVICE_NAME ) && (peripheral.name != "M5Stack") && (peripheral.name != "M5StickC")) return;

            manager.StopScan();
            manager.ConnectToPeripheral(peripheral);
        });

        manager.OnConnectPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            Debug.Log("connected peripheral name: " + peripheral.name);
            peripheral.discoverServices();
        });

        manager.OnDiscoverService((CoreBluetoothService service) =>
        {
            Debug.Log("discover service uuid: " + service.uuid);
            if (service.uuid.ToLower() != BleDevice.SERVICE_UUID) return;
            service.discoverCharacteristics();
        });


        manager.OnDiscoverCharacteristic((CoreBluetoothCharacteristic characteristic) =>
        {
            this.characteristic = characteristic;
            string uuid = characteristic.Uuid;
            string[] usage = characteristic.Propertis;
            Debug.Log("discover characteristic uuid: " + uuid + ", usage: " + usage);
            for (int i = 0; i < usage.Length; i++)
            {
                Debug.Log("discover characteristic uuid: " + uuid + ", usage: " + usage[i]);
                if (usage[i] == "notify")
                    characteristic.SetNotifyValue(true);
            }
        });

        manager.OnUpdateValue((CoreBluetoothCharacteristic characteristic, byte[] data) =>
        {
            this.value = data;
            this.flag = true;

            this.command = System.Text.Encoding.ASCII.GetString(data);
            Debug.Log("update value => " + this.command);
        });
        manager.Start();
    }

    private bool flag = false;
    private byte[] value = new byte[20];
    private string command = "";

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
        vy += 0.1f;
        transform.position += new Vector3(0, vy, 0);
    }

    void OnDestroy()
    {
        manager.Stop();
    }

    private int counter = 0;

    public void Write()
    {
        characteristic.Write(System.Text.Encoding.UTF8.GetBytes($"{counter}"));
        counter++;
    }
}
#endif
