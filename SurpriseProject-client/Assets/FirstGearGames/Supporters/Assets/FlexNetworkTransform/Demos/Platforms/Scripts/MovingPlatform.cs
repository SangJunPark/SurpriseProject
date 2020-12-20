using Mirror;
using UnityEngine;


namespace FirstGearGames.Mirrors.Assets.FlexNetworkTransforms.Demos
{


    public class MovingPlatform : NetworkBehaviour
    {
        public float MaxChangeTime = 2f;
        public Vector3 _movementRange;
        public Vector3 _rotationRange;
        public float MoveRate = 6f;
        public float RotRate = 60f;

        private Vector3 _startPos;

        private float _nextRotTime = 0f;
        private float _nextPosTime = 0f;

        private Quaternion _rotGoal;
        private Vector3 _moveGoal;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _startPos = transform.position;
        }


        private void Update()
        {
            if (!base.isServer)
                return;

            if (Time.time > _nextRotTime)
            {
                _nextRotTime = Time.time + Random.Range(0.5f, MaxChangeTime);
                _rotGoal = Quaternion.Euler(RandomV3(_rotationRange));
            }
            if (Time.time > _nextPosTime)
            {
                _nextPosTime = Time.time + Random.Range(0.5f, MaxChangeTime);
                _moveGoal = _startPos + RandomV3(_movementRange);
            }

            transform.position = Vector3.MoveTowards(transform.position, _moveGoal, Time.deltaTime * MoveRate);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _rotGoal, Time.deltaTime * RotRate);
        }

        private Vector3 RandomV3(Vector3 range)
        {
            return new Vector3(
                Random.Range(-range.x, range.x),
                Random.Range(-range.y, range.y),
                Random.Range(-range.z, range.z)
                );
        }
    }


}