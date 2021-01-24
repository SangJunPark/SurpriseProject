using System.Collections;
using System.Collections.Generic;
using Mirror.Cloud.Examples.Pong;
using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace SP
{
    public class SPLevelManager : LevelManager, MMEventListener<SPGameEvent>
    {
        SPGameMode GameMode;
        protected bool _gameOver = false;

        protected override void Start()
        {
            BoundsCollider = _collider;
            InstantiatePlayableCharacters();

            if (UseLevelBounds)
            {
                MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider);
            }

            if (Players == null || Players.Count == 0)
            {
                Players.AddRange(GameObject.FindObjectsOfType<Character>());
            }

            if (Players == null || Players.Count == 0) { return; }

            Initialization();

            // we handle the spawn of the character(s)
            if (Players.Count == 1)
            {
                SpawnSingleCharacter();
            }
            else
            {
                SpawnMultipleCharacters();
            }

            CheckpointAssignment();

            // we trigger a fade
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

            // we trigger a level start event
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
            MMGameEvent.Trigger("Load");

            
            MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
            MMGameEvent.Trigger("CameraBound");
            SPGameEvent.Trigger(SPGameEventType.LevelStart, null);

        }

        protected override void Initialization()
        {
            base.Initialization();
            GameMode = new SPGameModeSurvival(this, null);
        }

        //protected override void OnPlayerDeath(Character playerCharacter)
        //{
        //    base.OnPlayerDeath(playerCharacter);
        //    int aliveCharacters = 0;
        //    int i = 0;

        //    foreach (Character character in LevelManager.Instance.Players)
        //    {
        //        if (character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead)
        //        {
        //            WinnerID = character.PlayerID;
        //            aliveCharacters++;
        //        }
        //        i++;
        //    }

        //    if (aliveCharacters <= 1)
        //    {
        //        StartCoroutine(GameOver());
        //    }
        //}


        /// <summary>
        /// OnDisable, we start listening to events.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
            this.MMEventStartListening<SPGameEvent>();
        }

        /// <summary>
        /// OnDisable, we stop listening to events.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
            this.MMEventStopListening<SPGameEvent>();
        }


        protected virtual IEnumerator GameOver()
        {
            yield return new WaitForSeconds(2f);
            //if (WinnerID == "")
            //    WinnerID = "Player1";
            //{
            //}
            //MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
            //_gameOver = true;
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
        }

        public virtual void Update()
        {
            //CheckForGameOver();
        }

        protected virtual void CheckForGameOver()
        {
            if (_gameOver)
            {
                if ((Input.GetButton("Player1_Jump"))
                    || (Input.GetButton("Player2_Jump"))
                    || (Input.GetButton("Player3_Jump"))
                    || (Input.GetButton("Player4_Jump")))
                {
                    //MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
                    //LoadingSceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        public virtual void OnMMEvent(SPGameEvent gameEvent)
        {
            switch (gameEvent.EventType)
            {
                case SPGameEventType.LevelStart:
                    //SPSurvivalEvent.Trigger(SPSurvivalEventType.LevelStart);
                    StartCoroutine(GameMode.IGameReady());
                    break;
            }
        }
    }

}
