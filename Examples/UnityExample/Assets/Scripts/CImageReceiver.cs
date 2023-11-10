using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.InteropServices;

namespace BLEService {

    /// <summary>
    /// 受信フェーズ
    /// </summary>
    // [Flags]
    enum ReceivePhase{

        /// <summary> 初期状態</summary>
        NATURAL = 0x00,
        /// <summary> 開始</summary>
        START   = 0x01,
        /// <summary> データ受信</summary>
        DATA   = 0x10,
        /// <summary> 終了：終了コマンドを受け取るとNATURALに遷移する</summary>
        END   = 0xff,
    }

    /// <summary>
    /// 画像データ受信クラス
    /// </summary>
    public class CImageReceiver
    {
        /// <summary> 受信フェーズ</summary>
        private ReceivePhase _phase;

        /// <summary> シーケンス番号</summary>
        private uint _sequenceNumber;

        /// <summary> Imageデータバッファ（ローカル）</summary>
        private byte[] _imageDataBuffer;
        /// <summary> Imageデータバッファ格納オフセット</summary>
        private uint _imageDataBufferOffset;


        private bool _debugFlag = true;

        /// <summary> Imageデータバッファプロパティ（取得専用）</summary>
        public byte[] ImageDataBuffer {
            get {
                byte[] buffer = this._imageDataBuffer;
                // Imageデータバッファ解放
                this._imageDataBuffer = null;
                return buffer;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CImageReceiver()
        {
            this.resetPhase();
        }


        /// <summary>
        /// コマンド受信
        /// </summary>
        /// <param name="command">BLEより受信したバイト配列</param>
        /// <returns>true:正常受信 / false:異常受信</returns>
        public bool commandReceive(byte[] command,  Action<byte[], ushort> onCompleteImageRecv) {
            bool stat = false;

            // 受信したImageコマンドデータ
            ImageCommandData imgData = this.generateImageCommandData(command);

            switch(imgData.command) {
                case CBLEConstant.CMD_STRING_NATURAL:
                //  初期状態：通常はありえない
                    break;
                case CBLEConstant.CMD_STRING_START:
                //  開始
                    if (this._phase != ReceivePhase.NATURAL) {
                        this.debugPrint(
                            String.Format("recv phase START but now phase =>{0}", this._phase));
                        break;
                    }
                    // シーケンス番号初期化
                    this._sequenceNumber = imgData.sequenceNumber;
                    // Imageデータバッファのバイトデータ確保
                    this._imageDataBuffer = new byte[imgData.dataLength];
                    this._imageDataBufferOffset = 0;

                    // フェーズ更新
                    this._phase = ReceivePhase.DATA;

                    break;
                case CBLEConstant.CMD_STRING_DATA:
                //  データ受信
                    if ( (this._phase != ReceivePhase.DATA) || (this._sequenceNumber != imgData.sequenceNumber) ) {
                        this.debugPrint(
                            String.Format("recv phase DATA but now phase =>{0} or sequenceNumber is unmatch {1}/{2}",
                                this._phase, this._sequenceNumber, imgData.sequenceNumber));
                        break;
                    }

                    // Imageデータバッファを更新
                    Buffer.BlockCopy(
                        imgData.imageDataArray, (int)0,
                        this._imageDataBuffer, (int)this._imageDataBufferOffset,
                        (int)(imgData.dataLength) & 0xffff
                    );
                    this._imageDataBufferOffset += imgData.dataLength;

                    // 分割が最後の場合はフェーズ更新
                    if (imgData.splitNumber == CBLEConstant.SPLIT_END_NUM) {
                        this._phase = ReceivePhase.END;
                    }

                    break;
                case CBLEConstant.CMD_STRING_END:
                //  終了
                    if ( (this._phase != ReceivePhase.END) || (this._sequenceNumber != imgData.sequenceNumber) ) {
                        this.debugPrint(
                            String.Format("recv phase END but now phase =>{0} or sequenceNumber is unmatch {1}/{2}",
                                this._phase, this._sequenceNumber, imgData.sequenceNumber));
                        break;
                    }
                    // Imageデータ受信完了のコールバック
                    ushort dataLen = (ushort)(this._imageDataBuffer.Length);
                    onCompleteImageRecv(this.ImageDataBuffer, dataLen);

                    // Imageデータバッファ解放
                    // this._imageDataBuffer =  null; => ImageDataBuffer getterで実施

                    // フェーズ更新
                    this.resetPhase();

                    break;
                default:
                    break;
            }

            return (stat);
        }

        /// <summary>
        /// フェーズの初期化
        /// </summary>
        private void resetPhase() {
            this._phase = ReceivePhase.NATURAL;
        }

        /// <summary>
        /// Imageコマンドデータの生成
        /// </summary>
        /// <param name="command">BLEより受信したバイト配列</param>
        /// <returns>生成されたImageコマンドデータ</returns>
        private ImageCommandData generateImageCommandData(byte[] command)  {
            ImageCommandData imgData = new ImageCommandData();
            int size = Marshal.SizeOf(imgData);
            System.IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(command, 0, ptr, size);
            imgData = (ImageCommandData)Marshal.PtrToStructure(ptr, typeof(ImageCommandData));
            Marshal.FreeHGlobal(ptr);

            return imgData;

        }

        private void debugPrint(string message) {
            if (this._debugFlag != true) {
                return;
            }

            Debug.Log(message);
        }


    }
}