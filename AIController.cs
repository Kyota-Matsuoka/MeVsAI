using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : BaseAI
{

    /*
    衝突判定本当に注意する!
    AIとAIbulletの衝突をオフにするとか、タグを変えるとか
    */



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        isGroundedCheck = GetComponentInChildren<IsGroundedCheck>();//コンポーネントが子オブジェクトの場合の取得方法
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
        toCenterVector = Vector3.zero;

        proportalData = new List<Vector3>();
        derivativeData = new List<Vector3>();

        isGrounded = true;
        isOutOfArea = true;
        isGoBacked = false;// true;

        //クラスの初期化 ここはデータが揃ってから行う  →変更したい場合は、ここをコメントアウト&inspectorから変更
        fierce = new Personality
        {
            speed = 5,
            socialDistance = 3f,
            emergencyDistance = 1f,
            shootCoolTime = 0.5f,
            intelligence = 0.7f,
            cognition = 0.6f,
            randomCoolTime = false,
            moveTimerCheck = 0.5f,
            distanceCheck = 70.0f,//可弾の基準
            bulletSpeed_AI = 60f, // 弾の速度  →65
            bulletSpeed_AI_near = 30.0f,
        };//実体をここで初期化しておいて、切り替わったらpersonalityに代入
        timid = new Personality
        {
            speed = 5f,
            socialDistance = 8f,
            emergencyDistance = 5f,
            shootCoolTime = 1.5f,
            intelligence = 0.4f,
            cognition = 0.8f,
            randomCoolTime = true,
            moveTimerCheck = 0.5f,
            distanceCheck = 70.0f,//可弾の基準
            bulletSpeed_AI = 60f, // 弾の速度  →65
            bulletSpeed_AI_near = 30.0f,
        };
        cunning = new Personality
        {
            speed = 5f,
            socialDistance = 5f,
            emergencyDistance = 2f,
            shootCoolTime = 1f,
            intelligence = 0.9f,
            cognition = 0.9f,
            randomCoolTime = true,
            moveTimerCheck = 0.5f,
            distanceCheck = 70.0f,//可弾の基準
            bulletSpeed_AI = 60f, // 弾の速度  →65
            bulletSpeed_AI_near = 30.0f
        };

        //inspecterの初期の値(identity)をcurrentIdentityに代入   currentはprotectedでinspecterからは見えない
        usingIdentity = Identity.Null;
        SetIdentity(currentIdentity);

        /*
        personality = new Personality
        {
            //陰湿
            speed = 5f,
            socialDistance   = 30f, // 距離の保ち方（例：0.5f〜10f）
            emergencyDistance= 10f,//   相手の動きの向きが逃げる向きを決定づける距離
            shootCoolTime    = 0.5f,    // 弾を撃つ頻度（例：秒単位のクールタイム）
            intelligence     = 1f,    // 予測力（0〜1）高いほど先読み行動  
            cognition        = 0.7f, //判断力
            shootCoolTimeMin = 0.3f,
            shootCoolTimeMax = 0.9f,
            randomCoolTime   = true,
            moveTimerCheck = 0.5f,
            distanceCheck = 70.0f,//可弾の基準
            bulletSpeed_AI = 60f, // 弾の速度  →65
            bulletSpeed_AI_near = 30.0f,

        };
        */
        /*
        pidInfo = new PIDInfo
        {
            proportal = 1.0f,
            integral  = 0.0f,
            derivative= 0.4f,
            proportal_short = 0.4f,
            integral_short = 0.0f,
            derivative_short = 0.9f,
            proportal_middle = 0.9f,
            integral_middle = 0.0f,
            derivative_middle = 0.6f,
            proportal_long = 1.7f,//1.6
            integral_long = 0.0f,
            derivative_long = 0.3f
        };

        learningAI = new LearningAI
        {
            derWeightMin = 0.3f,//合成微分ベクトルを作るうえで、過去微分ベクトルと直近微分ベクトルの割合の下限(derWeight)
            derWeightMax = 0.8f,
            propWeightMin = 0.3f,
            propWeightMax = 0.8f,
            learningRate = 0.1f,
            derLearningRateMin = 0.01f,
            derLearningRateMax = 0.2f,
            propLearningRateMin = 0.01f,//比例ベクトルをどの程度学習させるか→小さいほうが無難
            propLearningRateMax = 0.2f,
            dataCount = 10,

        };
        */
        //ステートの初期化 アクセスするときは型.変数にする
        position = Position.Stop;

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //性格の更新確認
        if(currentIdentity != usingIdentity)//identity型は大文字の文字列,personality型は小文字の変数名
            SetIdentity(currentIdentity);

        //ベクトル更新　→　弾を撃つ
        //ベクトル更新のクールタイム

        //shootTimerCheckとmoveTimerCheckは同じのほうが無難
        //弾を打つクールタイム
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootTimerCheck)
        {//shootTimerCheckを下げると、うまく追従できなくなる。厳密には、追いつかない →　なぜ　→　短すぎて、うまく予測できなくなっている？　→　弾を撃つ速度だけ、下げる
            shootTimer = 0f;
            shootPos = PID();
            Shoot_AI(shootPos);      
        }

        
        //移動ベクトル計算クールタイム
        moveTimer += Time.deltaTime;
        if(moveTimer >= personality.moveTimerCheck)
        {
            moveTimer = 0f;
            KeepSocialDistance();//headForVector);
            //headForVector = shootVector;
        }

        //中央に戻るべきかのチェック
        //isGrounded =isGroundedCheck.IsGrounded();
        //goBacked   = isGroundedCheck.IsGoBack();
        //Debug.Log($"goBacked:{goBacked}");



        /*色々試したみたところ、TimerとBulletSpeedは同時に調節する必要がある
         * また、Timerを大きくすると、速すぎる＋予測が過剰評価になる　→　bulletSpeedで調節が必要
         * もしくは、PIDの数値を変える
         * いや、全部変えた方がいい
         * 
         * 
         */

    }

    public override void UnderstandSituation()//移動　攻撃　判断
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


        //エリア外かエリア内かどうかの確認
        //isGroundCheck.isGround();
        //isGroundCheck.isOutOfArea();


    }

    public override void KeepSocialDistance()//Vector3 headForVector) 
    {//shootVectorは次の相手の予測位置　ここは一旦保留 PID内でこの関数を呼ぶのも手 shootVector OR varVectorで迷っている
   
        
        //AIの移動ベクトルの基となるPlayerの予測ベクトルはここで更新する
        headForVector = shootVector;

        //1.フィールド上　→追跡or逃走の2パターン

        //playerとの距離とベクトルの角度は比例する   →toAIVectorが基本・shootVectorが予測
        toAIVector  = transform.position - player.transform.position;

        //Stateの処理
        isGrounded = isGroundedCheck.IsGrounded();
        if ((isGrounded == false && toAIVector.magnitude < personality.emergencyDistance) || isGoBacked == true)//場外かつ緊急距離なら中央に戻る
            position = Position.GoBack;
        else if (toAIVector.magnitude > personality.socialDistance || isGrounded == false)//実際の距離が社会距離より大きければ、動かない
            position = Position.Stop;
        else //if (toAIVector.magnitude < personality.socialDistance && )
            position = Position.Escape;

        
        //Debug.Log($"position:{position}");

        /*
        //ベクトル同士の相違度を調べる   相違度から方向ベクトルを算出することはできない
        float dot   = Vector3.Dot(shootVector.normalized, toAIVector.normalized);//AIまでのベクトルとPlayerの予測ベクトルの内積
        //dotProportion :dotをどの程度ベクトル作成に反映するかどうか   ↓以下の式は　AIVector.magnitude > socialDistanceであることが条件
        float dotProportion = Mathf.Pow(1f / socialDistance, 2) * Mathf.Pow(toAIVector.magnitude - socialDistance, 2);//dotProportion = ((1/socialDistance)**2)  *  (AIVector.magnitude - socialDistance) ** 2)  
        float reDot = dot * dotProportion;//相違度が大きい/距離が近いほど1 or -1に近くなる
        reDot = -reDot;//反転//(reDot < 0f) ? -reDot : reDot;//0以下ならプラスに変換
        */

        //距離の保ち方と実際の距離を使って、どの程度playerの移動角度を考慮するかどうか、を2次関数で 計算(playerとの距離が近いほど、rotProportionが大きくなる)
        /*
        float rotProportion = Mathf.Pow(1f / personality.socialDistance , 2) * Mathf.Pow(toAIVector.magnitude - personality.socialDistance, 2);
        rotProportion = Mathf.Clamp01(rotProportion); // 安全のため0〜1に制限

        if (toAIVector.magnitude < personality.emergencyDistance)//緊急なら、予測をすべてにする
            rotProportion = 1;
        */
        // 内積から角度を求める）
        /*
        float dot = Vector3.Dot(toAIVector.normalized, headForVector.normalized);
        dot = Mathf.Clamp(dot, -1f, 1f); // 浮動小数の誤差対策
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;//現状180度しか拾えてないので、符号が消えている　　　-が+に変換されている？
        */
        /*
        float signedAngle = Vector3.SignedAngle(headForVector, toAIVector, Vector3.up);//angleからsigndeAngleに変更


        
        // Quaternionを使って、toAIVecotrを線対称にshootVectorの対称方向への方向単位ベクトル(rotVector)の作成
        Quaternion rot = Quaternion.AngleAxis(signedAngle, Vector3.up);
        Vector3 rotVector = rot * toAIVector.normalized;
        rotVector = rotVector * headForVector.magnitude;//作成したrotatedVector(方向単位ベクトル)を元々の大きさにする


        Debug.DrawRay(transform.position,rotVector  * 20f, Color.green, 0.5f);
        */
        //Debug.Log(rotProportion);
        //Debug.Log(toAIVector.magnitude);//現状　距離12で0.1

        //toAIVector = toAIVector.normalized;
        //escapeVector = (toAIVector * (1 - rotProportion)) + (rotVector * (rotProportion));//全体の割合を１


        //近くにいる状態から、Playerが逃げると追いかけるベクトルになるのを修正したい

        //2.フィールド端
        //3.フィールドの四隅
        //はしになったら、一旦苦るのをやめる＆emergency以下になったら中央に戻る
        //現在どの位置にいるのか

        //Debug.DrawRay(transform.position,rotVector  * 5f, Color.red, 0.5f);

        //if (toAIVector.magnitude < personality.socialDistance)
            //Debug.Log("射程内に入りました");

        switch (position) 
        {
            case Position.Escape:
                if(toAIVector.magnitude < personality.socialDistance - 0.5f)//保険のための0.5
                {
                    float rotProportion = Mathf.Pow(1f / personality.socialDistance, 2) * Mathf.Pow(toAIVector.magnitude - personality.socialDistance, 2);//10・20=0.25  5・15=0.56 
                    rotProportion = Mathf.Clamp01(rotProportion); // 安全のため0〜1に制限
                    //if (toAIVector.magnitude < personality.emergencyDistance)//緊急なら、予測をすべてにする
                        //rotProportion = 1;
                    float signedAngle = Vector3.SignedAngle(headForVector, toAIVector, Vector3.up);//angleからsigndeAngleに変更
                    // Quaternionを使って、toAIVecotrを線対称にshootVectorの対称方向への方向単位ベクトル(rotVector)の作成
                    Quaternion rot = Quaternion.AngleAxis(signedAngle, Vector3.up);
                    Vector3 rotVector = rot * toAIVector.normalized;
                    rotVector = rotVector * headForVector.magnitude;//作成したrotatedVector(方向単位ベクトル)を元々の大きさにする

                    Debug.DrawRay(transform.position, rotVector * 20f, Color.green, 0.5f);
                    toAIVector = toAIVector.normalized;
                    escapeVector = (toAIVector * (1 - rotProportion)) + (rotVector * (rotProportion));//全体の割合を１
                    
                    

                }
                else
                {
                    escapeVector = Vector3.zero;
                    //Debug.Log("SocialDistanceを超えています");
                }

                break;

            case Position.Stop:
                escapeVector = Vector3.zero;
                break;

            case Position.GoBack:
                toCenterVector = (Vector3.zero - transform.position).normalized;
                escapeVector = toCenterVector;
               
                //中央に戻る遷移において、中央に戻ったら変更可能にする
                if (isGroundedCheck.IsGoBack() == false)
                    isGoBacked = false;
                else
                    isGoBacked = true;
                
                break;
        
        
        
        }
       
        Debug.DrawRay(transform.position, escapeVector * 20f, Color.blue, 0.5f);
        //Debug.Log($"isGoBacked:{isGoBacked}");
        //Debug.Log($"isGrounded:{isGrounded}");
        //急接近の謎を解明したい
        //SocialDistanceとAimagnitudeが同じになった時に動作がおかしい　

        
        rb.velocity = escapeVector * personality.speed;//ここで、最終移動更新
        //return escapeVector;
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

        if (toPlayerVector.magnitude > personality.distanceCheck)//可弾かどうか
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
            age = (proportal_long - proportal_middle) / (personality.distanceCheck - distance_middle_long);
            century = proportal_long - age * personality.distanceCheck;
            proportal = age * toPlayerVector.magnitude + century;
            //微分
            derivative = derivative_long;
            bulletSpeed_AI_ver2 = personality.bulletSpeed_AI;
        }
        else if (toPlayerVector.magnitude > distance_short_middle)//中距離
        {
            age = (proportal_middle - proportal_short) / (distance_middle_long - distance_short_middle);
            century = proportal_middle - age * distance_middle_long;
            proportal = age * toPlayerVector.magnitude + century;

            derivative = derivative_middle;
            bulletSpeed_AI_ver2 = personality.bulletSpeed_AI;
        }
        else
        {
            //Debug.Log("近距離攻撃に移行します！\n");
            age = (proportal_short - 0) / (distance_short_middle - 0);
            century = proportal_short - age * distance_short_middle;
            proportal = age * toPlayerVector.magnitude + century;
            //proportal = proportal_short;
            derivative = derivative_short;
            bulletSpeed_AI_ver2 = personality.bulletSpeed_AI_near;
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
                transform.LookAt(shootPos);//playerの方を向く

                //ベクトルの作成+normalized
                Vector3 origin = transform.position;
                Vector3 shootDir_AI = (shootPos - origin).normalized;
                Ray ray = new Ray(origin, shootDir_AI);

                /*
                 * SpherePrafab_AIにDestroy関数を入れると、参照元自体を破壊することになる
                 * →おそらく、インスタンス化されたものにのみDestroy関数を適用しないといけない
                */
                GameObject bullet_AI = Instantiate(SpherePrefab_AI, origin, Quaternion.identity);//Quaternionは無回転を表す
                
                // 弾に力を加える   子オブジェクトとかにコライダーを付けると反発して、velocity.yが跳ね上がるので注意
                Rigidbody bulletRb = bullet_AI.GetComponent<Rigidbody>();
                Collider bulletCol = bullet_AI.GetComponent<Collider>();
                bulletRb.velocity = shootDir_AI * bulletSpeed_AI_ver2;
                
                //AIとAIbulletの衝突を無視する
                if (col != null && bulletCol != null)// true を渡すことで、この二つのコライダー間の衝突を無視する
                    Physics.IgnoreCollision(col, bulletCol, true); 
            


                //Debug.Log($"velocity:{bulletRb.velocity}\n");
                //Debug.Log($"ShootPos: {shootPos}\n");
                //Debug.Log($"bullet_AI:{bullet_AI}\n");
                //Debug.Log($"bullet_AI_pos:{bullet_AI.transform.rotation}\n");
                //Debug.Log($" shootDir:{shootDir_AI}");
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

        /*
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
        */

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
        /*
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

    public override void SetIdentity(Identity currentIdentity)//Identityが種類　Personalityが能力
    {
        if (usingIdentity == currentIdentity) return;

        usingIdentity = currentIdentity;//usingは現在適応しているidentity,currentは現在inspecter上のidentity
        
        switch(usingIdentity)
        {
            case Identity.Fierce:
                personality = fierce;
                break;
            case Identity.Timid:
                personality = timid;
                break;
            case Identity.Cunning:
                personality = cunning;
                break;
            default:
                Debug.Log($"usingIdentity:{usingIdentity}");
                break;
        }
    
        Debug.Log($"usingIdentity:{usingIdentity}");
    }


}
