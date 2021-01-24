using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace SP
{
    public enum SPSurvivalEventType
    {
        LevelStart,
        LevelComplete,
        LevelEnd,
        Pause,
        UnPause,
        PlayerDeath,
        SpawnComplete,
        RespawnStarted,
        RespawnComplete,
        StarPicked,
        GameOver,
        CharacterSwap,
        CharacterSwitch,
        Repaint,
        TogglePause
    }

    public struct SPSurvivalEvent
    {
        SPSurvivalEventType EventType;
        public SPSurvivalEvent(SPSurvivalEventType eventType)
        {
            EventType = eventType;
        }

        static SPSurvivalEvent e;
        public static void Trigger(SPSurvivalEventType eventType)
        {
            e.EventType = eventType;
            //e.OriginCharacter = originCharacter;
            MMEventManager.TriggerEvent(e);
        }
    }
    public enum SPSurvivalState
    {
        INITIAL,
        READY,
        START,
        PAUSE,
        OVER,
        RESULT
    }

    public class SPGameModeSurvival : SPGameMode, MMEventListener<SPSurvivalEvent>
    {
        public SPGameModeSurvival(LevelManager levelManager, Character [] characters) : base(levelManager, characters)
        {
            GameState = new MMStateMachine<GameModeState>(null, false);
        }
        public virtual void OnMMEvent(SPSurvivalEvent e)
        {

        }
    }
}

