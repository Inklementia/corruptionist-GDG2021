﻿using EZCameraShake;
using Hand;
using System.Collections;
using UnityEngine;

public class EndlessHandGenerator : MonoBehaviour
{
    [SerializeField] private HandStruct[] hands;

    [SerializeField] private float handMovementInterval = 2;
    [SerializeField] private float handMovementTime = .5f;
    [SerializeField] private float handStayDuration = .5f;
    [SerializeField] private float handSpeedMultiplier = 0.066f;
    [SerializeField] private float blockDurationForMent = 3;
    [SerializeField] private EndlessModeController endlessModeController;
    [SerializeField] private GameObject jailPanelGO;
    [SerializeField] private Animator jailAnimator;


    private int _index;

    [HideInInspector] public bool _isBlocked;
    private bool _isBlockedByMent;
    private bool _isAudioPlayed;
    private bool _canGoBack;
    private bool _canMoveHands = true;

    private float _elapsedMoveTime = 0.0f;
    private float _elapsedWaitTime = 0.0f;
    private float _elapsedBlockTime = 0.0f;

    private float _elapsedHandGeneratorBlockTime = 0.0f;
    private float _handGeneratorBlockTime = 1f;

    // to keep default values unchanged
    private float _handMovementInterval;
    private float _handMovementTime;
    private float _handStayDuration;
    private int _currentDay;

    private AudioManager _audioManager;

    #region Cache

    private static readonly int MoveUp = Animator.StringToHash("MoveUp");

    #endregion

    private void Start()
    {
        _audioManager = FindObjectOfType<AudioManager>();
        jailPanelGO.SetActive(false);
        // to set camera shaker to initial shake value
        CameraShaker.Instance.DefaultPosInfluence = new Vector3(0, 1f, 0f);
        CameraShaker.Instance.DefaultRotInfluence = new Vector3(0, 0, 0);
        _isBlocked = true;

        _handMovementInterval = handMovementInterval;
        _handMovementTime = handMovementTime;
        _handStayDuration = handStayDuration;

        //BOBUR LOOK HERE
        //_currentDay = endlessModeController.currentDay; // load current level here
        //_handMovementInterval = handMovementInterval - (_currentDay * handSpeedMultiplier);
        //_handMovementTime = handMovementTime - (_currentDay * handSpeedMultiplier);
        //_handStayDuration = handStayDuration - (_currentDay * handSpeedMultiplier);
        //lets suppose we do it when game loads once
    }

    private void Update()
    {
        _elapsedMoveTime += Time.deltaTime;

        if (_canMoveHands)
        {
            if (!_isBlocked && !_isBlockedByMent)
            {
                MoveHandForward();
            }

            else if (_isBlockedByMent)
            {
                ShowJail();
            }
        }

        MoveHandBack();
    }

    public void UnblockHandGenerator()
    {
        _isBlockedByMent = false;
        jailPanelGO.SetActive(false);
        _isBlocked = false;
    }

    public void UnblockHandGeneratorAfterWait()
    {
        StartCoroutine(WaitAndUnblock(1f));
    }

    private IEnumerator WaitAndUnblock(float time)
    {
        //hands[_index].handGO.SetActive(true); зачем это?
        yield return new WaitForSeconds(time);
        _isBlocked = false;
    }

    public void BlockHandGenerator()
    {
        _isBlocked = true;
        // hands[_index].handGO.SetActive(false); зачем это? если isblocked он как обычно вернет руку назад 
        //а щас если рука осталась она пропадает по дибильному
        Debug.Log("Should lock generaor");
    }

    public void BlockHandGeneratorByMent()
    {
        _isBlockedByMent = true;
        _isBlocked = true;
  
    }

    public void StopHands()
    {
        Debug.Log("Stop Hands");
        _canMoveHands = false;
    }

    public void MoveHands()
    {
        Debug.Log("Stop Hands");
        _canMoveHands = true;
    }

    private void ShowJail()
    {
        jailPanelGO.SetActive(true);


        if (!_isAudioPlayed)
        {
            // audioSource.Play();
            _audioManager.Play("jail");
            _isAudioPlayed = true;
        }

        _elapsedBlockTime += Time.deltaTime;

        if (_elapsedBlockTime >= blockDurationForMent)
        {
            Debug.Log("MoveUP");
            _elapsedBlockTime = 0.0f;
            //jailAnimator.SetTrigger(MoveUp);
            _isBlocked = false;
        }
    }

    public void DeactivateJail()
    {
        _isBlockedByMent = false;
        jailPanelGO.SetActive(false);
    }

    private void MoveHandForward()
    {
        if (_elapsedMoveTime >= _handMovementInterval && _canMoveHands)
        {
            _canMoveHands = false;
            _isAudioPlayed = false;
            _audioManager.Play("handSwoosh");
            _index = Random.Range(0, hands.Length);
            // go to target
            LeanTween.move(hands[_index].handGO, hands[_index].target.position, _handMovementTime)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnComplete(() => { _canGoBack = true; });

        }
    }

    private void MoveHandBack()
    {
        // wait time
        if (_canGoBack)
        {
            _elapsedWaitTime += Time.deltaTime;

            if (_elapsedWaitTime >= _handStayDuration)
            {
                LeanTween.move(hands[_index].handGO, hands[_index].initialPosition.position, _handMovementTime)
                    .setOnComplete(() =>
                    {
                        hands[_index].cashGO.SetActive(true);
                        hands[_index].cashGO.GetComponent<EndlessCash>().CashCanBeTaken();
                        _canMoveHands = true;
                    });

                _elapsedMoveTime = 0.0f;
                _elapsedWaitTime = 0.0f;
                _canGoBack = false;
            }
        }
    }

    //public void OnLevelUp()
    //{
    //    // accelerates hand movement speed according to current level
    //    // called in level controller
    //    _handMovementInterval -= handSpeedMultiplier;
    //    _handMovementTime -= handSpeedMultiplier;
    //    _handStayDuration -= handSpeedMultiplier;
    //    //this would still work, ты можешь вставить тоже самое что в Старте но я не уверена в порядке: тип что первое? Лвл повышается или ускоряются руки
    //    //probably need to remove this
    //    //0.066f

    //    //_handMovementInterval = handMovementInterval - (currentlevel * 0.066f)
    //    //_handMovementTime = handMovementTime - (currentlevel * 0.066f)
    //    //_handStayDuration = handStayDuration - (currentlevel * 0.066f)

    //    // i put this in start
    //}

    public void SpeedUpHands()
    {
        if(_handMovementInterval <= 0.2f)
        {
            _handMovementInterval = 0.18f;
        }
        else
        {
            _handMovementInterval -= handSpeedMultiplier;
        }


        if(_handMovementTime <= 0.2f)
        {
            _handMovementTime = 0.18f;
        }
        else
        {
            _handMovementTime -= handSpeedMultiplier;
        }


        if(_handStayDuration <= 0.2f)
        {
            _handStayDuration = 0.18f;
        }
        else
        {
            _handStayDuration -= handSpeedMultiplier;
        }
        
    }
}