using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using InformationNamespace;
using EventArgumentNamespace;

public class HundHideManager : MonoBehaviour {
    public Hund hund;
    private Material hundMat;
    public Player player;
    public Animator[] lightAnimators;
    public ParticleSystem[] lightParticles;
    public Animator[] wallTrapAniamtor;
    public CamTarget camTarget;
    public Transform[] maskSets;
    private Vector3[] maskSetOriginSizes;
    public SpriteRenderer[] darkBackgroundSets;
    public Timer maskSizeTimer;
    public Timer maskAttackAddtionTimer;
    public AnimationCurve maskSizeAdditionCurve;
    public float maskAttackAddition = 0f;
    public float maskSizeMultiplier = 1f;

    public ParticleSystem hundDisappearParticle;
    public ParticleSystem hundDarkdustParticle;

    //  -1: 현재 밝음
    //  0 : Now HIDE (NOT ACTIVE)
    //  1 : DarkRun
    //  2 : SuddenAttack
    //  3 : Normal Chain Attack
    public int hidePhase = -1;

    public Timer darkRunCoolTimer = new Timer { duration = 5f };
    public Timer suddenAttackCoolTimer = new Timer { duration = 3f };
    public int darkRunLoopCount = 1;

    public ElectricGenerator[] generators;

    public UnityEvent onLightOn;
    public UnityEvent onLightOff;
    public bool init = false;
    public void Init()
    {
        init = true;
        hundMat = hund.mat;
        maskSetOriginSizes = new Vector3[maskSets.Length];
        for (int i = 0; i < maskSets.Length; i++)
        {
            maskSetOriginSizes[i] = maskSets[i].localScale;
        }
        maskAttackAddition = 0f;

        player.fsm.GetState<AttackState>().OnEnter += delegate ()
        {
            maskAttackAddtionTimer.Reset();
        };
        hund.hundFsm.chaseState.OnEnter += delegate()
        {
            if (hund.phaseIndex == 3)
            {
                hidePhase = 3;
                hund.SetUnbeatable(false);
                hund.GetComponent<MeshRenderer>().enabled = true;
                hund.PlaySound (AudioType.HUND_APPEAR);
                hundDisappearParticle.Play();
            }
        };
        hund.hundFsm.tackleState.OnEnter += delegate()
        {
            if (hund.phaseIndex == 3)
            {
                hidePhase = 3;
                hund.SetUnbeatable(false);
                hund.GetComponent<MeshRenderer>().enabled = true;
                hund.PlaySound (AudioType.HUND_APPEAR);
                hundDisappearParticle.Play();
            }
        };
        hund.hundFsm.stopState.OnEnter += delegate()
        {
            if (hund.phaseIndex == 3)
            {
                hidePhase = 0;
                hund.SetUnbeatable(false);
                hund.GetComponent<MeshRenderer>().enabled = true;
                hund.PlaySound (AudioType.HUND_APPEAR);
                hundDisappearParticle.Play();
            }
        };
        hund.hundFsm.darkRunState.OnEnter += delegate()
        {
            if (hund.phaseIndex == 3)
            {
                hund.SetUnbeatable(false);
                hund.GetComponent<MeshRenderer>().enabled = true;
                hund.PlaySound (AudioType.HUND_APPEAR);
                hundDisappearParticle.Play();
            }
        };
        hund.fsm.GetState<HundDarkRunEndState>().OnEnter += delegate ()
        {
           
        };
        hund.fsm.GetState<HundDarkRunEndState>().OnExit += delegate ()
        {
            hundDarkdustParticle.Stop();
            hundDisappearParticle.Play();
            if (hund.phaseIndex == 1)
            {
                hund.PlaySound (AudioType.HUND_HIDE);
                hund.gameObject.SetActive(false);
            }
            else if (hund.phaseIndex == 3)
            {
                hund.SetUnbeatable(true);
                hund.PlaySound (AudioType.HUND_HIDE);
                hund.GetComponent<MeshRenderer>().enabled = false;
            }
            hidePhase = 0;
            darkRunCoolTimer.Reset();
        };
        hund.fsm.GetState<HundSuddenAttackEndState>().OnExit += delegate()
        {
            hundDarkdustParticle.Stop();
            hundDisappearParticle.Play();
             if (hund.phaseIndex == 1)
            {
                hund.PlaySound (AudioType.HUND_HIDE);
                hund.gameObject.SetActive(false);
            }
            hidePhase = 0;
            suddenAttackCoolTimer.Reset();
        };
    }
    public void OnPlayerStayInGeneratorArea()
    {
        if (hund.phaseIndex == 1)
        {
            suddenAttackCoolTimer.IncTimer(Time.deltaTime);
        }
    }
    public void Update()
    {
        if (!init)
            return;
        if (hund.phaseIndex == 1)
        {
            //  DarkRun Update
            darkRunCoolTimer.IncTimer(Time.deltaTime);
            if (darkRunCoolTimer.CheckTimer() && hidePhase == 0)
            {
                hidePhase = 1;
                hund.fsm.BreakStateChain();
                hundMat.SetFloat("_BlendAmount", 1f);
                hundDarkdustParticle.Play();
                hundDisappearParticle.Play();
                hund.gameObject.SetActive(true);
                hund.PlaySound (AudioType.HUND_APPEAR);
                hund.fsm.StartStateChain("DarkRun", new List<ActionState>()
                {
                    hund.hundFsm.darkRunState,
                    hund.hundFsm.darkRunEndState
                });
                //darkRunCoolTimer.Reset();
            }
            else if (suddenAttackCoolTimer.CheckTimer() && hidePhase == 0)
            {
                hidePhase = 2;
                hund.patternIndex = 0;
                hundMat.SetFloat("_BlendAmount", 1f);
                hund.gameObject.SetActive(true);
                hund.PlaySound (AudioType.HUND_APPEAR);
                var destPos = hund.transform.position;
                destPos.x = player.transform.position.x;
                hund.transform.position = destPos;
                hundDarkdustParticle.Play();
                hundDisappearParticle.Play();
                hund.nowPattern = EnemyAIHelper.GetPatternInfo("급습",hund);
                hund.skillCoolTimer.MakeFullCharge("급습");
                hund.hundFsm.attackState.EditStateInfo(EnemyAIHelper.GetSkillInfo("급습",hund));
                hund.fsm.StartStateChain("SuddenAttack", new List<ActionState>(){
                    hund.hundFsm.attackState,
                    hund.hundFsm.suddenAttackEndState
                });
            }
        }
        else if (hund.phaseIndex == 3)
        {
            if (EnemyAIHelper.HundAIHelper.IsNormal)
            {
                //  DarkRun Update
                darkRunCoolTimer.IncTimer(Time.deltaTime);
                if (darkRunCoolTimer.CheckTimer() && hidePhase == 0)
                {
                    hidePhase = 1;
                    hund.fsm.BreakStateChain();
                    hundMat.SetFloat("_BlendAmount", 1f);
                    hundDarkdustParticle.Play();
                    hundDisappearParticle.Play();
                    hund.gameObject.SetActive(true);
                    hund.fsm.StartStateChain("DarkRun", new List<ActionState>()
                    {
                        hund.hundFsm.darkRunState,
                        hund.hundFsm.darkRunEndState
                    });
                    //darkRunCoolTimer.Reset();
                }
                else if (suddenAttackCoolTimer.CheckTimer() && hidePhase == 0)
                {
                    hidePhase = 2;
                    hund.patternIndex = 0;
                    hundMat.SetFloat("_BlendAmount", 1f);
                    hund.gameObject.SetActive(true);
                    var destPos = hund.transform.position;
                    destPos.x = player.transform.position.x;
                    hund.transform.position = destPos;
                    hundDarkdustParticle.Play();
                    hundDisappearParticle.Play();
                    hund.nowPattern = EnemyAIHelper.GetPatternInfo("급습",hund);
                    hund.skillCoolTimer.MakeFullCharge("급습");
                    hund.hundFsm.attackState.EditStateInfo(EnemyAIHelper.GetSkillInfo("급습",hund));
                    hund.fsm.StartStateChain("SuddenAttack", new List<ActionState>(){
                        hund.hundFsm.attackState,
                        hund.hundFsm.suddenAttackEndState
                    });
                }
            }
        }
        //  빛 마스크 Update
        maskSizeTimer.IncTimer(Time.deltaTime);
        if (!maskAttackAddtionTimer.CheckTimer())
        {
            maskAttackAddition = Mathf.Lerp(0.3f, 0f, maskAttackAddtionTimer.GetRatio());
            maskAttackAddtionTimer.IncTimer(Time.deltaTime);
        }
        var curveEval = maskSizeAdditionCurve.Evaluate(maskSizeTimer.GetRatio());
        curveEval *= maskSizeMultiplier;
        curveEval += maskAttackAddition;
        for (int i = 0; i < maskSets.Length; i++)
        {
            maskSets[i].localScale = maskSetOriginSizes[i] + Vector3.one * curveEval;
        }
        if (maskSizeTimer.CheckTimer())
        {
            maskSizeTimer.Reset();
        }
    }
    public void OnGeneratorFixed()
    {
        for (int i = 0; i < generators.Length; i++)
        {
            if (!generators[i].IsWork)
                return;
        }
        OnLightOn();
    }
    public void OnLightOn()
	{
        camTarget.EditFollowWeight(0,0.5f);
        EnemyAIHelper.ResetPattern("쇼크웨이브",hund);
        EnemyAIHelper.ClearPatternBuffer(hund);
        StartCoroutine(ILightOnStart());
    }
	public void OnLightOff()
	{
        if (hund.phaseIndex == 0)
        {
            hund.OnPhaseChanged(1);
            camTarget.EditFollowWeight(0,0);
        }
        //EnemyAIHelper.HundAIHelper.IsLightOFF = true;
		Debug.Log("Light OFF");
        StartCoroutine(ILightOffStart());
        //hund.gameObject.SetActive(false);
	}

    public IEnumerator ILightOffStart()
    {
        GameSystemService.Instance.camManager.ChromaticEffectOn(0.5f);
        GameSystemService.Instance.camManager.VibrateCameraByAttack(0.5f, 12, 0.5f);
        Timer timer = new Timer { duration = 0.3f };
        lightParticles[0].Play();
        lightAnimators[0].Play("Light_Off");
        hund.PlaySound(AudioType.GLASS_BROKEN);
        darkBackgroundSets[0].gameObject.SetActive(true);
        Color destColor = new Color(0f, 0f, 0f, 0.4275f);
        Color originColor = new Color(0f, 0f, 0f, 0f);
        while(!timer.CheckTimer())
        {
            hundMat.SetFloat("_BlendAmount",timer.timer);
            darkBackgroundSets[0].color = Color.Lerp(originColor, destColor, timer.GetRatio());
            timer.IncTimer(Time.deltaTime);
            yield return null;
        }
        lightParticles[1].Play();
        lightAnimators[1].Play("Light_Off");
        hund.PlaySound(AudioType.GLASS_BROKEN);
        darkBackgroundSets[1].gameObject.SetActive(true);
        originColor = new Color(0f, 0f, 0f, 0.4275f);
        destColor = new Color(0f,0f,0f,0.6509f);
        timer.Reset();
        while(!timer.CheckTimer())
        {
            hundMat.SetFloat("_BlendAmount", timer.timer * 2f + 0.3f);
            darkBackgroundSets[1].color = Color.Lerp(originColor, destColor, timer.GetRatio());
            timer.IncTimer(Time.deltaTime);
            yield return null;
        }
        hundMat.SetFloat("_BlendAmount", 1f);
        lightParticles[2].Play();
        lightAnimators[2].Play("Light_Off");
        hund.PlaySound(AudioType.GLASS_BROKEN);
        darkBackgroundSets[2].gameObject.SetActive(true);
        GameSystemService.Instance.camManager.ChromaticEffectOff(0.3f);
        yield return new WaitForSeconds(0.5f);
        hundDisappearParticle.Play();
        hundDarkdustParticle.Play();
        darkRunCoolTimer.Reset();
        hidePhase = 0;
        if (hund.phaseIndex == 3)
        {
            hund.GetComponent<MeshRenderer>().enabled = false;
            hund.SetUnbeatable(true);
        }
        else
        {
            hund.gameObject.SetActive(false);
            for (int i = 0; i < generators.Length; i++)
            {
                generators[i].GeneratorOff();
            }
        }
        if (onLightOff != null)
        {
            onLightOff.Invoke();
        }
    }
    public IEnumerator ILightOnStart()
    {
        if (hund.phaseIndex == 1)
        {
            for (int i = 0; i < wallTrapAniamtor.Length; i++)
            {
                wallTrapAniamtor[i].Play("ON");
            }
            hund.OnPhaseChanged(2);
        }
        //  Active Wall Trap
        player.RaiseActorWallCollision += (object sender, ActorCollisionEventArg arg) =>{
            SkillInfo wallTrapInfo = EnemyAIHelper.GetSkillInfo("전기벽",hund);
            DamageInfo damInfo = new DamageInfo(hund,player,wallTrapInfo);
            player.OnActorDamaged(damInfo);
            var smashDir = Mathf.Sign(player.transform.position.x - arg.col.transform.position.x);
            player.rigid.AddForce(new Vector3(0.6f * smashDir,0.4f,0f) * 15f, ForceMode.Impulse);
            player.SetLookDirection(smashDir);
        };
        hund.fsm.BreakStateChain();
        hund.fsm.TryTransferAction<IdleState>();
        Timer timer = new Timer { duration = 0.3f };
        lightAnimators[0].Play("Light_On");
        Color destColor = new Color(0f, 0f, 0f, 0.4275f);
        Color originColor = new Color(0f, 0f, 0f, 0f);
        darkBackgroundSets[2].gameObject.SetActive(false);
         hundDarkdustParticle.Play();
        while(!timer.CheckTimer())
        {
            hundMat.SetFloat("_BlendAmount",1f - timer.timer);
            darkBackgroundSets[0].color = Color.Lerp(originColor, destColor, timer.GetRatio());
            timer.IncTimer(Time.deltaTime);
            yield return null;
        }
        darkBackgroundSets[1].gameObject.SetActive(false);
        
        lightAnimators[1].Play("Light_On");
        originColor = new Color(0f, 0f, 0f, 0.4275f);
        destColor = new Color(0f,0f,0f,0.6509f);
        timer.Reset();
        while(!timer.CheckTimer())
        {
            hundMat.SetFloat("_BlendAmount", 1f - (timer.timer * 2f + 0.3f));
            darkBackgroundSets[1].color = Color.Lerp(originColor, destColor, timer.GetRatio());
            timer.IncTimer(Time.deltaTime);
            yield return null;
        }
        for (int i = 0; i < maskSets.Length; i++)
        {
            maskSets[i].localScale = maskSetOriginSizes[i];
        }
        hundDarkdustParticle.Stop();
        darkBackgroundSets[0].gameObject.SetActive(false);
        hundMat.SetFloat("_BlendAmount", 0f);
        lightAnimators[2].Play("Light_Off");
        
        if (hund.fsm.chainName == "DarkRun")
        {
            EnemyAIHelper.ClearPatternBuffer(hund);
            hund.fsm.BreakStateChain();
        }
        hund.nowPattern = null;
        hund.patternIndex = 0;
        hund.gameObject.SetActive(true);
        
        for (int i = 0; i < generators.Length; i++)
        {
            var maxLife = generators[i].actorInfo.GetTotalBasicInfo().maxLife;
            generators[i].actorInfo.SetLife(maxLife);
            generators[i].fsm.TryTransferAction<IdleState>();
        }
        darkRunCoolTimer.Reset();
        hidePhase = -1;
        if (onLightOn != null)
        {
            onLightOn.Invoke();
        }
    }
}
