using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinemachine : MonoBehaviour
{
    public Transform target;//追従対象の座標
    public Vector3 offset = new Vector3(0, 5, -8);
    public float smoothSpeed = 30f;//5f;→がくがく問題、連射すると解決しない

    void LateUpdate()
    {
        // カメラは LateUpdate（プレイヤーの動きが終わった後）
        Vector3 desiredPosition = target.position + offset;//ターゲットの移動後の座標+オフセット(対象との距離)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime * 10f);//線形補間　目的地に移動するまでの時間→カメラの移動(なめらかにする)
        //transform.LookAt(target);//→ターゲットを注視する
    }



    
}
/* POVを右クリックしたときだけ有効にする
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookOnlyWhenRightMouseDownScoped : MonoBehaviour
{
    public CinemachineFreeLook freeLook;
    public float sensitivity = 1f;
    public bool invertY = false;
    void Awake()
    {
        if (freeLook == null) freeLook = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        if (freeLook == null) return;

        if (Input.GetMouseButton(1))
        {
            // 横回転 (X)
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            freeLook.m_XAxis.Value += mouseX;

            // 垂直 (Y) — FreeLook の m_YAxis は通常 Value が [0,1] の正規化値だが
            // ここでは直接減算/加算で高さを変える簡易手法
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity * (invertY ? 1 : -1);
            freeLook.m_YAxis.Value = Mathf.Clamp01(freeLook.m_YAxis.Value + mouseY * 0.01f);
            // 注意: m_YAxis のスケールや補正は調整が必要（0.01f は例）
        }
        else
        {
            // 何もしない（FreeLook の通常の入力は無効化しておくべき）
            // もし FreeLook が自動で Input を参照するよう設定されている場合は、そちらをオフにするか、
            // こちらで m_XAxis.m_InputAxisValue = 0 などして上書きする実装を追加する
        }
    }
}
*/
