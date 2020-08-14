//Put this script on your blue cube.
//F12で飛ぶ

using System.Collections;
using UnityEngine;
using MLAgents;

public class PushAgentBasic : Agent
{
    //新しく定義。CollectObservations()で渡すために
    //Rigidbody rBody;  ⇒m_AgentRb　で定義されてたわ。

    /// <summary>
    /// 地面。
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    public GameObject area;

    /// <summary>
    /// バウンドするエレメントを生成
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;

    PushBlockAcademy m_Academy;

    /// <summary>
    /// ゴールを生成
    /// The goal to push the block to.
    /// </summary>
    public GameObject goal;

    /// <summary>
    ///（ゴールへ押される為の）ブロック生成
    /// The block to be pushed to the goal.
    /// </summary>
    public GameObject block;

    //追加。このエージェントのボール自体の取得するために追加　　★InspectorでTarget項目に対象を選ぶ。
    //public Transform Target;          ここもいらん。　★m_BlockRbがこれ

    ///↓★新規追加項目。　学習性能あげるために、お金の位置もエージェントに渡すために。　→Blockで指定してた、いらんかったｗ
    //public GameObject Okane;

    /// <summary>
    /// ゴールにブロックが到達したときの「検出」処理
    /// Detects when the block touches the goal.
    /// </summary>
    [HideInInspector]
    public GoalDetect goalDetect;

    public bool useVectorObs;

    Rigidbody m_BlockRb;  //cached on initialization  ≒  初期値が入る（キャッシュされる）よ  っていみ？？
    Rigidbody m_AgentRb;  //cached on initialization
    Material m_GroundMaterial; //cached on Awake()
    RayPerception m_RayPer;

    //Ray angles　ブロックの真横を0度として、角度つけてる　　デフォルトの7個から12個に増やそう            ⇒360度兼縦の角度もつけて全天球風に発射する必要あるかも⇒空オブジェに着けて対処する。
    //float[] m_RayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
    float[] m_RayAngles = { 0f, 30f, 60f, 90f, 120f, 150f, 180f, 210f, 240f, 270f, 300f, 330f};
    string[] m_DetectableObjects = { "block", "goal", "wall" };//Tag名。 オブジェクト名がOrigoalでも何ら問題は無かった。

    /// <summary>
    ///成功・失敗で芝生のマテリアルを変更する。　GrounｄRenderer
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    void Awake()
    {
        //ビルドした時解像度がなんか、変にされちゃってて　←多分どっかの値がさんこうにいされてんだけど、めんどいからこのスクリプトで強制的に起動時に上書きする
        Screen.SetResolution(1920, 1080, true, 60);
        m_Academy = FindObjectOfType<PushBlockAcademy>(); //cache the academy
    }

    //初期化処理
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        goalDetect = block.GetComponent<GoalDetect>();
        goalDetect.agent = this;
        m_RayPer = GetComponent<RayPerception>();

        // Cache the agent rigidbody
        m_AgentRb = GetComponent<Rigidbody>();
        // Cache the block rigidbody
        m_BlockRb = block.GetComponent<Rigidbody>();
        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        //レンダラーを取得しておくことで、ゴールした時変えれるようにする
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        //最初のは初期のマテリアルを読み込むだけでいい
        // Starting material 
        m_GroundMaterial = m_GroundRenderer.material;

        SetResetParameters();
    }

    //Ray 光線の設定            ★★★ここで、Vector ObservationのVector Sizeが決まる。　デフォルトは７０　　→今合計６＋３＋１2０の１2９
    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            var rayDistance = 7f; //ここで70㎝　前はここで角度とか設定してたのに。上へうつってる。　　

            //Brainに与える情報。　Percive？　知覚だって。　球が近づいたか、RayPerception3Dのスクリプト内で反射時間が短ければ、で判断してるんだろうなきっと。
            //ここで、Vector ObservationのSpace size、70個になる。光線の数７ｘ（識別タグ”Wall”とか３　＋新しく返す情報２）　ｘ2行（光線2つ分）⇒７０　　光線3本の105にしてもっと学習増やせれる。⇒120個にした
            AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, m_DetectableObjects, 0f, 0f));
            AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, m_DetectableObjects, 1f, 0f));

            //ここ、球の位置や速さも与えたほうがいいのではないか？
            //位置は間違いなく、あったほうがいい。無駄に端っこにいかなくなるはず。　実装してみるか　　　　　　ここで、まずxyzのそれぞれの次元が渡るから６こ。
            AddVectorObs(m_BlockRb.position);
            AddVectorObs(this.transform.position);

            //★Vector　おぶじぇくとじゃなくて、Vector Observationに【Vector】を加える（Add）という意味だったんだ！！！
            AddVectorObs(m_AgentRb.velocity);

        }
    }

    /// <summary>
    /// ランダムな位置生成をピック？するために地面のバウンド？をつかう
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>


    //マップ位置のランダムな位置取得・生成 （Areaにまるごと適用される）　⇒　★マップ変わんなかったマジか ⇒ただPivotがズレて回転してるだけだった。だから4パターンしかなかった
    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            //★ここで「「ボール」」のX座標とZ座標、位置ランダムに決めてる　147行目の第１引数と第3引数　固定したからここ5行消してもOK       
            var randomPosX = Random.Range(-areaBounds.extents.x * m_Academy.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_Academy.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * m_Academy.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_Academy.spawnAreaMarginMultiplier);
            //・・・・・・・・・・読める！！！！ ★ボールの位置リセットしてる。 ここで高さ固定になってるじゃあねーか！！！マップも複数で魅せたいが、ここのxとz固定したらボールと金も固定されちゃうんじゃ。⇒大丈夫だった
            randomSpawnPos =  ground.transform.position + new Vector3(randomPosX, 5f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.1f, 2.5f)) == false)//このif文のyの値は、0よりうえならいい、って意味かも
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    //ゴールした時の処理
    /// Called when the agent moves the block into the goal.
    /// </summary>

    public void ScoredAGoal()
    {
        //報酬を獲得させる      ←Ispectorのほうで決まってるMax_steps（今は５０００）に到達すると、リセットされて報酬が与えられない？　≒　決められた時間内に、という定義になる。あと精度を決めるのは報酬の数値バランスとRayの本数、向き？
        // We use a reward of 5.⇒1に変更。   v0.6の記事はAddRewardからSetRewardに変わった、ってあったけど　→多分両方つかえる
        AddReward(1.0f);

        //★☆↓　ここ、ゴールしたらDoneだが、　タイムアップで報酬マイナス付与させるDoneも、追加できそうだよな。AgentAction関数内に。

        //自動的にAgentResetメソッドが完了として呼び出され、エージェントを再生成。
        // By marking an agent as done AgentReset() will be called automatically.
        Done();

        // Swap ground material for 「a bit」 to indicate we scored.　　　　★「a bit」が0.5fか。0.3fに地面の色が変わる時間の長さ変更してたけど物足りなくて0.6にした。
        StartCoroutine(GoalScoredSwapGroundMaterial(m_Academy.goalScoredMaterial, 0.6f));//★Academyオブジェクトの、Inspector欄の「Academy」ScriptのgoalScoredMaterialっていう項目に、飛んでた。
    }

    /// <summary>
    ///★ここだ。ゴールしたとき、2秒待機してる。2秒？？　それから元のマテリアルに戻してる　↓で定義した関数を↑で呼び出してる。
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }

    /*
     *
     * ★↓ここコメントアウト。これで、 ぶったいのうごかしかた  は【オレが支配】する。
     * 
     * 
    /// <summary>
    ///選択したアクションによって、エージェントを動かす Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        //ここが謎だったが、そうか。★多分ここでUse Heuristicの値を取得してた ⇒その後下で条件分岐させてる
        //Mathf.FloorToIntで、act[0]以下で最大の整数値を取得してる、このactはこのMoveAgent関数に渡された引数。  ⇒この下でVectorActionが「float[] act」として渡されてる。Switchでint型を扱う(floatじゃ多分だめなんだ)ために、ここで変換してる。
        var action = Mathf.FloorToInt(act[0]);

        //slightly；わずかにゴールとストライカーズは異なる行動範囲を持つ←？  あーー・・判定にズレがあるってこと？
       //Switch文で切り替え↓
        // Goalies and Strikers have slightly different action spaces.
        switch (action)
        {
            //前後ろ、右左、上下の6か所でしょ。これつくったやつコピペして貼った疑惑出てきたな。だってcase３と４いらないだろ。
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                //あーー！「前へー１進む」で、後ろに
                dirToGo = transform.forward * -1f;
                break;
                //これで上に上がる （向き？）                      ⇒    Inspectorのほうで固定させて無効化させてるんだよな。入力受け取る初期仕様書いとかないと、エラー起こるとか？？
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                //右に「-0.75回る」で、左に動くようにしてるのか！！これは、このtoransformは角度なんだろうか⇒RotationとPosition多分両方混ざってる  ⇒でも Use Heuristicで０～４までしかない。case５，６にきてるの？？
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * m_Academy.agentRunSpeed,
            ForceMode.VelocityChange);
    }
    */  
    
    //★ここオリジナル改変     うおおAct２ーーーーーーーッッッ！！
    public void MoveAgent(float[] act)
    {
        var tate = act[0];
        var yoko = act[1];

        // カーソルキーの入力に合わせて移動方向を設定  空中へ行かないようにｙは０
        var movement = new Vector3(tate, 0, yoko);

        ///★★★ここ、恐らくVector ActionのSpace size！！！このスクリプトをつけてるAgentに、行動結果として返すベクトルの次元数(要素数)　はここで決まってる・・気がする！　ただ、new Vector3()の次元数を１としてるのか３としてるのかは不明！！
        // Ridigbody に力を与えて玉を動かす    ★ここ弄るとjobcrown球の速度変更　　　　　　　　　　　　　　　　　　　　　　　　                             AddForceではmovementとして1つで渡してるし。
        m_AgentRb.AddForce(movement * 1, ForceMode.VelocityChange);

    }

    /// <summary>
    /// エンジンの全ステップが呼ばれ、エージェントが行動を起こすとこ。                         textActionつかってないのが気になるな。
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {

        //アクションを使っている（？）エージェントを動かす ←仕様[0]~[4]から変えたからこれいらない？？？？？？？？？？？？？？？？？？？？？？？？
        // Move the agent using the action.
        MoveAgent(vectorAction);

        //　↓★追加。罰がへたくそ。
        float distanceToTarget = Vector3.Distance(this.transform.position, m_BlockRb.position);

        /*
        //ballが近づいていない間、罰を与え続ける。　近づく（接触するくらい）と、報酬を足す？⇒いや、Setで０にしよう。　→いや、それじゃプラスに向かわない。プラスの結果を追い続けるはず。マイナスを減らそうとはしない。動き見てる感じ。
        if (distanceToTarget > 1.5f)
        {
            //花京院「罰を与える」　　　めっちゃちょっぴり      ↓★もしかしたら、下で早くしようという取り組みあるからこっちいらんかも。
            AddReward(-0.001f);

        }
        */

        /*
        //★17cm以内に近づいたら報酬上げるよって意味らしい        ★★１ｆ　＝　10㎝　　←段階づけるのは、うまくできないならよろしくないかも。ここはどう頑張っても正の値にならないようにすべき。ｉｆ文でＡｗａｒｄ＜０の時にできるならいいけど。★☆
        if (distanceToTarget < 1.7f)
        {
        //近づくと、0.0001プラスして正の数にさせる。 ←Setだと毎回なって意味がない。Rewardが、ーのときって条件付けたい。　→諦めてAddにかえた。　★☆Rewardの、変数？の呼び方わからん。
        AddReward(0.0001f);

            //さらに近づいた場合 0.1cmレベルこれはもうボール当て続けろって意味　←ここでもまだ＋になってはいけない。
            if(distanceToTarget < 0.1f)
            {
                AddReward(0.001f);
            }
        }
                ーーーーーーーーーやりなおしーーーーーーーーーー　　　無駄に段階分けても、途中で正の値になって「近づくのがゴール」で考えるのをやめてしまう。　わんちゃんここ、ないほうがいけるかも
        */

        //160cm以上離れたら強制終了　→Unityデフォの１ｍCube出して計測した。 1m? Scale1=10cm
        if(distanceToTarget > 16.0f)
        {
                //1回ごとにＲｅｗａｒｄは初期化されてると想定できるから、ここはＳｅｔでいいとおもう。　⇒確実性の為にＡｄｄにした。下の0.05がどれくらいの頻度で付与されるかいまいちわからんからな。
            AddReward(-1f);
            Done();
        }

        //エージェントに直ぐタスクを完遂させるため、各ステップでペナルティが課せられる          ⇒★☆Max_stepsまで、達したらマイナスー１ｆじゃなくて？？　X学習うまく行われてないよなココ。-1fを割る意味も解らん　ifで時間内の条件でー１にすべき
        // Penalty given each step to encourage agent to finish task quickly.                                  ←↑★長引いて到達しても、MaxStep分の―1報酬減らされ続けてるから、早くなろうとするという意味。理解しましたさーせん
        // －１ｆ　⇒ー0.05fで　学習させただけどやっぱ元に戻す
        AddReward(-1f / agentParameters.maxStep);
    }


    //Heuristic操作   ☆ここオリジナル！！  
    public override float[] Heuristic()
    {
        var action = new float[2];

        // カーソルキーの入力を取得 
       action[0] = Input.GetAxis("Horizontal");
       action[1] = Input.GetAxis("Vertical");

        //縦横、二つの入力がこれで返される
        return action;

        /*
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { 3 };
        }
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { 1 };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { 4 };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { 2 };
        }
        return new float[] { 0 };
        */
    }

    /// <summary>
    /// ブロックの位置と速さをリセット
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlock()
    {
        // Get a random position for the block.
        block.transform.position = GetRandomSpawnPos();

        // Reset block velocity back to zero.
        m_BlockRb.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        m_BlockRb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    //エージェントとブロックのリセット処理  AgentResetクラス内で、定義したResetBlock()も呼び出される。
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void AgentReset()
    {
        //東西南北ランダムの向きを向かせる2行
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        //Areaの向きも、東西南北の「ｙ軸」のみ回転
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        ResetBlock();
        transform.position = GetRandomSpawnPos();
        
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;

        //パラメータのリセット
        SetResetParameters();
    }

    //地面の摩擦定義。
    public void SetGroundMaterialFriction()
    {
        var resetParams = m_Academy.resetParameters;

        var groundCollider = ground.GetComponent<Collider>();

        groundCollider.material.dynamicFriction = resetParams["dynamic_friction"];
        groundCollider.material.staticFriction = resetParams["static_friction"];
    }


    //★Scaleがあるのここだけ。
    public void SetBlockProperties()
    {
        var resetParams = m_Academy.resetParameters;


        ///　500万円のスケールがずれるから、無効化
        //ここで、ブロックのサイズの処理が記述されてる。　工藤さんと話し合ったが、サイズ代わる処理があるわけではないので、不要。　余分なコードだった
        //Set the scale of the block                            第一引数と第三引数が」Resetなんたらになってたのを、固定値に変更。   高さは２.２くらいでいいか ⇒高さじゃない。Scale
        //m_BlockRb.transform.localScale = new Vector3(4.58f, 1.43f, 2.18f);

        // Set the drag of the block
        m_BlockRb.drag = resetParams["block_drag"];
    }

    public void SetResetParameters()
    {
        SetGroundMaterialFriction();
        SetBlockProperties();
    }
}
