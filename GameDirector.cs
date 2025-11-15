using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  


public class GameDirector : MonoBehaviour
{

    //static → 一つしか存在しない　const →　変更できない
    public static GameDirector Instance { get; private set; }
    
    //プレイヤーの体力
    public int playerHP = 10;
    public int maxHP    = 10;
    public Slider hpBar; // UIのHPバーをInspectorで紐づけ
    public GameObject fillImage;


    void Awake()
    {
        if (Instance == null)//一度だけ呼ばれる
        {
            Debug.LogWarning("GameDirectorをインスタンス化します！");
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン切り替えでも破壊されない
        }
        else
        {
            Destroy(gameObject); // 既に存在したら自分を破棄
        }

        if(hpBar != null)//HPBarの初期化
        {
            hpBar.minValue = 0;
            hpBar.maxValue = maxHP;
            hpBar.value = playerHP;
        }
    }


    // ダメージを与える関数 他のスクリプトから呼べるようにpublic
    public void DamagePlayer(int damage)
    {
        playerHP -= damage;
        playerHP = Mathf.Max(playerHP, 0); // 0未満にならないようにする

        Debug.Log($"Playerに{damage}ダメージ！ 残りHP: {playerHP}");

        if (playerHP <= 0)
        {
            playerHP = 0;
            PlayerDead();

            if (fillImage != null)
                fillImage.SetActive(false); // ← HPが0になったら非表示

        }

        //ダメージを受けるとHPバーを更新する
        UpdateHPBar();
    }

    // プレイヤー死亡処理
    private void PlayerDead()
    {
        Debug.Log("Playerが死亡しました！");
        // ゲームオーバー処理など
    }

    void UpdateHPBar()
    {
        //HPバーがある場合、playerHPをHPバーに反映
        if (hpBar != null)
        {
            hpBar.value = playerHP;
        }
    }


}






