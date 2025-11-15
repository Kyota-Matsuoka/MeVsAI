using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Bullet_AI : MonoBehaviour
{

    public float existTime = 3f; // 自動削除までの時間
    public int bulletDamage = 1;

    // Start is called before the first frame update
    void Start()
    {
        //Updateで計算しなくても、勝手に計算してくれる
        // Destroy(gameObject, existTime);

#if UNITY_EDITOR//ここはよくわからない、prefab(asset)かinstance(scene上)かどうか
            // エディタ上でPrefabそのものかを判定
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                Debug.Log("これはProject上のPrefabです（実体ではない）");
                return;
            }
#endif

            // 実際にシーン上に存在する場合のみDestroyを実行
            if (gameObject.scene.IsValid())
            {
                //Debug.Log("これはシーン上の実体（Instantiateされたもの）です");
                //Debug.Log(" 数秒後に破壊されます\n");

                Destroy(gameObject, existTime);
            }
            else
            {
                Debug.Log("これはProject上のPrefabです（実体ではない）");

            }



    }
    

    // 衝突判定
    void OnCollisionEnter(Collision collision)
    {

#if UNITY_EDITOR//ここはよくわからない、prefab(asset)かinstance(scene上)かどうか
            // エディタ上でPrefabそのものかを判定
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                Debug.Log("これはProject上のPrefabです（実体ではない）");
                return;
            }
#endif
        if (gameObject.scene.IsValid())//参照元を削除しないように、インスタンス化されたものだけ削除する
        {
            
            //Debug.Log("これはインスタンスです");
            //Debug.Log($"{gameObject}\n");

            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Playerにヒット！");
                // 必要ならPlayer側にダメージ処理呼び出し
                // collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);



                //プレイヤーの体力を減らす
                //ここがNullになっている
                if (GameDirector.Instance != null)
                    GameDirector.Instance.DamagePlayer(bulletDamage);

                Destroy(gameObject); // 弾を消す
            }
            else if (collision.gameObject.CompareTag("Enemy"))//当たったのがAi自身ならそのまま
            {
                //AI自身に生成されたときに感知してDestroyしていた
                return;
            }
            else
            { 
                // 壁や床に当たった場合
                Destroy(gameObject);
            }
        }
    }

}
