using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BLEService {

    /// <summary>
    /// BLE関連定数クラス
    /// </summary>
    public static class CBLEConstant
    {
        /// <summary>BLE最大送信サイズ</summary>
        public const int BLE_MAX_SIZE = 512;

        /// <summary>コマンド文字列長さ</summary>
        public const int COMMAND_LENGTH = 8;

        /// <summary>コマンド長さ</summary>
        public const int COMMAND_DATA_LENGTH = 16;

        /// <summary>Imageデータ長さ</summary>
        public const int IMAGE_DATA_SIZE = BLE_MAX_SIZE - COMMAND_DATA_LENGTH;
    }

    /// <summary>
    /// Imageコマンドデータ
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct ImageCommandData {
        /// <summary>コマンド文字列：START or DATA or END</summary>
        [FieldOffset(0)]
        public fixed byte command[CBLEConstant.COMMAND_LENGTH];

        /// <summary>シーケンス番号</summary>
        [FieldOffset(8)]
        public fixed uint sequenceNumber[1];

        /// <summary>Imageデータ長さ/送信長さ</summary>
        [FieldOffset(12)]
        public fixed ushort dataLength[1];

        /// <summary>送信分割番号</summary>
        [FieldOffset(14)]
        public fixed ushort splitNumber[1];

        /// <summary>Imageデータ:分割で受信</summary>
        [FieldOffset(16)]
        public fixed byte imageDataArray[CBLEConstant.IMAGE_DATA_SIZE];
    };

    [StructLayout(LayoutKind.Sequential)]
    struct ImageCommandData2 {
        /// <summary>コマンド文字列：START or DATA or END</summary>
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=CBLEConstant.COMMAND_LENGTH)]
        public string command;

        /// <summary>シーケンス番号</summary>
        [MarshalAs(UnmanagedType.I4)]
        public uint sequenceNumber;

        /// <summary>Imageデータ長さ/送信長さ</summary>
        [MarshalAs(UnmanagedType.I2)]
        public ushort dataLength;

        /// <summary>送信分割番号</summary>
        [MarshalAs(UnmanagedType.I2)]
        public ushort splitNumber;

        /// <summary>Imageデータ:分割で受信</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=CBLEConstant.IMAGE_DATA_SIZE)]
        public byte[] imageDataArray;

        // [MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
        // public byte[] test;
    }
}