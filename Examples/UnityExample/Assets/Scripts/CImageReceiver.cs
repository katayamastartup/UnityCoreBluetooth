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
        /// <summary>
        /// 受信フェーズ
        /// </summary>
        private ReceivePhase _phase;

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
        public bool commandReceive(byte[] command) {
            bool stat = false;

            ImageCommandData imgData = new ImageCommandData();
            int size = Marshal.SizeOf(imgData);
            System.IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(command, 0, ptr, size);
            imgData = (ImageCommandData)Marshal.PtrToStructure(ptr, typeof(ImageCommandData));
            Marshal.FreeHGlobal(ptr);

            return (stat);
        }

        /// <summary>
        /// フェーズの初期化
        /// </summary>
        private void resetPhase() {
            this._phase = ReceivePhase.NATURAL;
        }


    }
}