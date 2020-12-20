using Mirror;
using UnityEngine;

namespace FirstGearGames.Mirrors.Assets.FlexNetworkTransforms.Demos
{

    public class Motor3D : NetworkBehaviour
    {
        public float MoveRate = 3f;

        private FlexNetworkTransform _fnt;

        private NetworkIdentity _localPlatform;
        private Vector3? _lastPlatformPosition = null;

        private void Awake()
        {
            _fnt = GetComponent<FlexNetworkTransform>();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

        }

        // Update is called once per frame
        private void Update()
        {
            if (!base.hasAuthority)
                return;

            /* Local movement, nothing special. */
            float horizontal = Input.GetAxis("Horizontal");
            float forward = Input.GetAxis("Vertical");
            Vector3 nextPos = new Vector3(horizontal, 0f, forward) * MoveRate * Time.deltaTime;
            transform.position += nextPos;

            /* Move with platform locally. 
             * This is so the object doesn't slide locally
             * for the owning player. */
            if (_localPlatform != null)
            {
                transform.rotation = _localPlatform.transform.rotation;

                if (_lastPlatformPosition != null)
                {
                    Vector3 diff = (_localPlatform.transform.position - _lastPlatformPosition.Value);
                    diff.y = 0;
                    transform.position += diff;
                }

                _lastPlatformPosition = _localPlatform.transform.position;
            }

            /* Snap to the platform. This is just to keep the owner on
             * the platform locally. */
            Ray ray = new Ray(transform.position + new Vector3(0f, 1f, 0f), -transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
                transform.position = new Vector3(transform.position.x, hit.collider.transform.position.y + 0.5f, transform.position.z);
            
            /* Check to set the platform.
             * Here is where if a platform is hit
             * the value is set to FlexNetworkTransform. 
             * Also notice that when the platform is not hit,
             * I am setting null. */
            ray = new Ray(transform.position + new Vector3(0f, 1f, 0f), -transform.up);
            //If hit.
            if (Physics.Raycast(ray, out hit, 2f))
            {
                _localPlatform = hit.collider.gameObject.GetComponent<NetworkIdentity>();
                _fnt.SetPlatform(_localPlatform);
            }
            //No hit.
            else
            {
                _localPlatform = null;
                _fnt.SetPlatform(null);
                _lastPlatformPosition = null;
            }
        }




    }


}