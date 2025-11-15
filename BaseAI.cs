using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAI : MonoBehaviour
{

    [System.Serializable]//構造体をエディター内で表示するために必要
    public struct Personality
    {
        public float socialDistance;   // 距離の保ち方（例：0.5f〜10f）
        public float shootCoolTime;    // 弾を撃つ頻度（例：秒単位のクールタイム）
        public float intelligence;     // 予測力（0〜1）高いほど先読み行動
        public float cognition;        // 状況把握力（0〜1）高いほど素早く反応
        public float shootCoolTimeMin;
        public float shootCoolTimeMax; //ランダム性がオンのときのクールタイム最大値
        public bool  randomCoolTime;   //弾を撃つ頻度のランダム性 →trueならクールタイムをランダムにする
    }

    public enum State
    {
        Attack,        // 攻撃
        Escape,        // 逃走
        Indifference   // 無関心
    }

    public float speed = 5f;//移動速度
    //PID比率情報
    public float proportal = 1.0f;
    public float integral = 0.0f;
    public float derivative = 0.4f;
    public float proportal_short = 0.4f;
    public float integral_short = 0.0f;
    public float derivative_short = 0.9f;
    public float proportal_middle = 0.9f;
    public float integral_middle = 0.0f;
    public float derivative_middle = 0.6f;
    public float proportal_long = 1.7f;//1.6
    public float integral_long = 0.0f;
    public float derivative_long = 0.3f;//0.5
    //弾丸情報
    public float shootTimer = 0.5f;
    public float shootTimerCheck = 0.5f;
    public float shootPosTimer = 0.45f;
    public float shootPosTimerCheck = 0.5f;//ベクトル更新の間隔
    public float distance_short_middle = 25.0f;//短距離と中距離の基準
    public float distance_middle_long = 50.0f;//中距離と遠距離の基準
    public float distanceCheck = 70.0f;//可弾の基準
    public float bulletSpeed_AI = 20f;  // 弾の速度  →65
    public float bulletSpeed_AI_ver2 = 0.0f;
    //強化学習情報
    public float derWeightMin = 0.3f;//合成微分ベクトルを作るうえで、過去微分ベクトルと直近微分ベクトルの割合の下限(derWeight)
    public float derWeightMax = 0.8f;
    public float propWeightMin = 0.3f;
    public float propWeightMax = 0.8f;
    public float learningRate = 0.1f;
    public float derLearningRateMin = 0.01f;
    public float derLearningRateMax = 0.2f;
    public float propLearningRateMin = 0.01f;//比例ベクトルをどの程度学習させるか→小さいほうが無難
    public float propLearningRateMax = 0.2f;
    public int dataCount = 10;//過去データを蓄積する数
    //その他
    public GameObject player;
    public GameObject SpherePrefab_AI;  // 弾のプレハブ


    // --- AI内部で扱うベクトル類 ---privateだと子クラスで使えない
    protected Vector3 playerPos;
    protected Vector3 prePlayerPos;
    protected Vector3 preVarVector;
    protected Vector3 varVector;          // 前回から今回の座標の変化量ベクトル
    protected Vector3 propVector;         // 比例の座標
    protected Vector3 derVector;
    protected Vector3 integVector;
    protected Vector3 predVector;         // 予測ベクトル
    protected Vector3 errorVector;
    protected Vector3 avgPropVector;
    protected Vector3 avgDerVector;
    protected Vector3 avgVector;
    protected Vector3 shootVector;
    protected Vector3 combinedDerVector;  // 平均微分ベクトルと微分ベクトルの合成ベクトル
    protected Vector3 combinedPropVector;
    protected Vector3 predDerVector;
    protected Vector3 errorDerVector;
    protected Vector3 predPropVector;
    protected Vector3 errorPropVector;

    protected Vector3 toAIVector;
    protected Vector3 rotVector;
    protected Vector3 escapeVector;
    protected Vector3 headForVector;

    protected Vector3 shootPos;
    protected Vector3 shootPosSet;        // PID関数内で呼ぶ
    protected Vector3 toPlayerVector;     // AIからPlayerまでのベクトル

    protected List<Vector3> proportalData;
    protected List<Vector3> derivativeData;


    protected Rigidbody rb;

    //構造体の実体化
    public Personality personality;
    public State       action;


    /*ShootTimer/BulletSpeed_AI/PD_long:PD_middle:PD_shortの上手くいった比率と数値
     * 0.5/65/1.7:0.6/0.9:0.6/0.5:0.8 →初期装備 D_long 1ぐらいあってもいいかも
     * 0.3/85/2.1:0.6/1.3:0.6/未定
     * 
     */

    public abstract void    Behaivor();
    public abstract void    Shoot_AI(Vector3 shootPos);
    public abstract Vector3 PID();
    public abstract Vector3 KeepSocialDistance(Vector3 headForVector);
    public abstract Vector3 ProportalDataAverage(Vector3 newData);
    public abstract Vector3 DerivativeDataAverage(Vector3 newData);
    public abstract Vector3 ReinForceLearning_Ver2(Vector3 varVector, float derFactor);





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
