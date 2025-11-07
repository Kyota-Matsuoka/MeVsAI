using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControll : MonoBehaviour
{

    public float speed = 5f;//移動速度

    public GameObject SpherePrefab;  // 弾のプレハブ
    public float bulletSpeed = 20f;  // 弾の速度
    private Rigidbody rb;
    private Vector3 input;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 回転防止（地面で倒れないように）
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Input.GetMouseButtonDown(0))//左クリックされたら
        {
            Shoot_ver2();

        }
        

    }

    void FixedUpdate()
    {
        input = GetInput_ver2();
        rb.velocity = input;
    }


    Vector3 GetInput()
    {//入力から座標を取得する関数
        float h = Input.GetAxisRaw("Horizontal"); // A / D　左右
        float v = Input.GetAxisRaw("Vertical");   // W / S　前後　-1 ~ 1

        // 入力方向ベクトル
        Vector3 goDir = new Vector3(h, 0f, v).normalized;

        // 現在のy速度（重力など）を維持しつつ、x,zだけ書き換える
        Vector3 newVelocity = goDir * speed;
        newVelocity.y = rb.velocity.y;
        input = newVelocity;
        return input;
    }

    Vector3 GetInput_ver2()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D　左右
        float vertical = Input.GetAxisRaw("Vertical");   // W / S　前後　-1 ~ 1
                                                 
        // カメラの方向からX-Z平面の単位ベクトルを取得
        //Scale(a,b) -> aにbを掛ける -> (1,0,0)*(1,0,1)  Vector3(1,0,1)はカメラが上に向いてたりして、成分が弱くならないように
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        /*
         * cameraForward * vertical -> (1,0,0)に -1 ~ 1を掛ける　-> 前なら 1 後ろなら -1
         * Camera.main.transform.right * horizontal -> (0,0,1)に　-1 ~ 1を掛ける
         */
        Vector3 moveDir = cameraForward * vertical + Camera.main.transform.right * horizontal;

        // 移動処理 newは別にジャンプしないのでいらない
        Vector3 input = moveDir * speed + new Vector3(0, rb.velocity.y, 0);
        return input;
        
    }

    void Shoot()
    {

        // カメラからクリック位置に向けたレイを作成
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        // カメラのレイと同じ方向に向かってCharacterからレイを飛ばす
        Ray ray = new Ray(transform.position, cameraRay.direction);
        //float distance = 10f; // 好きな距離に調整
        //ray.origin + ray.direction * distance;//始点からrayの方向の座標(カメラからのray)

        // プレイヤーの少し前に弾を出す（例: 0.5m前）→座標からrayの方向(1m)*0.5
        Vector3 frontPos = transform.position + ray.direction * 0.5f;

        GameObject bullet = Instantiate(SpherePrefab, frontPos, Quaternion.identity);//Quaternionは無回転を表す

        // 弾に力を加える
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.velocity = ray.direction * bulletSpeed;//ray.direction に向かってbulletSpeedで飛ばす
        //bulletRb.AddForce(ray.direction * bulletSpeed, ForceMode.VelocityChange);//ForceMode VelocityChange質量を無視して、速度を一気に変える

        // 一定時間後に弾を自動削除
        Destroy(bullet, 3f);

    }
    void Shoot_ver2()
    {
        // カメラからクリック位置に向けたレイを作成
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        // カメラのレイと同じ方向に向かってCharacterからレイを飛ばす
        float distance = 100f; // 好きな距離に調整
        Vector3 shootPos = cameraRay.origin + cameraRay.direction * distance;//始点からrayの方向の座標(カメラからのray)
        //ベクトルの作成+normalized
        Vector3 shootDir = (shootPos - transform.position).normalized;
        Ray ray = new Ray(transform.position, shootDir);

        // プレイヤーの少し前に弾を出す（例: 0.5m前）→座標からrayの方向(1m)*0.5
        Vector3 frontPos = transform.position + ray.direction * 0.5f;

        GameObject bullet = Instantiate(SpherePrefab, frontPos, Quaternion.identity);//Quaternionは無回転を表す

        // 弾に力を加える
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.velocity = ray.direction * bulletSpeed;//ray.direction に向かってbulletSpeedで飛ばす
        //bulletRb.AddForce(ray.direction * bulletSpeed, ForceMode.VelocityChange);//ForceMode VelocityChange質量を無視して、速度を一気に変える

        // 一定時間後に弾を自動削除
        Destroy(bullet, 3f);
    }
}
