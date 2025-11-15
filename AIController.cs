using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : BaseAI
{

    //PID比率情報
    /*親クラスで定義しているため、コメントアウトして再定義しないようにする
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
    public float derWeightMin    = 0.3f;//合成微分ベクトルを作るうえで、過去微分ベクトルと直近微分ベクトルの割合の下限(derWeight)
    public float derWeightMax    = 0.8f;
    public float propWeightMin   = 0.3f;
    public float propWeightMax   = 0.8f;
    public float learningRate    = 0.1f;
    public float derLearningRateMin = 0.01f;
    public float derLearningRateMax = 0.2f;
    public float propLearningRateMin = 0.01f;//比例ベクトルをどの程度学習させるか→小さいほうが無難
    public float propLearningRateMax = 0.2f;
    public int dataCount = 10;//過去データを蓄積する数
    //その他
    public GameObject player;
    public GameObject SpherePrefab_AI;  // 弾のプレハブ


    private Vector3 playerPos;
    private Vector3 prePlayerPos;
    private Vector3 preVarVector;
    private Vector3 varVector;//前回から今回の座標の変化量ベクトル
    private Vector3 propVector;//比例の座標
    private Vector3 derVector;
    private Vector3 integVector;
    private Vector3 predVector;//予測ベクトル
    private Vector3 errorVector;
    private Vector3 avgPropVector;
    private Vector3 avgDerVector;
    private Vector3 avgVector;
    private Vector3 shootVector;
    private Vector3 combinedDerVector;//平均微分ベクトルと微分ベクトルの合成ベクトル
    private Vector3 combinedPropVector;
    private Vector3 predDerVector;
    private Vector3 errorDerVector;
    private Vector3 predPropVector;
    private Vector3 errorPropVector;

    private Vector3 shootPos;
    private Vector3 shootPosSet;//PID関数内で呼ぶ
    private Vector3 toPlayerVector;//AiからPlayerまでのベクトル
    private List<Vector3> proportalData;
    private List<Vector3> derivativeData;

    */

    /*ShootTimer/BulletSpeed_AI/PD_long:PD_middle:PD_shortの上手くいった比率と数値
     * 0.5/65/1.7:0.6/0.9:0.6/0.5:0.8 →初期装備 D_long 1ぐらいあってもいいかも
     * 0.3/85/2.1:0.6/1.3:0.6/未定
     * 
     

    public struct Personality
    {
        public float socialDistance;   // 距離の保ち方（例：0.5f〜10f）
        public float shootCoolTime;    // 弾を撃つ頻度（例：秒単位のクールタイム）
        public float intelligence;     // 予測力（0〜1）高いほど先読み行動
        public float cognition;        // 状況把握力（0〜1）高いほど素早く反応
    }

    public enum State
    {
        Attack,        // 攻撃
        Escape,        // 逃走
        Indifference   // 無関心
    }
    */
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 回転防止（地面で倒れないように）
    }
    // Start is called before the first frame update
    void Start()
    {
        playerPos = player.transform.position;
        preVarVector = Vector3.zero;
        varVector = Vector3.zero;
        prePlayerPos = Vector3.zero;
        propVector = Vector3.zero;
        derVector = Vector3.zero;
        integVector = Vector3.zero;
        predVector = Vector3.zero;
        errorVector = Vector3.zero;
        avgPropVector = Vector3.zero;
        avgDerVector = Vector3.zero;
        avgVector = Vector3.zero;
        shootVector = Vector3.zero;
        combinedDerVector = Vector3.zero;
        combinedPropVector = Vector3.zero;
        toPlayerVector = Vector3.zero;
        shootPos = Vector3.zero;
        shootPosSet = Vector3.zero;
        predDerVector = Vector3.zero;
        errorDerVector = Vector3.zero;
        predPropVector = Vector3.zero;
        errorPropVector = Vector3.zero;

        toAIVector = Vector3.zero;
        rotVector = Vector3.zero;
        escapeVector = Vector3.zero;
        headForVector = Vector3.zero;

        proportalData = new List<Vector3>();
        derivativeData = new List<Vector3>();

        //構造体の初期化
        personality = new Personality
        {
            //陰湿
            socialDistance   = 15f,  // 距離の保ち方（例：0.5f〜10f）
            shootCoolTime    = 0.5f,    // 弾を撃つ頻度（例：秒単位のクールタイム）
            intelligence     = 1f,    // 予測力（0〜1）高いほど先読み行動  
            cognition        = 0.7f, //判断力
            shootCoolTimeMin = 0.3f,
            shootCoolTimeMax = 0.9f,
            randomCoolTime   = true,

        };


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        //ベクトル更新　→　弾を撃つ
        //ベクトル更新のクールタイム
        shootPosTimer += Time.deltaTime;
        if(shootPosTimer >= shootPosTimerCheck)
        {
            shootPosTimer = 0f;
            shootPos = PID();
        } →これはやめといたほうがいい
        */
        //弾を打つクールタイム
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootTimerCheck)
        {//shootTimerCheckを下げると、うまく追従できなくなる。厳密には、追いつかない →　なぜ　→　短すぎて、うまく予測できなくなっている？　→　弾を撃つ速度だけ、下げる
            shootTimer = 0f;
            shootPos = PID();
            
            Shoot_AI(shootPos);
            //AIの移動ベクトルの基となるPlayerの予測ベクトルはここで更新する
            headForVector = shootVector;
            //追加
            escapeVector = KeepSocialDistance(headForVector);
            rb.velocity = escapeVector.normalized * speed;

        }



        /*色々試したみたところ、TimerとBulletSpeedは同時に調節する必要がある
         * また、Timerを大きくすると、速すぎる＋予測が過剰評価になる　→　bulletSpeedで調節が必要
         * もしくは、PIDの数値を変える
         * いや、全部変えた方がいい
         * 
         * 
         */
       
    }

    public override void Behaivor()//移動　攻撃　判断
    {
        /*
         * 逃げる時もPID使ってみるか？　
         * 判断は移動と攻撃を求めてから、いや逆か、判断して、移動と攻撃だな。逃げるべきか、追うべきか、それから実行かな。
         * となると、関数は分けたほうが良いか。
         */
        if (personality.randomCoolTime == true)
        {
            personality.shootCoolTime = Random.Range(personality.shootCoolTimeMin, personality.shootCoolTimeMax);
            shootTimer = personality.shootCoolTime;


        }
    }

    public override Vector3 KeepSocialDistance(Vector3 headForVector) 
    {//shootVectorは次の相手の予測位置　ここは一旦保留 PID内でこの関数を呼ぶのも手 shootVector OR varVectorで迷っている
   
        //1.フィールド上　→追跡or逃走の2パターン

        //playerとの距離とベクトルの角度は比例する   →toAIVectorが基本・shootVectorが予測
        toAIVector  = transform.position - player.transform.position;

        /*
        //ベクトル同士の相違度を調べる   相違度から方向ベクトルを算出することはできない
        float dot   = Vector3.Dot(shootVector.normalized, toAIVector.normalized);//AIまでのベクトルとPlayerの予測ベクトルの内積
        //dotProportion :dotをどの程度ベクトル作成に反映するかどうか   ↓以下の式は　AIVector.magnitude > socialDistanceであることが条件
        float dotProportion = Mathf.Pow(1f / socialDistance, 2) * Mathf.Pow(toAIVector.magnitude - socialDistance, 2);//dotProportion = ((1/socialDistance)**2)  *  (AIVector.magnitude - socialDistance) ** 2)  
        float reDot = dot * dotProportion;//相違度が大きい/距離が近いほど1 or -1に近くなる
        reDot = -reDot;//反転//(reDot < 0f) ? -reDot : reDot;//0以下ならプラスに変換
        */

        float rotProportion = Mathf.Pow(1f / personality.socialDistance, 2) * Mathf.Pow(toAIVector.magnitude - personality.socialDistance, 2);
        rotProportion = Mathf.Clamp01(rotProportion); // 安全のため0〜1に制限

        // 内積から角度を求める）
        float dot = Vector3.Dot(toAIVector.normalized, headForVector.normalized);
        dot = Mathf.Clamp(dot, -1f, 1f); // 浮動小数の誤差対策
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        // Quaternionを使って、toAIVecotrを線対称にshootVectorの対称方向への方向単位ベクトル(rotVector)の作成
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 rotVector = rot * toAIVector.normalized;
        rotVector = rotVector * headForVector.magnitude;//作成したrotatedVector(方向単位ベクトル)を元々の大きさにする
        


        escapeVector = (toAIVector * (1 - rotProportion)) + (rotVector * (rotProportion));//全体の割合を１

        //2.フィールド端
        /*
        if () {//フィールド端


        }
        */
        //3.フィールドの四隅

        if(toAIVector.magnitude > 20)
            return Vector3.zero;
        else
            return escapeVector;
    }

    public override Vector3 PID()
    {
        //更新処理

        //preVarVectorはvarVectorを更新する前に作成する
        preVarVector = varVector;//更新
        prePlayerPos = playerPos;//更新
        playerPos = player.transform.position;//-> ゲームオブジェクトの座標
        //varVectorの更新は最後
        varVector = (playerPos - prePlayerPos);//更新：playerPos - prePlayerPos -> 変化量ベクトル


        propVector = varVector;//比例計算後の座標
        derVector = (varVector - preVarVector);//微分ベクトル

        /*ReinForceLeaning内で行うことにする
        //ここで過去データの更新・平均の取得
        avgPropVector = ProportalDataAverage(propVector);
        avgDerVector　= DerivativeDataAverage(derVector);
        avgDerVector  = avgPropVector + avgDerVector;
        */




        //微分処理

        /*distanceCheck(AIからPlayerの距離に応じてPIDの比率を変更する)
         * 
         *近いほど比例に対して微分の比率を大きくする、遠いほど比例に対して微分の比率を小さくする
         *また逆方向になった時の微分を0に近くする
         *近距離 P:0.8/D:0.7 計:1.5
         *中距離 P:1.5/D:0.5 計:2.0
         *遠距離 P:2.3/D:0.2 計:2.5
         *
         **/
        toPlayerVector = playerPos - transform.position;

        if (toPlayerVector.magnitude > distanceCheck)//可弾かどうか
        {
            //Debug.Log("撃つのやーめた\n");
            shootPosSet = Vector3.zero;
            return shootPosSet;
        }

        // --- 逆方向判定と微分抑制 ---  :逆方向になった時、derivativeを抑える処理
        float dot = Vector3.Dot(varVector.normalized, preVarVector.normalized);//単位化(normalized)→dot(cosθ:（1:同方向、-1:逆方向）)を作成

        float derFactor = 1f; // 通常はそのまま
        if (dot < 0) // 逆方向の場合だけ
        {
            // 逆向きほど小さくする：1 + dot … 逆向き（dot<0）のときに0〜1に落とし込む変換 　/   Clamp01() … 負の値や1以上を丸めて安全にする
            // → dot=-1 → 0, dot=0 → 1, dot=1 → 2（max1にclampされるので問題なし） 180度で 0,   90度で 1, 0度で 1
            derFactor = Mathf.Clamp01(1f + dot);
        }
        //Debug.Log($"微分係数:{derFactor}\n");


        //PID比率処理


        //(1.7 - 0.9) = a * (60 - 40) -> age
        //1.7 = a * 60 + b            -> century
        //比例(P)を求める計算式
        float age = 0.0f;
        float century = 0.0f;

        if (toPlayerVector.magnitude > distance_middle_long)//遠距離
        {
            age = (proportal_long - proportal_middle) / (distanceCheck - distance_middle_long);
            century = proportal_long - age * distanceCheck;
            proportal = age * toPlayerVector.magnitude + century;
            //微分
            derivative = derivative_long;
            bulletSpeed_AI_ver2 = bulletSpeed_AI;
        }
        else if (toPlayerVector.magnitude > distance_short_middle)//中距離
        {
            age = (proportal_middle - proportal_short) / (distance_middle_long - distance_short_middle);
            century = proportal_middle - age * distance_middle_long;
            proportal = age * toPlayerVector.magnitude + century;

            derivative = derivative_middle;
            bulletSpeed_AI_ver2 = bulletSpeed_AI;
        }
        else
        {
            //Debug.Log("近距離攻撃に移行します！\n");
            age = (proportal_short - 0) / (distance_short_middle - 0);
            century = proportal_short - age * distance_short_middle;
            proportal = age * toPlayerVector.magnitude + century;
            //proportal = proportal_short;
            derivative = derivative_short;
            bulletSpeed_AI_ver2 = bulletSpeed_AI - 30;
        }



        //Debug.Log($"比例:{proportal}\n");
   
        shootVector = Vector3.zero;//shootVectorというベクトルで整理する→ワールド指定なので、FixedUpdate内と共通変数になっている　→つまり、FixedUpdate内からも使える
        shootVector = (propVector * proportal) + (derVector * derivative * derFactor) + (integVector * integral);

        //微分係数が確定したので、avgVectorを作成
        avgVector = (propVector * proportal) + (derVector * derivative * derFactor) + (integVector * integral);
        
        //強化学習
        predDerVector = ReinForceLearning_Ver2(varVector, derFactor);
        shootVector = (propVector * proportal) + (predDerVector * derivative * derFactor) + (integVector * integral);



        //最終
        shootPosSet = playerPos + shootVector;

        /*

        if (toPlayerVector.magnitude > distance_middle_long)//一定の距離離れている場合
        {
            Debug.Log("遠距離攻撃に移行します！\n");   
            shootPosSet = playerPos + (propVector * proportal_long) + (derVector * derivative_long * derFactor) + (integVector * integral_long);
        }
        else if(toPlayerVector.magnitude > distance_short_middle)
        {
            Debug.Log("中距離攻撃に移行します！\n");

            shootPosSet = playerPos + (propVector * proportal_middle) + (derVector * derivative_middle * derFactor) + (integVector * integral_middle);


        }
        else 
        {
            //近距離だったらplayerPosにそのまま打つぐらいでもあたりそうだな
            Debug.Log("近距離攻撃に移行します！\n");

            
            shootPosSet = playerPos + (propVector * proportal_short) + (derVector * derivative_short * derFactor) + (integVector * integral_short);
            //shootPosSet = playerPos + (propVector * proportal) + (derVector * derivative) + (integVector * integral);
        }
        //*/

        return shootPosSet;
    }

    public override void Shoot_AI(Vector3 shootPos)
    {
        //Debug.Log(SpherePrefab_AI);
        if (shootPos != Vector3.zero)
        {
            if (SpherePrefab_AI != null)
            {
                //ベクトルの作成+normalized
                Vector3 origin = transform.position;
                Vector3 shootDir_AI = (shootPos - origin).normalized;
                Ray ray = new Ray(origin, shootDir_AI);

                /*
                 * SpherePrafab_AIにDestroy関数を入れると、参照元自体を破壊することになる
                 * →おそらく、インスタンス化されたものにのみDestroy関数を適用しないといけない
                */
                GameObject bullet_AI = Instantiate(SpherePrefab_AI, origin, Quaternion.identity);//Quaternionは無回転を表す
                
                // 弾に力を加える
                Rigidbody bulletRb = bullet_AI.GetComponent<Rigidbody>();
                bulletRb.velocity = shootDir_AI * bulletSpeed_AI_ver2;
                Debug.Log($"velocity:{bulletRb.velocity}\n");
                //Debug.Log($"ShootPos: {shootPos}\n");
                Debug.Log($"bullet_AI:{bullet_AI}\n");
                //Debug.Log($"bullet_AI_pos:{bullet_AI.transform.position}\n");
                //bullet_AIも通ってる

                //ray.direction に向かってbulletSpeedで飛ばす
                //bulletRb.AddForce(ray.direction * bulletSpeed, ForceMode.VelocityChange);//ForceMode VelocityChange質量を無視して、速度を一気に変える





                // 一定時間後に弾を自動削除
                //Destroy(bullet_AI, 3f);
                //Debug.Log($"bulletSpeed:{bulletSpeed_AI_ver2}\n");
                /*
                 * Rayの表示
                */
                //自分の位置からshootPosに向けてRayを飛ばす
                float maxDistance = 100f; // レイの距離
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        //Debug.Log($"Playerにヒット: {hit.collider.name}");
                    }
                }

                //デバッグ可視化（Sceneビューで確認）
                Debug.DrawRay(origin, shootDir_AI * maxDistance, Color.red, 0.5f);
            }
            else
            {
                Debug.LogWarning("SpherePrefab_AI がアタッチされていません！");
            }

        }
        
    }

    //過去データの更新・整理・平均の取得
    public override Vector3 ProportalDataAverage(Vector3 newData)
    {
        proportalData.Add(newData);
        if (proportalData.Count > dataCount)
        {
            proportalData.RemoveAt(0); // 一番古いデータを削除
        }

        if (proportalData.Count == 0)
            return Vector3.zero;

        Vector3 proportalSum = Vector3.zero;
        foreach (var v in proportalData)
        {
            proportalSum += v;
        }
        return proportalSum / proportalData.Count;
    }

    public override Vector3 DerivativeDataAverage(Vector3 newData)
    {
        derivativeData.Add(newData);
        if (derivativeData.Count > dataCount)
        {
            derivativeData.RemoveAt(0); // 一番古いデータを削除
        }

        //データの平均
        if (derivativeData.Count == 0)
            return Vector3.zero;

        Vector3 derivativeSum = Vector3.zero;
        foreach (var v in derivativeData)
        {
            derivativeSum += v;
        }
        return derivativeSum / derivativeData.Count;
    }


    Vector3 ReinForceLearning(Vector3 varVector, float derFactor)//強化学習
    {   //1.前回の予測ベクトルの精査　→ 2.学習 → 3.予測ベクトルの作成


        //0.平均データベクトルの更新
        //ここで過去データの更新・平均の取得
        avgPropVector = ProportalDataAverage(propVector);
        avgDerVector = DerivativeDataAverage(derVector);
        avgDerVector = avgPropVector + avgDerVector;

        //1.前回の予測精度の精査(前回のベクトルと実際の変化ベクトルの比較)


        //①微分予測ベクトルの誤差
        errorDerVector = derVector - predDerVector;

        //②比例予測ベクトルの誤差
        errorPropVector = propVector - predPropVector; 

        //③前回の変化量ベクトルと予測ベクトルの誤差   
        errorVector = varVector - predVector; //予測と結果の誤差ベクトル

        //2.学習

        //①重み学習(derWeight/propWeight)　→んー、しっくりこない
        /*
        // 誤差が大きいなら、現在のデータを重視する方向に学習→これは場所が良くないな
        derWeight = Mathf.Clamp01(derWeight + learningRate * errorDerVector.magnitude);
        propWeight = Mathf.Clamp01(propWeight + learningRate * errorPropVector.magnitude);
        */
        
        // ② 強化学習(誤差の大きさから学習率を修正)
        float derLearningRate = Mathf.Clamp(errorDerVector.magnitude * 0.1f, derLearningRateMin, derLearningRateMax);
        float propLearningRate = Mathf.Clamp(errorPropVector.magnitude * 0.1f, propLearningRateMin, propLearningRateMax);

        // ③ 報酬ベクトル（誤差の逆方向）
        //ここは逆方向ベクトルを作成することで、修正する。もし同じ方向だと、誤差の方向に誤差を加算して、より誤差が大きくなってしまう。
        Vector3 rewardDerVector  = -errorDerVector.normalized * errorDerVector.magnitude;
        Vector3 rewardPropVector = -errorPropVector.normalized * errorPropVector.magnitude;

        // ④ 誤差補正(平均ベクトルを更新) →combinedVectorに補正を掛けたほうがいい気がする
        avgDerVector  += rewardDerVector * derLearningRate;
        avgPropVector += rewardPropVector * propLearningRate;




        //平均微分ベクトルと直近微分ベクトルの合成
        // 変化の激しさを0〜1で表す(過去の変化量の平均と現在の変化量の差)　→大きいほど今回の変化量が平均から離れている　→derWeightMaxに近い値を返す　→derWeightを直接掛けているderVectorが大きくなる
        float derChangeRate = Mathf.Clamp01((derVector - avgDerVector).magnitude * 0.1f);
        // 動きが激しいほど現在を重視(0.3f:derWeightMin)：動きが緩やかなほど過去データからの予測を重視(0.8f)
        float derWeight = Mathf.Lerp(derWeightMin, derWeightMax, derChangeRate);//0.3と0.8は変数にしよう

        //平均比例ベクトルと直近比例ベクトルの合成
        float propChangeRate = Mathf.Clamp01((propVector - avgPropVector).magnitude * 0.1f);
        float propWeight = Mathf.Lerp(propWeightMin, propWeightMax, propChangeRate);




        //3.予測ベクトルの作成  :微分ベクトルだけ適用するか迷っている

        //①微分ベクトルの作成
        combinedDerVector = (avgDerVector * (1 - derWeight)) + (derVector * derWeight);
        predDerVector  = combinedDerVector;
        //②比例ベクトルの作成
        combinedPropVector = (avgPropVector * (1 - propWeight)) + (propVector * propWeight);
        predPropVector  = combinedPropVector;
        //③予測ベクトルの作成
        predVector 　= (combinedPropVector * proportal) + (combinedDerVector * derivative * derFactor) + (integVector * integral);


        // 可視化の原点（例えばAIの現在位置）
        Vector3 origin = transform.position;

        // 実際の変化ベクトル（赤）
        Debug.DrawRay(origin, derVector * 5f, Color.red, 0.1f);

        // 学習済み平均ベクトル（緑）
        Debug.DrawRay(origin, avgDerVector * 5f, Color.green, 0.1f);

        // 現在の予測ベクトル（青）
        Debug.DrawRay(origin, predDerVector * 5f, Color.blue, 0.1f);

        // エラー（誤差）を紫で可視化
        Debug.DrawRay(origin, errorDerVector * 5f, Color.magenta, 0.1f);

        // デバッグログで数値確認（小数第2位）
        Debug.Log(
            $"[Derivatives]\n" +
            $"derVector: {derVector.ToString("F2")}\n" +
            $"avgDerVector: {avgDerVector.ToString("F2")}\n" +
            $"predDerVector: {predDerVector.ToString("F2")}\n" +
            $"errorDerVector: {errorDerVector.ToString("F2")}\n"+
            $"derWeight:{derWeight.ToString("F2")}\n" 
        );


        return predDerVector;//一旦、比例を無視した、微分のみの戻り値
    }


    public override Vector3 ReinForceLearning_Ver2(Vector3 varVector, float derFactor)//強化学習
    {   //1.前回の予測ベクトルの精査　→ 2.学習 → 3.予測ベクトルの作成 →4.最終学習

        //0.平均データベクトルの更新
        //ここで過去データの更新・平均の取得
        avgPropVector = ProportalDataAverage(propVector);
        avgDerVector = DerivativeDataAverage(derVector);
        avgDerVector = avgPropVector + avgDerVector;

        //1.前回の予測精度の精査
        //①微分予測ベクトルの誤差
        errorDerVector = derVector - predDerVector;
        //②比例予測ベクトルの誤差
        errorPropVector = propVector - predPropVector;
        //③前回の変化量ベクトルと予測ベクトルの誤差(PID) 
        errorVector = varVector - predVector; //予測と結果の誤差ベクトル


        //2-1.学習 強化学習方式(データが多くないと精度が低い：300個くらいはないと頷けない)
        /*
        //①学習率学習(誤差の大きさから学習率を修正)
        //ここはあまり効果なさそう
        float derLearningRate = Mathf.Clamp(errorDerVector.magnitude * 0.3f, derLearningRateMin, derLearningRateMax);
        float propLearningRate = Mathf.Clamp(errorPropVector.magnitude * 0.1f, propLearningRateMin, propLearningRateMax);

        //②報酬ベクトルの作成（誤差の逆方向）
        //ここは逆方向ベクトルを作成することで、修正する。もし同じ方向だと、誤差の方向に誤差を加算して、より誤差が大きくなってしまう。
        Vector3 rewardDerVector = -errorDerVector.normalized * errorDerVector.magnitude;
        Vector3 rewardPropVector = -errorPropVector.normalized * errorPropVector.magnitude;

        //③誤差補正(平均ベクトルを更新) →combinedVectorに補正を掛けたほうがいい気がする
        float learningRateFix = 0.5f;//予測ベクトルよりも平均ベクトルの修正のほうを抑える
        avgDerVector += rewardDerVector * derLearningRate * learningRateFix;
        avgPropVector += rewardPropVector * propLearningRate * learningRateFix;
        */

        //2-2.学習　EMA方式(単純構造：変化に強い・強化学習ではない)
        avgDerVector = Vector3.Lerp(avgDerVector, derVector, 0.1f);
        avgPropVector = Vector3.Lerp(avgPropVector, derVector, 0.1f);







        //3.予測ベクトルの作成  :微分ベクトルだけ適用するか迷っている
        /*
        //平均微分ベクトルと直近微分ベクトルの合成(D)
        // 変化の激しさを0〜1で表す(過去の変化量の平均と現在の変化量の差)　→変化が大きいほど今回の変化量が平均から離れている　→derWeightMaxに近い値を返す　→derWeightを直接掛けているderVectorが大きくなる
        float derChangeRate = Mathf.Clamp01((derVector - avgDerVector).magnitude * 0.1f);
        // 動きが激しいほど現在を重視(0.3f:derWeightMin)：動きが緩やかなほど過去データからの予測を重視(0.8f)
        float derWeight = Mathf.Lerp(derWeightMin, derWeightMax, derChangeRate);//0.3と0.8は変数にしよう

        //平均比例ベクトルと直近比例ベクトルの合成(P)
        float propChangeRate = Mathf.Clamp01((propVector - avgPropVector).magnitude * 0.1f);
        float propWeight = Mathf.Lerp(propWeightMin, propWeightMax, propChangeRate);


        //①微分ベクトルの作成
        combinedDerVector = (avgDerVector * (1 - derWeight)) + (derVector * derWeight);
        predDerVector = combinedDerVector;
        //②比例ベクトルの作成
        combinedPropVector = (avgPropVector * (1 - propWeight)) + (propVector * propWeight);
        predPropVector = combinedPropVector;
        //③予測ベクトルの作成
        predVector = (predPropVector * proportal) + (predDerVector * derivative * derFactor) + (integVector * integral);
        */

        //一度、平均ベクトルとの合成ではなく一個前の予測ベクトルとの合成にしてみる

        //平均微分ベクトルと予測ベクトルの合成(D)
        // 変化の激しさを0〜1で表す(過去の変化量の平均と現在の変化量の差)　→変化が大きいほど今回の変化量が平均から離れている　→derWeightMaxに近い値を返す　→derWeightを直接掛けているderVectorが大きくなる
        float derChangeRate = Mathf.Clamp01((derVector - predDerVector).magnitude * 0.1f);
        // 動きが激しいほど現在を重視(0.3f:derWeightMin)：動きが緩やかなほど過去データからの予測を重視(0.8f)
        float derWeight = Mathf.Lerp(derWeightMin, derWeightMax, derChangeRate);//0.3と0.8は変数にしよう

        //平均比例ベクトルと直近比例ベクトルの合成(P)
        float propChangeRate = Mathf.Clamp01((propVector - predPropVector).magnitude * 0.1f);
        float propWeight = Mathf.Lerp(propWeightMin, propWeightMax, propChangeRate);

        //①微分ベクトルの作成
        combinedDerVector = (predDerVector * (1 - derWeight)) + (derVector * derWeight);
        predDerVector = combinedDerVector;
        //②比例ベクトルの作成
        combinedPropVector = (predPropVector * (1 - propWeight)) + (propVector * propWeight);
        predPropVector = combinedPropVector;
        //③予測ベクトルの作成
        predVector = (predPropVector * proportal) + (predDerVector * derivative * derFactor) + (integVector * integral);


        //最終学習修正(オリジナルよりも修正強め) Ver2追加
        //誤差補正(平均ベクトルを更新)
        //んー予測ベクトルを修正しても、一抹なだけ
        //平均と予測の両方を修正することにする
        /*
        //微分
        predDerVector += rewardDerVector * derLearningRate;
        //比例
        predPropVector += rewardPropVector * propLearningRate;
        //PID作成
        predVector = (predPropVector * proportal) + (predDerVector * derivative * derFactor) + (integVector * integral);
        */

        //デバッグ
        // 可視化の原点（例えばAIの現在位置）
        Vector3 origin = transform.position;

        // 実際の変化ベクトル（赤）
        Debug.DrawRay(origin, derVector * 5f, Color.red, 0.3f);

        // 学習済み平均ベクトル（緑）
        Debug.DrawRay(origin, avgDerVector * 5f, Color.green, 0.3f);

        // 現在の予測ベクトル（青）
        Debug.DrawRay(origin, predDerVector * 5f, Color.blue, 0.3f);

        // エラー（誤差）を紫で可視化
        Debug.DrawRay(origin, errorDerVector * 5f, Color.magenta, 0.3f);

        // デバッグログで数値確認（小数第2位）
        /*
        Debug.Log(
            $"[Derivatives]\n" +
            $"derVector: {derVector.ToString("F2")}\n" +
            $"avgDerVector: {avgDerVector.ToString("F2")}\n" +
            $"predDerVector: {predDerVector.ToString("F2")}\n" +
            $"errorDerVector: {errorDerVector.ToString("F2")}\n" +
            $"derWeight:{derWeight.ToString("F2")}\n"
        );
        */


        return predDerVector;//一旦、比例を無視した、微分のみの戻り値
    }



}
