//Every scene needs an academy script.
//Create an empty gameObject and attach this script.
//The brain needs to be a child of the Academy gameObject.

using UnityEngine;
using MLAgents;

public class PushBlockAcademy : Academy
{
    /// <summary>
    /// エージェントの歩くスピード
    /// The "walking speed" of the agents in the scene.
    /// </summary>
    public float agentRunSpeed;

    /// <summary>
    /// エージェントの回転速度
    /// The agent rotation speed.
    /// Every agent will use this setting.
    /// </summary>
    public float agentRotationSpeed;

    /// <summary>
    /// �}�[�W���E�E�󔒂̏搔�A�G���A�𐶐��H�H
    /// The spawn area margin multiplier.
    /// �G���A�̂X�O�����E�E������H
    /// ex: .9 means 90% of spawn area will be used.
    /// .1 margin will be left (so players don't spawn off of the edge).
    /// ���̒l��������΍����قǁA�g���[�j���O�ɗv���鎞�Ԃ������Ȃ�
    /// The higher this value, the longer training time required.
    /// </summary>
    public float spawnAreaMarginMultiplier;

    /// <summary>
    /// ゴールした時これに切り替える
    /// When a goal is scored the ground will switch to this
    /// material for a few seconds. �F�ς��
    /// </summary>
    public Material goalScoredMaterial;

    /// <summary>
    /// エージェントが失敗したら、数秒この失敗マテリアルに地面を塗りかえる
    /// When an agent fails, the ground will turn this material for a few seconds.
    /// </summary>
    public Material failMaterial;

    /// <summary>
    /// �d�͂̏搔�B ���炩�ɂ��邽�߂Ɂ`�R�ȉ������B  �˒T�������ǂǂ��ɂR�Ȃ�Ă���񂾁B
    /// The gravity multiplier.  Gravity Multiplier	 �F �����}�l�[�W���[�Őݒ肳��Ă���d�͒l�̑傫���B�l��0�Ȃ�Ζ��d�͂ɂȂ�B�����}�l�[�W���Q�Ƃ��Ă�̂�
    /// Use ~3 to make things less floaty　　　　３まで？？？ん？？？
    /// </summary>
    public float gravityMultiplier;

    void State()
    {
        Physics.gravity *= gravityMultiplier;
    }
}
