using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR_OSX || UNITY_IOS
using UnityCoreBluetooth;
using UnityCoreBluetooth.NativeInterface;

namespace BLEService {

    /// <summary>
    /// カメラ取付位置（種別）
    /// </summary>
    // [Flags]
    public enum CameraMountPosition{

        /// <summary> 左</summary>
        LEFT = 0x1000,
        /// <summary> 右</summary>
        RIGHT = 0x0001,
        /// <summary> 上</summary>
        UP = 0x1100,
        /// <summary> 下</summary>
        BOTTOM = 0x0011,
        /// <summary> 未定義</summary>
        UNKNOWN = 0x11111,
    }

    /// <summary>
    /// BLEデバイスのの設定値
    /// </summary>
    static class BLEDevice
    {
        public const string SERVICE_UUID            = "55725ac1-066c-48b5-8700-2d9fb3603c5e";
        public const string CHARACTERISTIC_UUID     = "69ddb59c-d601-4ea4-ba83-44f679a670ba";
        /// <summary> デバイス名：汎用</summary>
        public const string DEVICE_NAME = "MyBLEDevice";

        /// <summary> 左側カメラデバイス名</summary>
        public const string DEVICE_NAME_LEFT_CAMERA = "CAPXLE_VR_LEFT_CAMERA";
        /// <summary> 右側カメラデバイス名</summary>
        public const string DEVICE_NAME_RIGHT_CAMERA = "CAPXLE_VR_RIGHT_CAMERA";
        /// <summary> 上側カメラデバイス名</summary>
        public const string DEVICE_NAME_UP_CAMERA = "CAPXLE_VR_UP_CAMERA";
        /// <summary> 下側カメラデバイス名</summary>
        public const string DEVICE_NAME_BOTTOM_CAMERA = "CAPXLE_VR_BOTTOM_CAMERA";
    }

    /// <summary>
    /// BLE管理クラス
    /// </summary>
    public class CBLEManager
    {
        /// <summary>デバイス名(ローカル)</summary>
        private string _deviceName;

        /// <summary>CoreのManager</summary>
        private CoreBluetoothManager _coreBluetoothManager;
        private CoreBluetoothCharacteristic _characteristic;

        private Action<byte[]> _onBLEDataReceive;

        /// <summary>デバイス名(取得専用)</summary>
        public string DeviceName {
            get { return this._deviceName;}
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public CBLEManager() {
            this.initialize(CameraMountPosition.UNKNOWN);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="camPos">カメラ取付位置</param>
        public CBLEManager(CameraMountPosition cmdPos) {
            this.initialize(cmdPos);
        }

        /// <summary>
        /// BLEの開始
        /// </summary>
        /// <param name="onBLEDataReceive">BLEデータ受信時のハンドラ</param>
        public void StartBLE(Action<byte[]> onBLEDataReceive) {
            this._coreBluetoothManager.Start();

            this._onBLEDataReceive = onBLEDataReceive;
        }

        public void StopBLE() {
            this._coreBluetoothManager.Stop();

            this._onBLEDataReceive = null;
        }

        public void WriteBLE(byte[] data) {
            this._characteristic.Write(data);
        }

        private void initialize(CameraMountPosition camPos) {
            this._setDeviceName(camPos);

            // TODO:コンストラクタのpublic化検討
            // this._coreBluetoothManager = new UnityCoreBluetoothManagerWrapper();
            this._coreBluetoothManager = CoreBluetoothManager.Shared;

            this._onBLEDataReceive = null;

#region  CoreBluetoothManager delegate
        this._coreBluetoothManager.OnUpdateState((string state) =>
        {
            Debug.Log("state: " + state);
            if (state != "poweredOn") return;
            this._coreBluetoothManager.StartScan();
        });

        this._coreBluetoothManager.OnDiscoverPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            if (peripheral.name != "")
                Debug.Log("discover peripheral name: " + peripheral.name);
            if ((peripheral.name != BleDevice.DEVICE_NAME ) && (peripheral.name != "M5Stack") && (peripheral.name != "M5StickC")) return;

            this._coreBluetoothManager.StopScan();
            this._coreBluetoothManager.ConnectToPeripheral(peripheral);
        });

        this._coreBluetoothManager.OnConnectPeripheral((CoreBluetoothPeripheral peripheral) =>
        {
            Debug.Log("connected peripheral name: " + peripheral.name);
            peripheral.discoverServices();
        });

        this._coreBluetoothManager.OnDiscoverService((CoreBluetoothService service) =>
        {
            Debug.Log("discover service uuid: " + service.uuid);
            if (service.uuid.ToLower() != BleDevice.SERVICE_UUID) return;
            service.discoverCharacteristics();
        });


        this._coreBluetoothManager.OnDiscoverCharacteristic((CoreBluetoothCharacteristic characteristic) =>
        {
            this._characteristic = characteristic;
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

        this._coreBluetoothManager.OnUpdateValue((CoreBluetoothCharacteristic characteristic, byte[] data) =>
        {
            if (this._onBLEDataReceive != null) {
                this._onBLEDataReceive(data);
            }
        });
#endregion

        }

        /// <summary>
        /// デバイス名の設定
        /// </summary>
        /// <param name="camPos">カメラ取り付け位置</param>
        private void _setDeviceName(CameraMountPosition camPos) {
            switch (camPos) {
                case CameraMountPosition.LEFT:
                    this._deviceName = BLEDevice.DEVICE_NAME_LEFT_CAMERA;
                    break;
                case CameraMountPosition.RIGHT:
                    this._deviceName = BLEDevice.DEVICE_NAME_RIGHT_CAMERA;
                    break;
                case CameraMountPosition.UP:
                    this._deviceName = BLEDevice.DEVICE_NAME_UP_CAMERA;
                    break;
                case CameraMountPosition.BOTTOM:
                    this._deviceName = BLEDevice.DEVICE_NAME_BOTTOM_CAMERA;
                    break;
                default:
                    this._deviceName = BLEDevice.DEVICE_NAME;
                    break;
            }
        }

    }
}

#endif