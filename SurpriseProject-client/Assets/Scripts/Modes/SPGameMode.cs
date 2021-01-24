using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace SP
{
    public enum GameModeState
    {
        INITIAL,
        READY,
        START,
        PAUSE,
        OVER,
        RESULT
    }

    public abstract class SPGameMode
    {
        protected LevelManager Level;
        protected MMStateMachine<GameModeState> GameState;

        //Character -> 유저 정보 객체에서 접근하는 식으로 변경 필요.
        protected SPGameMode(LevelManager levelManager, Character [] characters)
        {
            Level = levelManager;
        }

        public bool CheckGameOver()
        {
            GameState.ChangeState(GameModeState.OVER);
            return false;
        }

        public virtual void DoGameReady()
        {
            
        }

        public virtual void DoGameStart()
        {

        }

        public virtual void DoGameOver()
        {

        }

        public IEnumerator IGameReady()
        {
            GameState.ChangeState(GameModeState.INITIAL);

            SPGameEvent.Trigger(SPGameEventType.TogglePause, null);

            var spgui = (SPGUIManager) GUIManager.Instance;
            int c = 6;
            while (c-- > 0)
            {
                spgui.SetCounterText(c.ToString());
                yield return new WaitForSeconds(1f);
            }
            yield return null;
            yield return IGameStart();
        }

        IEnumerator IGameStart()
        {
            SPGameEvent.Trigger(SPGameEventType.TogglePause, null);
            var spgui = (SPGUIManager)GUIManager.Instance;
            spgui.SetCounterText("START");
            yield return new WaitForSeconds(1f);
            spgui.SetCounterText(null);
            yield return null;
        }

        IEnumerator IGamePause()
        {
            yield return null;
        }

        IEnumerator IGameOver()
        {
            yield return null;
        }

        IEnumerator IResult()
        {
            yield return null;
        }
    }

}
